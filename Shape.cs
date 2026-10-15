using System.Numerics;
using Raylib_cs;

abstract class Shape
{
    public Vector2 position;
    public float rotation;

    protected Shape(Vector2 position, float rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }

    public abstract void Draw();
    public abstract Vector2 GetSupportPoint(Vector2 direction);
}

class Circle : Shape
{
    public float radius;

    public Circle(Vector2 position, float rotation, float radius)
        : base(position, rotation)
    {
        this.radius = radius;
    }

    public override void Draw()
    {
        Raylib.DrawCircleLinesV(position, radius, Color.White);
    }

    public override Vector2 GetSupportPoint(Vector2 direction)
    {
        return position + direction * radius;
    }
}

class Rect : Shape
{
    public Vector2 size; // width + height

    public Rect(Vector2 position, float rotation, Vector2 size)
        : base(position, rotation)
    {
        this.size = size;
    }

    public override void Draw()
    {
        Vector2 half = (0.5f * size).Rotate(rotation);
        Vector2 p1 = position + half;
        Vector2 p2 = position + half.TurnRight();
        Vector2 p3 = position - half;
        Vector2 p4 = position + half.TurnLeft();

        Raylib.DrawLineV(p1, p2, Color.White);
        Raylib.DrawLineV(p2, p3, Color.White);
        Raylib.DrawLineV(p3, p4, Color.White);
        Raylib.DrawLineV(p4, p1, Color.White);
    }

    public override Vector2 GetSupportPoint(Vector2 direction)
    {
        Vector2 half = (0.5f * size).Rotate(rotation);
        Vector2 p1 = position + half;
        Vector2 p2 = position + half.TurnRight();
        Vector2 p3 = position - half;
        Vector2 p4 = position + half.TurnLeft();

        Vector2 maxPoint = p1;
        float maxProj = Vector2.Dot(p1, direction);

        float tempProj;
        if ((tempProj = Vector2.Dot(p2, direction)) > maxProj)
        {
            maxPoint = p2;
            maxProj = tempProj;
        }
        if ((tempProj = Vector2.Dot(p3, direction)) > maxProj)
        {
            maxPoint = p3;
            maxProj = tempProj;
        }
        if (Vector2.Dot(p4, direction) > maxProj)
        {
            maxPoint = p4;
        }
        return maxPoint;
    }
}
