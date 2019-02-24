using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovePerformer))]
public class GameManager : MonoBehaviour
{
    GameState _gameState;

    Coroutine _gameStateCoroutine;

    BoardTile _selectedTile;

    [SerializeField]
    Board _board;

    public static GameManager instance;

    PlayerMovePerformer playerMovePerformer;

    public int minChainLength = 3;

    // Use this for initialization
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        playerMovePerformer = GetComponent<PlayerMovePerformer>();

        _gameState = GameState.PlayerTurn;
    }

    // Update is called once per frame
    void Update()
    {
        PerformGameState();
    }

    private void PerformGameState()
    {
        switch (_gameState)
        {
            case GameState.PlayerTurn:
                
                PerformPlayerTurn();

                break;

            case GameState.MatchAndMoveCycle:

                if (_gameStateCoroutine == null)
                {
                    _gameStateCoroutine = StartCoroutine(PerformMatchAndMoveCycle(playerMovePerformer.LastPerformedSwap));
                }

                break;

            default:
                throw new System.Exception("not handled game state: " + _gameState);
        }
    }

    void PerformPlayerTurn()
    {
        if (_selectedTile == null && Input.GetMouseButtonDown(0))
        {
            SelectTile(_board.GetTileAtMousePosition());

            playerMovePerformer.Initialize(_selectedTile, _board);

            playerMovePerformer.enabled = true;
        }
        else if (_selectedTile != null && !playerMovePerformer.enabled)
        {
            SelectTile(null);

            _gameState = GameState.MatchAndMoveCycle;
        }
    }

    IEnumerator PerformMatchAndMoveCycle(PlayerMovePerformer.Swap lastPerformedSwap)
    {
        List<MatchInfo> matches = new List<MatchInfo>();

        MatchInfo matchTmp;

        Debug.Log("" + lastPerformedSwap.Coord1 + ", " + MatchFinder.FindMatch(lastPerformedSwap.Coord1, _board, minChainLength));
        Debug.Log("" + lastPerformedSwap.Coord2 + ", " + MatchFinder.FindMatch(lastPerformedSwap.Coord2, _board, minChainLength));

        if (MatchFinder.FindMatch(lastPerformedSwap.Coord1, _board, minChainLength, out matchTmp))
        {
            matches.Add(matchTmp);
        }

        if (MatchFinder.FindMatch(lastPerformedSwap.Coord2, _board, minChainLength, out matchTmp))
        {
            matches.Add(matchTmp);
        }

        while (matches.Count > 0)
        {
            // ----------------------------------------------
            // Step 1: Resolve all matches
            foreach(MatchInfo matchInfo in matches)
            {
                foreach(BoardTile tile in matchInfo.Tiles)
                {
                    tile.ChangeState(BoardTile.TileState.match);
                }
            }

            yield return new WaitForSeconds(2F);

            foreach (MatchInfo matchInfo in matches)
            {
                foreach (BoardTile tile in matchInfo.Tiles)
                {
                    _board.DestroyTileAt(tile.Coordinates);
                }
            }

            yield return new WaitForEndOfFrame();

            // ----------------------------------------------
            // Step 2: player moves character


            // ----------------------------------------------
            // Step 3: tiles fall down
            _board.MoveTiles();

            _board.FillEmptySpaces();

            yield return new WaitForEndOfFrame();

            // ----------------------------------------------
            // Step 4: find new matches

            matches.Clear();

            matches = MatchFinder.FindAllMatches(_board, minChainLength);
        }

        _gameStateCoroutine = null;
        _gameState = GameState.PlayerTurn;
    }

    public void SelectTile(BoardTile tile)
    {
        if (_selectedTile != null)
        {
            _selectedTile.ChangeState(BoardTile.TileState.standard);
        }

        if (tile != null)
        {
            tile.ChangeState(BoardTile.TileState.selected);
        }

        _selectedTile = tile;
    }
}
