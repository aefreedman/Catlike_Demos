using System;
using UnityEngine;

public class BezierSpline : MonoBehaviour
{
    public enum BezierControlPointMode
    {
        Free,
        Aligned,
        Mirrored
    }


    [SerializeField] private bool _loop;

    [SerializeField] private BezierControlPointMode[] _modes;

    [SerializeField] private Vector3[] _points;

    public bool Loop
    {
        get { return _loop; }
        set
        {
            _loop = value;
            if (!value) return;
            _modes[_modes.Length - 1] = _modes[0];
            SetControlPoint(0, _points[0]);
        }
    }

    public int ControlPointCount
    {
        get { return _points.Length; }
    }

    public int CurveCount
    {
        get { return (_points.Length - 1)/3; }
    }

    public BezierControlPointMode GetControlPointMode(int index)
    {
        return _modes[(index + 1)/3];
    }

    public void SetControlPointMode(int index, BezierControlPointMode mode)
    {
        var modeIndex = (index + 1)/3;

        _modes[modeIndex] = mode;
        if (Loop)
        {
            if (modeIndex == 0)
            {
                _modes[_modes.Length - 1] = mode;
            }
            else if (modeIndex == _modes.Length - 1)
            {
                _modes[0] = mode;
            }
        }
        EnforceMode(index);
    }

    public Vector3 GetControlPoint(int index)
    {
        return _points[index];
    }

    public void SetControlPoint(int index, Vector3 point)
    {
        if (index%3 == 0)
        {
            var delta = point - _points[index];

            if (Loop)
            {
                if (index == 0)
                {
                    _points[1] += delta;
                    _points[_points.Length - 2] += delta;
                    _points[_points.Length - 1] = point;
                }
                else if (index == _points.Length - 1)
                {
                    _points[0] = point;
                    _points[1] += delta;
                    _points[index - 1] += delta;
                }
                else
                {
                    _points[index - 1] += delta;
                    _points[index + 1] += delta;
                }
            }
            else
            {
                if (index > 0)
                {
                    _points[index - 1] += delta;
                }
                if (index + 1 < _points.Length)
                {
                    _points[index + 1] += delta;
                }
            }
        }
        _points[index] = point;
        EnforceMode(index);
    }

    private void EnforceMode(int index)
    {
        var modeIndex = (index + 1)/3;
        var mode = _modes[modeIndex];
        if (mode == BezierControlPointMode.Free || !Loop && modeIndex == 0 || modeIndex == _modes.Length - 1) return;
        var middleIndex = modeIndex*3;
        int fixedIndex, enforcedIndex;
        if (index <= middleIndex)
        {
            fixedIndex = middleIndex - 1;
            if (fixedIndex < 0)
            {
                fixedIndex = _points.Length - 2;
            }
            enforcedIndex = middleIndex + 1;
            if (enforcedIndex >= _points.Length)
            {
                enforcedIndex = 1;
            }
        }
        else
        {
            fixedIndex = middleIndex + 1;
            if (fixedIndex >= _points.Length)
            {
                fixedIndex = 1;
            }
            enforcedIndex = middleIndex - 1;
            if (enforcedIndex < 0)
            {
                enforcedIndex = _points.Length - 2;
            }
        }

        var middle = _points[middleIndex];
        var enforcedTangent = middle - _points[fixedIndex];
        if (mode == BezierControlPointMode.Aligned)
        {
            enforcedTangent = enforcedTangent.normalized*Vector3.Distance(middle, _points[enforcedIndex]);
        }
        _points[enforcedIndex] = middle + enforcedTangent;
    }

    public void Reset()
    {
        _points = new[]
        {
            new Vector3(1f, 0f, 0f),
            new Vector3(2f, 0, 0),
            new Vector3(3f, 0, 0),
            new Vector3(4f, 0, 0)
        };
        _modes = new[]
        {
            BezierControlPointMode.Free,
            BezierControlPointMode.Free
        };
    }

    public Vector3 GetPoint(float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = _points.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t)*CurveCount;
            i = (int) t;
            t -= i;
            i *= 3;
        }
        return transform.TransformPoint(Bezier.GetPoint(_points[i], _points[i + 1], _points[i + 2], _points[i + 3], t));
    }

    public Vector3 GetVelocity(float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = _points.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t)*CurveCount;
            i = (int) t;
            t -= i;
            i *= 3;
        }
        return
            transform.TransformPoint(
                Bezier.GetFirstDerivative(_points[i], _points[i + 1], _points[i + 2], _points[i + 3], t) -
                transform.position);
    }

    public Vector3 GetDirection(float t)
    {
        return GetVelocity(t).normalized;
    }

    public void AddCurve()
    {
        var point = _points[_points.Length - 1];
        Array.Resize(ref _points, _points.Length + 3);
        point.x += 1f;
        _points[_points.Length - 3] = point;
        point.x += 1f;
        _points[_points.Length - 2] = point;
        point.x += 1f;
        _points[_points.Length - 1] = point;

        Array.Resize(ref _modes, _modes.Length + 1);
        _modes[_modes.Length - 1] = _modes[_modes.Length - 2];
        EnforceMode(_points.Length - 4);
        if (Loop)
        {
            _points[_points.Length - 1] = _points[0];
            _modes[_modes.Length - 1] = _modes[0];
            EnforceMode(0);
        }
    }
}