using System.Numerics;

static class Solver
{
    private const int MAX_ITERATIONS = 100;
    private const float EPS = 0.001f;

    private static readonly List<Vector2> edgePoints = new List<Vector2>();
    private static readonly List<Vector2> edgeNormals = new List<Vector2>();

    public static bool GJK(Shape shapeA, Shape shapeB, out Vector2 normal, out float depth)
    {
        normal = Vector2.Zero;
        depth = 0.0f;

        // select first point
        Vector2 d1 = new Vector2(1.0f, 0.0f);
        Vector2 p1 = GetSupportPoint(shapeA, shapeB, d1);

        // select second point
        Vector2 d2 = Vector2.Normalize(-p1);
        Vector2 p2 = GetSupportPoint(shapeA, shapeB, d2);

        // if second point in the same halfspace as first, there is no collision
        if (Vector2.Dot(p2, d2) < 0.0f)
            return false;

        Vector2 p1p2 = p2 - p1;

        // two ways of computing d3
        // through cross:
        // Vector2 d3 = Vector2.Normalize(Vector3
        //    .Cross(Vector3.Cross(p1.AsVector3(), p1p2.AsVector3()), p1p2.AsVector3())
        //    .AsVector2());

        // third point is calculated by normal of edge between p1 and p2
        // there are two normals, select one that points towards origin

        // through dot and sign:
        Vector2 d3 = Vector2.Normalize(p1p2.TurnLeft());
        d3 *= -MathF.Sign(Vector2.Dot(p1, d3));

        for (int i = 0; i < MAX_ITERATIONS; i++)
        {
            Vector2 p3 = GetSupportPoint(shapeA, shapeB, d3);
            // if new point in same halfspace as p1-p2
            if (Vector2.Dot(p3, d3) < EPS)
                return false;

            Trig simplex = new Trig(p1, p2, p3);
            if (IsPointInTrig(simplex, Vector2.Zero))
            {
                (normal, depth) = EPA(shapeA, shapeB, simplex);
                return true;
            }

            // simplex can be extended in two directions
            // normal to p2p3 or normal to p3p1
            // select one that points towards origin

            d1 = (p3 - p2).TurnLeft();
            d2 = (p1 - p3).TurnLeft();

            // p1 and p2 are new trig base edge
            // p3 will be calculated as support to d3 direction
            if (Vector2.Dot(-p2, d1) > EPS)
            {
                p1 = p3;
                // p2 = p2;
                d3 = d1;
            }
            else
            {
                // p1 = p1;
                p2 = p3;
                d3 = d2;
            }

            d3 = Vector2.Normalize(d3);
        }

        return false;
    }

    private static (Vector2 normal, float depth) EPA(Shape shapeA, Shape shapeB, Trig simplex)
    {
        edgePoints.Add(simplex.p1);
        edgePoints.Add(simplex.p2);
        edgePoints.Add(simplex.p3);

        edgeNormals.Add(Vector2.Normalize((simplex.p2 - simplex.p1).TurnLeft()));
        edgeNormals.Add(Vector2.Normalize((simplex.p3 - simplex.p2).TurnLeft()));
        edgeNormals.Add(Vector2.Normalize((simplex.p1 - simplex.p3).TurnLeft()));

        Vector2 resultNorm = Vector2.Zero;
        float resultDepth = 0.0f;

        for (int i = 0; i < MAX_ITERATIONS; i++)
        {
            int closestIndex = -1;
            float closestDist = float.MaxValue;

            for (int j = 0; j < edgePoints.Count; j++)
            {
                Vector2 edgeStart = edgePoints[j];
                Vector2 edgeNorm = edgeNormals[j];

                float dist = Vector2.Dot(edgeStart, edgeNorm);
                if (dist > closestDist)
                    continue;

                closestDist = dist;
                closestIndex = j;
            }

            Vector2 closestPoint = edgePoints[closestIndex]; // closest edge start
            Vector2 closestNorm = edgeNormals[closestIndex]; // closest edge norm
            Vector2 support = GetSupportPoint(shapeA, shapeB, closestNorm);

            // if new point projection on closest edge is small
            // it means that point is one of two edge points
            // or also located on that edge
            if (Vector2.Dot(support - closestPoint, closestNorm) <= EPS)
            {
                resultNorm = closestNorm;
                resultDepth = closestDist;
                break;
            }

            // edgePoints[closestIndex] is splited edge start
            // edgePoints[closestIndex+1] is splited edge end, clamp by %edgePoints.Count
            // in case if edgeStart is last element, so edgeEnd is edgePoints[0]

            // norm from edgeStart to newPoint
            Vector2 n1 = Vector2.Normalize((support - edgePoints[closestIndex]).TurnLeft());

            // norm from newPoint to edgeEnd
            Vector2 n2 = Vector2.Normalize(
                (edgePoints[(closestIndex + 1) % edgePoints.Count] - support).TurnLeft()
            );

            edgeNormals[closestIndex] = n1; // change norm edgeStart-edgeEnd to edgeStart-newPoint
            edgeNormals.Insert(closestIndex + 1, n2); // same for newPoint-edgeEnd
            edgePoints.Insert(closestIndex + 1, support);
        }

        edgePoints.Clear();
        edgeNormals.Clear();

        return (resultNorm, resultDepth);
    }

    private static Vector2 GetSupportPoint(Shape shapeA, Shape shapeB, Vector2 direction)
    {
        // spent 30 mins debugging because didn't normalize dirs
        // so circle shape return wrong support point
        if (direction.LengthSquared() > 1.0f + EPS)
            throw new ArgumentException("Direction is not normalized", nameof(direction));
        return shapeA.GetSupportPoint(direction) - shapeB.GetSupportPoint(-direction);
    }

    // clockwise order
    private static bool IsPointInTrig(Trig trig, Vector2 point)
    {
        Vector2 n1 = (trig.p2 - trig.p1).TurnRight();
        Vector2 n2 = (trig.p3 - trig.p2).TurnRight();
        Vector2 n3 = (trig.p1 - trig.p3).TurnRight();

        float t1 = Vector2.Dot(point - trig.p1, n1);
        float t2 = Vector2.Dot(point - trig.p2, n2);
        float t3 = Vector2.Dot(point - trig.p3, n3);

        return t1 > 0.0f && t2 > 0.0f && t3 > 0.0f;
    }

    record struct Trig(Vector2 p1, Vector2 p2, Vector2 p3);
}
