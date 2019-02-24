using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static Vector2Int ConvertToInt(this Vector2 vector)
    {
        return new Vector2Int((int)vector.x, (int)vector.y);
    }

    /// <summary>
    /// checks if the vector lies within the 2D interval from min (inclusive) to max (exclusive)
    /// </summary>
    /// <param name="value">value to check for</param>
    /// <param name="min">lower bound of the interval (inclusive)</param>
    /// <param name="max">upper bound of the interval (exclusive)</param>
    public static bool IsBetween(this Vector2Int value, Vector2Int min, Vector2Int max)
    {
        return value.x >= min.x && value.x < max.x && value.y >= min.y && value.y < max.y;

    }

}
