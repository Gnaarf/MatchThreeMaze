using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    BoardTile[,] _tiles;
    
    [SerializeField]
    BoardTile _tileTemplate;

    public Vector2Int Dimensions;

    [SerializeField]
    Sprite[] _tileSprites;

    public BoardTile activeTile { get; private set; }

    // Use this for initialization
    void Start()
    {
        BuildBoard();
    }

    void BuildBoard()
    {
        // move to center of screen
        transform.position = -(Vector2)Dimensions / 2 + Vector2.one / 2;

        // create 2D array for tiles
        _tiles = new BoardTile[Dimensions.x, Dimensions.y];

        // fill the board
        FillEmptySpaces();
    }

    private void CreateTileAt(int x, int y, Sprite sprite)
    {
        BoardTile newTile = Instantiate(_tileTemplate, this.transform);
        newTile.Initialize(sprite, new Vector2Int(x, y));

        newTile.transform.localPosition = new Vector3(x, y, 0);
        newTile.name += " (" + x + ", " + y + ")";

        _tiles[x, y] = newTile;
    }

    public void MoveTiles(float movementSpeed, int fallDirectionX = 0, int fallDirectionY = -1)
    {
        Vector2Int fallDirection = new Vector2Int(fallDirectionX, fallDirectionY);

        for (int x = 0; x < Dimensions.x; ++x)
        {
            for (int y = 0; y < Dimensions.y; ++y)
            {
                if (_tiles[x, y] == null)
                {
                    Vector2Int currentCoordinates = new Vector2Int(x, y);

                    Vector2Int searchCoordinates = currentCoordinates - fallDirection;

                    while (searchCoordinates.IsBetween(Vector2Int.zero, Dimensions) && GetTileAt(searchCoordinates) == null)
                    {
                        searchCoordinates -= fallDirection;
                    }

                    if (searchCoordinates.IsBetween(Vector2Int.zero, Dimensions))
                    {
                        float movementDuration = Vector2.Distance(currentCoordinates, searchCoordinates) / movementSpeed;

                        SwapTiles(currentCoordinates, searchCoordinates, movementDuration);
                    }
                }
            }
        }
    }

    public void FillEmptySpaces()
    {
        int minChainLength = GameManager.instance.minChainLength;

        for (int x = 0; x < Dimensions.x; ++x)
        {
            for(int y = 0; y < Dimensions.y; ++y)
            {
                if(_tiles[x, y] == null)
                {
                    List<Sprite> possibleSprites = new List<Sprite>(_tileSprites);

                    do
                    {
                        int index = Random.Range(0, possibleSprites.Count);
                        Sprite sprite = possibleSprites[index];

                        System.Predicate<BoardTile> hasMatchingSprite = (t => t != null && BoardTile.TypeToSpriteLookup[t.Type] == sprite);

                        if (minChainLength - 1 > GetMatchingChainInDirection(x, y, Direction.HorizontalBothDirections, hasMatchingSprite, true).Count
                            && minChainLength - 1 > GetMatchingChainInDirection(x, y, Direction.VerticalBothDirections, hasMatchingSprite, true).Count)
                        {
                            CreateTileAt(x, y, sprite);
                        }
                        else
                        {
                            possibleSprites.RemoveAt(index);
                        }
                    }
                    while (GetTileAt(x, y) == null && possibleSprites.Count > 0);

                    if(GetTileAt(x,y) == null)
                    {
                        CreateTileAt(x, y, _tileSprites[Random.Range(0, possibleSprites.Count - 1)]);
                    }
                }
            }
        }
    }
    
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Simple Operations - e.g. swap, get, destroy
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////

    public void SwapTiles(BoardTile tile1, BoardTile tile2)
    {
        SwapTiles(tile1.Coordinates, tile2.Coordinates);
    }

    public void SwapTiles(Vector2Int tile1Coordinates, Vector2Int tile2Coordinates, float visualSwapDuration = 0F)
    {
        BoardTile tile1 = GetTileAt(tile1Coordinates);
        BoardTile tile2 = GetTileAt(tile2Coordinates);

        if (tile1 != null)
        {
            tile1.Coordinates = tile2Coordinates;
            Vector3 target = transform.position + new Vector3(tile2Coordinates.x, tile2Coordinates.y);
            tile1.StartMovingTo(target, visualSwapDuration);
        }

        if (tile2 != null)
        {
            tile2.Coordinates = tile1Coordinates;
            Vector3 target = transform.position + new Vector3(tile1Coordinates.x, tile1Coordinates.y);
            tile2.StartMovingTo(target, visualSwapDuration);
        }

        _tiles[tile1Coordinates.x, tile1Coordinates.y] = tile2;
        _tiles[tile2Coordinates.x, tile2Coordinates.y] = tile1;
    }

    public Vector2Int GetBoardCoordFromWorldCoord(Vector2 worldCoordinates)
    {
        return (worldCoordinates + (Vector2)Dimensions / 2).ConvertToInt();
    }

    public Vector2Int GetBoardCoordFromMousePosition()
    {

        RaycastHit2D hit = Physics2D.Raycast((Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition)), Vector2.zero);
        
        if (hit.transform != null)
        {
            return GetBoardCoordFromWorldCoord(hit.point);
        }

        return new Vector2Int(-1, -1);
    }

    public BoardTile GetTileFromWorldCoordinates(Vector2 worldCoordinates)
    {
        Vector2Int boardCoordinates = GetBoardCoordFromWorldCoord(worldCoordinates);
        return GetTileAt(boardCoordinates.x, boardCoordinates.y);
    }

    public BoardTile GetTileAtMousePosition()
    {
        Vector2Int boardCoordinates = GetBoardCoordFromMousePosition();
        return GetTileAt(boardCoordinates.x, boardCoordinates.y);
    }

    public BoardTile GetTileAt(Vector2Int boardCoordinates)
    {
        return GetTileAt(boardCoordinates.x, boardCoordinates.y);
    }

    public BoardTile GetTileAt(int x, int y)
    {
        return _tiles[x, y];
    }

    public void DestroyTileAt(Vector2Int coordinates)
    {
        DestroyTileAt(coordinates.x, coordinates.y);
    }

    public void DestroyTileAt(int x, int y)
    {
        Destroy(_tiles[x, y].gameObject);
        _tiles[x, y] = null;
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Helper Functions - more complex operations
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////

    public List<BoardTile> FloodFill(Vector2Int startCoordinates)
    {
        // prepare result variable
        List<BoardTile> tilesInMatch = new List<BoardTile>();

        // get relevant information
        BoardTile.TileType matchTileType = GetTileAt(startCoordinates).Type;

        // set up helper data structures
        bool[,] visited = new bool[Dimensions.x, Dimensions.y];

        Vector2Int[] directions = new Vector2Int[] { Vector2Int.right, Vector2Int.up, Vector2Int.left, Vector2Int.down };

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(startCoordinates);

        // perform the actual flood fill
        while (queue.Count > 0)
        {
            Vector2Int currentCoordinates = queue.Dequeue();
            if (!currentCoordinates.IsBetween(Vector2Int.zero, Dimensions) || visited[currentCoordinates.x, currentCoordinates.y] || GetTileAt(currentCoordinates).Type != matchTileType)
            {
                continue;
            }

            BoardTile currentTile = GetTileAt(currentCoordinates);
            visited[currentCoordinates.x, currentCoordinates.y] = true;
            tilesInMatch.Add(currentTile);

            foreach (Vector2Int direction in directions)
            {
                queue.Enqueue(currentCoordinates + direction);
            }
        }

        return tilesInMatch;
    }

    /// <summary>
    /// Gets a chain of tiles that fullfill the matching condition along a given direction.
    /// </summary>
    /// <param name="x">x coordinate of the startTile</param>
    /// <param name="y">y coordinate of the startTile</param>
    /// <param name="direction">search direction(s)</param>
    /// <param name="matchingCondition">matching condition</param>
    /// <param name="excludeStartTile">whether the tile at start coordinates should be included or excluded</param>
    /// <returns>number of consecutive tiles that fullfill the matching condition</returns>
    public List<BoardTile> GetMatchingChainInDirection(int x, int y, Direction direction, System.Predicate<BoardTile> matchingCondition, bool excludeStartTile = false)
    {
        return GetMatchingChainInDirection(new Vector2Int(x, y), direction, matchingCondition, excludeStartTile);
    }

    /// <summary>
    /// Gets a chain of tiles that fullfill the matching condition along a given direction.
    /// </summary>
    /// <param name="startCoordinates">coordinates of the startTile</param>
    /// <param name="direction">search direction(s)</param>
    /// <param name="matchingCondition">matching condition</param>
    /// <param name="excludeStartTile">whether the tile at start coordinates should be included or excluded</param>
    /// <returns>number of consecutive tiles that fullfill the matching condition</returns>
    public List<BoardTile> GetMatchingChainInDirection(Vector2Int startCoordinates, Direction direction, System.Predicate<BoardTile> matchingCondition, bool excludeStartTile = false)
    {
        List<BoardTile> matchingTiles = new List<BoardTile>();

        if (excludeStartTile || matchingCondition(GetTileAt(startCoordinates)))
        {
            if (!excludeStartTile)
            {
                matchingTiles.Add(GetTileAt(startCoordinates));
            }

            foreach (Vector2Int directionVector in DirectionHelper.DirectionToVectors(direction))
            {
                Vector2Int searchCoordinates = startCoordinates + directionVector;

                while (searchCoordinates.IsBetween(Vector2Int.zero, Dimensions) && matchingCondition(GetTileAt(searchCoordinates)))
                {
                    matchingTiles.Add(GetTileAt(searchCoordinates));
                    searchCoordinates += directionVector;
                }
            }
        }

        return matchingTiles;
    }
}
