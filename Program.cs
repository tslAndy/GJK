using System.Numerics;
using Raylib_cs;

class Program
{
    public static void Main()
    {
        Shape shapeA = new Circle(new Vector2(500.0f, 300.0f), 0.0f, 100.0f);
        Shape shapeB = new Rect(
            new Vector2(300.0f, 400.0f),
            float.DegreesToRadians(45.0f),
            new Vector2(200.0f, 100.0f)
        );

        Raylib.InitWindow(1200, 800, "GJK");
        while (!Raylib.WindowShouldClose())
        {
            if (Raylib.IsMouseButtonDown(MouseButton.Left))
                shapeB.position = Raylib.GetMousePosition();

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

            shapeA.Draw();
            shapeB.Draw();

            bool result = Solver.GJK(shapeA, shapeB, out Vector2 norm, out float depth);
            Raylib.DrawText(result ? "Collision" : "No collision", 30, 30, 30, Color.White);
            if (result)
                Raylib.DrawLineV(shapeB.position, shapeB.position + depth * norm, Color.Green);

            if (result && Raylib.IsMouseButtonPressed(MouseButton.Right))
                shapeB.position += norm * (depth + 0.01f);

            Raylib.EndDrawing();
        }
        Raylib.CloseWindow();
    }
}
