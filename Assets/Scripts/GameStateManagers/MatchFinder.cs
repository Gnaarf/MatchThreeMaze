using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MatchFinder
{

    public static List<MatchInfo> FindAllMatches(Board board, int minChainLength = 3)
    {
        List<MatchInfo> result = new List<MatchInfo>();

        bool[,] visited = new bool[board.Dimensions.x, board.Dimensions.y];

        for(int x = 0; x < board.Dimensions.x; ++x)
        {
            for (int y = 0; y < board.Dimensions.y; ++y)
            {
                if (!visited[x, y])
                {
                    List<BoardTile> floodFilledTiles = board.FloodFill(new Vector2Int(x, y));

                    foreach (BoardTile tile in floodFilledTiles)
                    {
                        visited[tile.Coordinates.x, tile.Coordinates.y] = true;
                    }

                    MatchInfo match = new MatchInfo(floodFilledTiles, minChainLength);

                    if(match.Size > 0)
                    {
                        result.Add(match);
                    }
                }
            }
        }
        
        return result;
    }

    public static bool FindMatch(Vector2Int startCoordinates, Board board, int minChainLength, out MatchInfo match)
    {
        match = new MatchInfo(board.FloodFill(startCoordinates), minChainLength);

        return match.Size > 0;
    }

    public static bool FindMatch(Vector2Int startCoordinates, Board board, int minChainLength)
    {
        MatchInfo tmp;
        
        return FindMatch(startCoordinates, board, minChainLength, out tmp);
    }
}
