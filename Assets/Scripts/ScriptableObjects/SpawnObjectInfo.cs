using UnityEngine;
using UnityEditor;
using System;

[Serializable]
public class SpawnObjectInfo: ScriptableObject {
	private bool Expanded = false;
	private bool PropertiesExpanded = false;
	private GUIStyle headerStyle;

	public int ParentId;
	public float SpawnTime;
	public SpawnProperties Properties;
	public SpawnType SpawnType;

	public static SpawnObjectInfo CreateCopyFromAsset(SpawnObjectInfo si) {
		var s = Instantiate(si);
		s.Properties = Instantiate(si.Properties);
		return s;
	}

	public void OnEnable() {
		hideFlags = HideFlags.HideAndDontSave;
	}

	public void InitInEditor() {
		ParentId = -1;
		SpawnTime = 0;

		Properties = new SpawnProperties();
		SpawnType = (SpawnType)999;
	}

	public void Save() {
		EditorUtility.SetDirty(this);
		if(Properties != null) {
			EditorUtility.SetDirty(Properties);
		}
	}

	public void Render(int i) {
		if(headerStyle == null ) {
			headerStyle = new GUIStyle(EditorStyles.foldout) {
				fontStyle = FontStyle.Bold
			};
		}

		string parentid = ParentId < 0 ? "" : $"ParentId: {ParentId}  ::: ";
		string type = Enum.IsDefined(typeof(SpawnType), SpawnType) ? SpawnType.ToString() : "undefined";
		string foldoutHeader = $"{i} - {parentid} Time: {SpawnTime}";
		foldoutHeader += $"  :::  Type: {type}";

		GUI.SetNextControlName($"_{i}");

		Expanded = EditorGUILayout.Foldout(Expanded, foldoutHeader, true, headerStyle);
		if (Expanded) {
			EditorGUI.indentLevel++;

			// Base Properties
			GUI.SetNextControlName($"pid_{i}");
			ParentId = EditorGUILayout.IntField("Parent Id", ParentId, GUILayout.Width(750));
			GUI.SetNextControlName($"spawntime_{i}");
			SpawnTime = EditorGUILayout.FloatField("Spawn Time", SpawnTime, GUILayout.Width(750));

			GUI.SetNextControlName($"type_{i}");
			var currentType = SpawnType;
			SpawnType = (SpawnType)EditorGUILayout.EnumPopup("Type", SpawnType, GUILayout.Width(750));
			if (currentType != SpawnType) {
				DestroyImmediate(Properties, true);
				SpawnProperties p;
				switch (SpawnType) {	
					case SpawnType.Small:
						p = ScriptableObject.CreateInstance<SmallEnemyProperties>();
						break;
					case SpawnType.Jail:
						p = ScriptableObject.CreateInstance<JailEnemyProperties>();
						break;
					case SpawnType.Laser:
						p = ScriptableObject.CreateInstance<LaserEnemyProperties>();
						break;
					case SpawnType.Blind:
						p = ScriptableObject.CreateInstance<SpawnProperties>();
						break;
					default:
					case SpawnType.Special:
						p = ScriptableObject.CreateInstance<SpawnProperties>();
						break;
				}
				AssetDatabase.AddObjectToAsset(p, this);
				Properties = p;
				PropertiesExpanded = true;
			}

			PropertiesExpanded = EditorGUILayout.Foldout(PropertiesExpanded, "Properties", true);
			if (Properties != null && PropertiesExpanded) {
				EditorGUI.indentLevel++;
				Properties.Render();
				EditorGUI.indentLevel--;
			}

			
		}
	}
}