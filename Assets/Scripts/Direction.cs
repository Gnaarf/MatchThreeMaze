using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public enum Direction
{
    Left,
    Right,
    Up,
    Down,
    HorizontalBothDirections,
    VerticalBothDirections,
    AllFourDirections
}

public static class DirectionHelper
{
    public static List<Vector2Int> DirectionToVectors(Direction direction)
    {
        switch(direction)
        {
            case Direction.Down:
                return new List<Vector2Int>() { Vector2Int.down };

            case Direction.Left:
                return new List<Vector2Int>() { Vector2Int.left };

            case Direction.Right:
                return new List<Vector2Int>() { Vector2Int.right };

            case Direction.Up:
                return new List<Vector2Int>() { Vector2Int.up };

            case Direction.HorizontalBothDirections:
                return new List<Vector2Int>() { Vector2Int.left, Vector2Int.right };

            case Direction.VerticalBothDirections:
                return new List<Vector2Int>() { Vector2Int.down, Vector2Int.up };

            case Direction.AllFourDirections:
                return new List<Vector2Int>() { Vector2Int.left, Vector2Int.down, Vector2Int.right, Vector2Int.up };

            default:
                return new List<Vector2Int>();
        }
    }
}