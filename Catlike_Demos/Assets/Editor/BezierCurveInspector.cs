using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierCurve))]
public class BezierCurveInspector : Editor
{
    private BezierCurve _curve;
    private Transform _handleTransform;
    private Quaternion _handleRotation;

    private const int LineSteps = 10;

    private void OnSceneGUI()
    {
        _curve = target as BezierCurve;
        _handleTransform = _curve.transform;
        _handleRotation = Tools.pivotRotation == PivotRotation.Local ? _handleTransform.rotation : Quaternion.identity;

        var p0 = ShowPoint(0);
        var p1 = ShowPoint(1);
        var p2 = ShowPoint(2);

        Handles.color = Color.gray;
        Handles.DrawLine(p0, p1);
        Handles.DrawLine(p1, p2);

        Handles.color = Color.white;
        var lineStart = _curve.GetPoint(0f);
        Handles.color = Color.green;
        Handles.DrawLine(lineStart, lineStart + _curve.GetDirection(0f));
        for (var i = 0; i < LineSteps; i++)
        {
            var lineEnd = _curve.GetPoint(i/(float) LineSteps);
            Handles.color = Color.white;
            Handles.DrawLine(lineStart, lineEnd);
            Handles.color = Color.green;
            Handles.DrawLine(lineEnd, lineEnd + _curve.GetDirection(i / (float) LineSteps));
            lineStart = lineEnd;
        }
    }

    private Vector3 ShowPoint(int index)
    {
        var point = _handleTransform.TransformPoint(_curve.Points[index]);
        EditorGUI.BeginChangeCheck();
        point = Handles.DoPositionHandle(point, _handleRotation);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_curve, "Move Point");
            EditorUtility.SetDirty(_curve);
            _curve.Points[index] = _handleTransform.InverseTransformPoint(point);
        }
        return point;
    }
}