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
            new Vector3(3f, 0, 0),
            new Vector3(4f, 0, 0) 
        };
    }

    public Vector3 GetPoint(float t)
    {
        var i = 0;
        if (t >= 1f)
        {
            t = 1f;
            i = Points.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t)*CurveCount;
            i = (int) t;
            t -= i;
            i *= 3;
        }
        return transform.TransformPoint(Bezier.GetPoint(Points[i], Points[i+1], Points[i+2], Points[i+3], t));
    }

    public Vector3 GetVelocity(float t)
    {
        var i = 0;
        if (t >= 1f)
        {
            t = 1f;
            i = Points.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
        return
            transform.TransformPoint(Bezier.GetFirstDerivative(Points[i], Points[i+1], Points[i+2], Points[i+3], t) - transform.position);
    }

    public Vector3 GetDirection(float t)
    {
        return GetVelocity(t).normalized;
    }


    public int CurveCount
    {
        get { return (Points.Length - 1)/3; }
    }
}