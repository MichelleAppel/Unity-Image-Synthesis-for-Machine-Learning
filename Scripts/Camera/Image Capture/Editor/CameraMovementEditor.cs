// CameraMovementEditor.cs

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraMovement))]
public class CameraMovementEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CameraMovement myScript = (CameraMovement)target;
        if (GUILayout.Button("Next Random Pose"))
        {
            myScript.NextPose();
        }
    }
}