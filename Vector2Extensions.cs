using System.Numerics;

static class Vector2Extensions
{
    public static Vector2 Rotate(this Vector2 vec, float angle)
    {
        (float sin, float cos) = MathF.SinCos(angle);
        return new Vector2(vec.X * cos - vec.Y * sin, vec.X * sin + vec.Y * cos);
    }

    public static Vector2 TurnLeft(this Vector2 vec) => new Vector2(-vec.Y, vec.X);

    public static Vector2 TurnRight(this Vector2 vec) => new Vector2(vec.Y, -vec.X);
}
