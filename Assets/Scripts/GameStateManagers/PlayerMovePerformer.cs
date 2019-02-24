using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovePerformer : MonoBehaviour
{
    [SerializeField]
    float swapDuration = 0.5F;
    float swapSpeed { get { return 1F / swapDuration; } }

    public Swap LastPerformedSwap { get; private set; }

    public struct Swap
    {
        public Vector2Int Coord1;
        public Vector2Int Coord2;
        public Swap(Vector2Int coord1, Vector2Int coord2)
        {
            this.Coord1 = coord1;
            this.Coord2 = coord2;
        }
    }

    Board _board;

    BoardTile _selectedTile;
    Vector2Int _selectedTileInitialCoordinates;
    Vector3 _selectedTileInitialPosition;
    
    BoardTile _swapTile;
    Vector2Int _swapDirection;

    SwapState _currentSwapState;

    enum SwapState
    {
        /// <summary>currently performing the swap that moves the selected tile away from its initial position</summary>
        Forward,
        /// <summary>currently performing the swap that moves the selected tile back into its initial position</summary>
        Backward,
        /// <summary>currently the selected tile stays in its initial position</summary>
        None,
    }

    public void Initialize(BoardTile selectedTile, Board board)
    {
        _selectedTile = selectedTile;
        _board = board;
        _selectedTileInitialCoordinates = _selectedTile.Coordinates;
        _selectedTileInitialPosition = _selectedTile.transform.position;

        ResetSwap();
    }

    private void Update()
    {
        if(!Input.GetMouseButton(0)) // player releases mouse button
        {
            if(_currentSwapState == SwapState.Forward)
            {
                _board.SwapTiles(_selectedTile, _swapTile);

                LastPerformedSwap = new Swap(_selectedTile.Coordinates, _swapTile.Coordinates);
            }

            this.enabled = false;
        }

        Vector2Int mouseCoordinates = _board.GetBoardCoordFromMousePosition();

        bool mouseIsOnTheBoard = mouseCoordinates.x > -1;

        Vector2Int mouseDirection = mouseCoordinates - _selectedTileInitialCoordinates;
        mouseDirection.Clamp(new Vector2Int(-1, -1), Vector2Int.one);

        switch (_currentSwapState)
        {
            case SwapState.None:

                if (mouseIsOnTheBoard && (mouseDirection.x != 0 ^ mouseDirection.y != 0))
                {
                    StartForwardSwap(mouseDirection);
                }

                break;


            case SwapState.Forward:
                
                if (mouseIsOnTheBoard && mouseDirection != _swapDirection)
                {
                    StartBackwardSwap();
                }
                else
                {
                    PerformForwardSwap();
                }
                
                break;

            case SwapState.Backward:

                if (mouseIsOnTheBoard && mouseDirection == _swapDirection)
                {
                    StartForwardSwap(_swapDirection);
                }
                if (_selectedTile.transform.position == _selectedTileInitialPosition)
                {
                    ResetSwap();
                }
                else
                {
                    PerformBackwardSwap();
                }

                break;
        }
    }

    private void ResetSwap()
    {
        _swapDirection = Vector2Int.zero;

        _swapTile = null;

        _currentSwapState = SwapState.None;

    }

    private void StartForwardSwap(Vector2Int direction)
    {
        _swapDirection = direction;

        _swapTile = _board.GetTileAt(_selectedTileInitialCoordinates + _swapDirection);

        _currentSwapState = SwapState.Forward;

        PerformForwardSwap();
    }

    private void PerformForwardSwap()
    {
        Vector3 swapDirectionVector3, target;
        float distanceToTarget;

        // move selected tile
        {
            swapDirectionVector3 = new Vector3(_swapDirection.x, _swapDirection.y);
            target = _selectedTileInitialPosition + swapDirectionVector3;
            distanceToTarget = Vector3.Distance(_selectedTile.transform.position, target);

            _selectedTile.transform.position += Vector3.ClampMagnitude(swapDirectionVector3 * swapSpeed * Time.deltaTime, distanceToTarget);
        }

        // move  swap tile
        {
            swapDirectionVector3 *= -1;
            target = _selectedTileInitialPosition;
            distanceToTarget = Vector3.Distance(_swapTile.transform.position, target);

            _swapTile.transform.position += Vector3.ClampMagnitude(swapDirectionVector3 * swapSpeed * Time.deltaTime, distanceToTarget);
        }
    }

    private void StartBackwardSwap()
    {
        _currentSwapState = SwapState.Backward;

        PerformBackwardSwap();
    }

    private void PerformBackwardSwap()
    {
        Vector3 swapDirectionVector3, target;
        float distanceToTarget;

        // move selected tile
        {
            swapDirectionVector3 = -new Vector3(_swapDirection.x, _swapDirection.y);
            target = _selectedTileInitialPosition;
            distanceToTarget = Vector3.Distance(_selectedTile.transform.position, target);

            _selectedTile.transform.position += Vector3.ClampMagnitude(swapDirectionVector3 * swapSpeed * Time.deltaTime, distanceToTarget);
        }

        // move  swap tile
        {
            swapDirectionVector3 *= -1;
            target = _selectedTileInitialPosition + swapDirectionVector3;
            distanceToTarget = Vector3.Distance(_swapTile.transform.position, target);

            _swapTile.transform.position += Vector3.ClampMagnitude(swapDirectionVector3 * swapSpeed * Time.deltaTime, distanceToTarget);
        }
    }
}
