using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaypointRegion))]
public class WaypointRegionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WaypointRegion region =
            (WaypointRegion)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Generate Waypoints"))
        {
            region.GenerateWaypoints();

            EditorUtility.SetDirty(region);
        }

        if (GUILayout.Button("Clear Waypoints"))
        {
            region.ClearWaypoints();

            EditorUtility.SetDirty(region);
        }
    }
}