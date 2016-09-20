using UnityEngine;

public class BezierCurve : MonoBehaviour
{
    public Vector3[] Points;

    public void Reset()
    {
        Points = new[]
        {
            new Vector3(1f, 0f, 0f),
            new Vector3(2f, 0, 0),
            new Vector3(3f, 0, 0)
        };
    }

    public Vector3 GetPoint(float t)
    {
        return transform.TransformPoint(Bezier.GetPoint(Points[0], Points[1], Points[2], t));
    }

    public Vector3 GetVelocity(float t)
    {
        return
            transform.TransformPoint(Bezier.GetFirstDerivative(Points[0], Points[1], Points[2], t) - transform.position);
    }

    public Vector3 GetDirection(float t)
    {
        return GetVelocity(t).normalized;
    }
}