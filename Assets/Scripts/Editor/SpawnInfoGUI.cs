using System;
using UnityEditor;
using UnityEngine;

public class SpawnInfoGUI
{
    public SpawnObjectInfo Info;
    public bool Expanded = false;
    public bool PropertiesExpanded = false;
    private Editor editor;

    private GUIStyle headerStyle;

    public SpawnInfoGUI() : this(new SpawnObjectInfo(-1, 0, ScriptableObject.CreateInstance<SpawnProperties>(), (SpawnType)999)) { }

    public SpawnInfoGUI(SpawnObjectInfo info)
    {
        Info = info;

        editor = Editor.CreateEditor(Info.Properties, typeof(PropertiesEditor));

        headerStyle = new GUIStyle(EditorStyles.foldout)
        {
            fontStyle = FontStyle.Bold
        };
    }

    public void Render(int i)
    {
        string parentid = Info.ParentId < 0 ? "" : $"ParentId: {Info.ParentId}  ::: ";
        string type = Enum.IsDefined(typeof(SpawnType), Info.SpawnType) ? Info.SpawnType.ToString() : "undefined";
        string foldoutHeader = $"{i} - {parentid} Time: {Info.SpawnTime}";
        foldoutHeader += $"  :::  Type: {type}";

        GUI.SetNextControlName($"_{i}");

        Expanded = EditorGUILayout.Foldout(Expanded, foldoutHeader, true, headerStyle);
        if (Expanded)
        {
            EditorGUI.indentLevel++;

            // Base Properties
            GUI.SetNextControlName($"pid_{i}");
            Info.ParentId = EditorGUILayout.IntField("Parent Id", Info.ParentId, GUILayout.Width(750));
            GUI.SetNextControlName($"spawntime_{i}");
            Info.SpawnTime = EditorGUILayout.FloatField("Spawn Time", Info.SpawnTime, GUILayout.Width(750));

            GUI.SetNextControlName($"type_{i}");
            var currentType = Info.SpawnType;
            Info.SpawnType = (SpawnType)EditorGUILayout.EnumPopup("Type", Info.SpawnType, GUILayout.Width(750));
            if (currentType != Info.SpawnType)
            {
                switch (Info.SpawnType)
                {
                    case SpawnType.Slow:
                        Info.Properties = ScriptableObject.CreateInstance<SlowEnemyProperties>();
                        break;
                    case SpawnType.Jail:
                        Info.Properties = ScriptableObject.CreateInstance<JailEnemyProperties>();
                        break;
                    case SpawnType.Blind:
                        Info.Properties = ScriptableObject.CreateInstance<SpawnProperties>();
                        break;
                    default:
                    case SpawnType.Boss:
                        Info.Properties = ScriptableObject.CreateInstance<SpawnProperties>();
                        break;
                }
                editor = Editor.CreateEditor(Info.Properties, typeof(PropertiesEditor));
                PropertiesExpanded = true;
            }

            PropertiesExpanded = EditorGUILayout.Foldout(PropertiesExpanded, "Properties", true);
            if (PropertiesExpanded)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(GUILayout.Width(750));
                EditorGUIUtility.wideMode = true;
                editor.OnInspectorGUI();
                EditorGUIUtility.wideMode = false;
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }
    }
}