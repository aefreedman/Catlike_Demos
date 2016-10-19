using System.Runtime.InteropServices.ComTypes;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierSpline))]
public class BezierSplineInspector : Editor
{
    private const int LineSteps = 10;
    private const float DirectionScale = 0.5f;
    private Quaternion _handleRotation;
    private Transform _handleTransform;

    private BezierSpline _spline;

    private static Color[] _modeColors =
    {
        Color.white,
        Color.yellow,
        Color.cyan
    };

    private void OnSceneGUI()
    {
        _spline = target as BezierSpline;
        _handleTransform = _spline.transform;
        _handleRotation = Tools.pivotRotation == PivotRotation.Local ? _handleTransform.rotation : Quaternion.identity;

        var p0 = ShowPoint(0);

        for (var i = 1; i < _spline.ControlPointCount; i += 3)
        {
            var p1 = ShowPoint(i);
            var p2 = ShowPoint(i + 1);
            var p3 = ShowPoint(i + 2);
            Handles.color = Color.gray;
            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p2, p3);
            Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
            p0 = p3;
        }
        ShowDirections();
    }

    public override void OnInspectorGUI()
    {
        _spline = target as BezierSpline;
        EditorGUI.BeginChangeCheck();
        var loop = EditorGUILayout.Toggle("Loop", _spline.Loop);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_spline, "Toggle Loop");
            EditorUtility.SetDirty(_spline);
            _spline.Loop = loop;
        }
        if (SelectedIndex >= 0 && SelectedIndex < _spline.ControlPointCount)
        {
            DrawSelectedPointInspector();
        }
        if (GUILayout.Button("Add Curve"))
        {
            Undo.RecordObject(_spline, "Add Curve");
            _spline.AddCurve();
            EditorUtility.SetDirty(_spline);
        }
    }

    private void DrawSelectedPointInspector()
    {
        GUILayout.Label("Selected Point");
        EditorGUI.BeginChangeCheck();
        var point = EditorGUILayout.Vector3Field("Position", _spline.GetControlPoint(SelectedIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_spline, "Move Point");
            EditorUtility.SetDirty(_spline);
            _spline.SetControlPoint(SelectedIndex, point);
        }
        EditorGUI.BeginChangeCheck();
        var mode =
            (BezierSpline.BezierControlPointMode)
                EditorGUILayout.EnumPopup("Mode", _spline.GetControlPointMode(SelectedIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_spline, "Change Point Mode");
            _spline.SetControlPointMode(SelectedIndex, mode);
            EditorUtility.SetDirty(_spline);
        }
    }

    private const float HandleSize = 0.04f;
    private const float PickSize = 0.06f;
    private int SelectedIndex = -1;

    private Vector3 ShowPoint(int index)
    {
        var point = _handleTransform.TransformPoint(_spline.GetControlPoint(index));
        var size = HandleUtility.GetHandleSize(point);
        if (index == 0)
        {
            size *= 2f;
        }
        Handles.color = _modeColors[(int)_spline.GetControlPointMode(index)];
        if (Handles.Button(point, _handleRotation, size*HandleSize, size*PickSize, Handles.DotCap))
        {
            SelectedIndex = index;
            Repaint();
        }
        if (SelectedIndex == index)
        {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, _handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_spline, "Move Point");
                EditorUtility.SetDirty(_spline);
                _spline.SetControlPoint(index, _handleTransform.InverseTransformPoint(point));
            }
        }
        return point;
    }

    private const int StepsPerCurve = 10;

    private void ShowDirections()
    {
        Handles.color = Color.green;
        var point = _spline.GetPoint(0f);
        Handles.DrawLine(point, point + _spline.GetDirection(0f)*DirectionScale);
        var steps = StepsPerCurve*_spline.CurveCount;
        for (var i = 0; i < steps; i++)
        {
            point = _spline.GetPoint(i/(float) steps);
            Handles.DrawLine(point, point + _spline.GetDirection(i/(float) steps)*DirectionScale);
        }
    }
}