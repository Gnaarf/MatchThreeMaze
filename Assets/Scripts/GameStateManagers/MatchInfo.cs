using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MatchInfo
{
    public List<BoardTile> Tiles;

    public int Size { get { return Tiles.Count; } }

    public BoardTile MostRecentlyMoved { get; private set; }

    public int MinX { get; private set; }
    public int MinY { get; private set; }
    public int MaxX { get; private set; }
    public int MaxY { get; private set; }

    public int longestHorizontalChain { get; private set; }
    public int longestVerticalChain { get; private set; }

    public MatchInfo(List<BoardTile> match)
        : this(match, 0)
    {
    }

    public MatchInfo(List<BoardTile> tiles, int minChainLength)
    {
        ///---------------------------------------------------------///
        /// crop offshoots to make this an legit match
        ///---------------------------------------------------------///
        Tiles = CropOffshootTiles(tiles, minChainLength);

        if(Tiles.Count <= 0)
        {
            MinX = -1;
            MaxX = -1;
            MinY = -1;
            MaxY = -1;

            MostRecentlyMoved = null;

            longestHorizontalChain = -1;
            longestVerticalChain = -1;
        }
        else
        {
            ///---------------------------------------------------------///
            /// step 1: sort & get extreme values
            ///---------------------------------------------------------///

            List<BoardTile> matchSortedByX = new List<BoardTile>(Tiles);
            List<BoardTile> matchSortedByY = new List<BoardTile>(Tiles);

            // sort by move time to get most recently moved tile
            // no need for yet another list, list will be recycled
            matchSortedByX.Sort((b1, b2) => Comparer.Default.Compare(b1.lastTimeOfMovement, b2.lastTimeOfMovement));

            MostRecentlyMoved = Tiles[0];

            // sort by x value to get minX, maxX
            matchSortedByX.Sort((b1, b2) => Comparer.Default.Compare(b1.Coordinates.x, b2.Coordinates.x));

            MinX = matchSortedByX[0].Coordinates.x;
            MaxX = matchSortedByX[matchSortedByX.Count - 1].Coordinates.x;

            // sort by x value to get minX, maxX
            matchSortedByY.Sort((b1, b2) => Comparer.Default.Compare(b1.Coordinates.y, b2.Coordinates.y));

            MinY = matchSortedByY[0].Coordinates.y;
            MaxY = matchSortedByY[matchSortedByY.Count - 1].Coordinates.y;

            ///---------------------------------------------------------///
            /// step 2: use sorted lists to calculate longest chains
            ///---------------------------------------------------------///

            longestHorizontalChain = GetLongestChainInMatch(ref matchSortedByX, Vector2Int.left);
            longestVerticalChain = GetLongestChainInMatch(ref matchSortedByY, Vector2Int.down);
        }
    }

    private static List<BoardTile> CropOffshootTiles(List<BoardTile> tiles, int minChainLength)
    {
        if (minChainLength <= 1)
            return new List<BoardTile>();

        HashSet<BoardTile> result = new HashSet<BoardTile>();

        foreach (BoardTile tile in tiles)
        {
            BoardTile left = tiles.Find(t => t.Coordinates == tile.Coordinates + Vector2Int.left);
            BoardTile right = tiles.Find(t => t.Coordinates == tile.Coordinates + Vector2Int.right);
            BoardTile up = tiles.Find(t => t.Coordinates == tile.Coordinates + Vector2Int.up);
            BoardTile down = tiles.Find(t => t.Coordinates == tile.Coordinates + Vector2Int.down);

            if (left != null && right != null)
            {
                result.Add(tile);
                result.Add(left);
                result.Add(right);
            }

            if (up != null && down != null)
            {
                result.Add(tile);
                result.Add(up);
                result.Add(down);
            }
        }

        return new List<BoardTile>(result);
    }

    private static int GetLongestChainInMatch(ref List<BoardTile> sortedMatchTilesInDirection, Vector2Int direction)
    {
        int maxChainLength = 0;

        while (sortedMatchTilesInDirection.Count > 0)
        {
            int chainLength = 0;

            BoardTile chainStartTile = sortedMatchTilesInDirection[sortedMatchTilesInDirection.Count - 1];
            sortedMatchTilesInDirection.RemoveAt(sortedMatchTilesInDirection.Count - 1);

            BoardTile currentTile = chainStartTile;

            while (currentTile != null)
            {
                chainLength++;
                int currentTileIndex = sortedMatchTilesInDirection.FindIndex((tile) => tile.Coordinates == currentTile.Coordinates + direction);
                if(currentTileIndex >= 0)
                {
                    currentTile = sortedMatchTilesInDirection[currentTileIndex];
                    sortedMatchTilesInDirection.RemoveAt(currentTileIndex);
                }
                else
                {
                    currentTile = null;
                }
            }

            maxChainLength = Mathf.Max(maxChainLength, chainLength);
        }

        return maxChainLength;
    }
}