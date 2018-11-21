using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using System.Linq;

public class PropertiesEditor: Editor {
	public override bool UseDefaultMargins() {
		return false;
	}
}

public class SpawnInfoGUI {
	public SpawnObjectInfo Info;
	public bool Expanded = false;
	public bool PropertiesExpanded = false;
	private Editor editor;

	private GUIStyle headerStyle;

	public SpawnInfoGUI() : this(new SpawnObjectInfo(-1, 0, new SpawnProperties(), (SpawnType)999)) { }

	public SpawnInfoGUI(SpawnObjectInfo info) {
		Info = info;
		editor = Editor.CreateEditor(Info.Properties);

		headerStyle = new GUIStyle(EditorStyles.foldout) {
			fontStyle = FontStyle.Bold
		};
	}

	public void Render(int i) {
		string parentid = Info.ParentId < 0 ? "" : $"ParentId: {Info.ParentId}  ::: ";
		string type = Enum.IsDefined(typeof(SpawnType), Info.SpawnType) ? Info.SpawnType.ToString() : "undefined";
		string foldoutHeader = $"{i} - {parentid} Time: {Info.SpawnTime}";	
		foldoutHeader += $"  :::  Type: {type}";

		GUI.SetNextControlName($"_{i}");
		
		Expanded = EditorGUILayout.Foldout(Expanded, foldoutHeader, true, headerStyle);
		if(Expanded) {
			EditorGUI.indentLevel++;

			// Base Properties
			GUI.SetNextControlName($"pid_{i}");
			Info.ParentId = EditorGUILayout.IntField("Parent Id", Info.ParentId, GUILayout.Width(750));
			GUI.SetNextControlName($"spawntime_{i}");
			Info.SpawnTime = EditorGUILayout.FloatField("Spawn Time", Info.SpawnTime, GUILayout.Width(750));

			GUI.SetNextControlName($"type_{i}");
			var currentType = Info.SpawnType;
			Info.SpawnType = (SpawnType)EditorGUILayout.EnumPopup("Type", Info.SpawnType, GUILayout.Width(750));
			if(currentType != Info.SpawnType) {
				switch (Info.SpawnType) {
					case SpawnType.Slow:
						Info.Properties = new SlowEnemyProperties();
						break;
					case SpawnType.Jail:
						Info.Properties = new JailEnemyProperties();
						break;
					case SpawnType.Blind:
						Info.Properties = new SpawnProperties();
						break;
					case SpawnType.Boss:
						Info.Properties = new SpawnProperties();
						break;
					default:
						break;
				}			
				editor = Editor.CreateEditor(Info.Properties, typeof(PropertiesEditor));	
				PropertiesExpanded = true;
			}

			PropertiesExpanded = EditorGUILayout.Foldout(PropertiesExpanded, "Properties", true);
			if(PropertiesExpanded) {
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

class SpawnTimeline : EditorWindow {

	public List<SpawnInfoGUI> SpawnInfo = new List<SpawnInfoGUI>();
	ScriptableObject asset;
	public bool buttonsEnabled = false;

	[MenuItem("Window/Spawn Timeline")]
	public static void ShowWindow() {
		EditorWindow.GetWindow<SpawnTimeline>("Spawn Timeline");
	}

	public void Awake() {
		EditorApplication.playModeStateChanged += PlayModeStateChanged;
	}

	public void OnDestroy() {
		EditorApplication.playModeStateChanged -= PlayModeStateChanged;
		SaveSpawnInfo();
	}

	public void PlayModeStateChanged(PlayModeStateChange state) {
		if(state == PlayModeStateChange.ExitingEditMode) {
			SaveSpawnInfo();
		}
	}

	private void OnFocus() {
		if(asset != null && (asset as SpawnExport).SpawnInfo.Length != SpawnInfo.Count) {
			SpawnInfo = new List<SpawnInfoGUI>((asset as SpawnExport).SpawnInfo.Select(info => new SpawnInfoGUI(info)));
		}
	}

	void OnSelectionChange() {	
		if ( Selection.activeObject is SpawnExport ) {
			buttonsEnabled = true;
			SpawnExport s = Selection.activeObject as SpawnExport;
			if(asset != s) {
				// switching to a different spawn export
				SaveSpawnInfo();
				asset = s;
				SpawnInfo = new List<SpawnInfoGUI>(s.SpawnInfo.Select(info => new SpawnInfoGUI(info)));
			}
		}
		else if ( Selection.activeTransform != null ) {			
			LevelManager lm = Selection.activeTransform.GetComponent<LevelManager>();
			if( lm != null ) {
				Debug.Log("Selected Level Manager");
				buttonsEnabled = true;
				if( lm.SpawnData != null ) {
					if(asset != lm.SpawnData) {
						asset = lm.SpawnData;
						SpawnInfo = new List<SpawnInfoGUI>(lm.SpawnData.SpawnInfo.Select(info => new SpawnInfoGUI(info)));
					}
				} 
				else {
					CreateSpawnData();
					SpawnExport e = asset as SpawnExport;
					e.SpawnInfo = new SpawnObjectInfo[0];
					lm.SpawnData = e;
				}
			}
			else {
				ClearSelectionAndSave();
			}
		}
		else {
			ClearSelectionAndSave();
		}
		Repaint();
	}

	private void CreateSpawnData() {
		asset = ScriptableObject.CreateInstance(typeof(SpawnExport));
		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath($"Assets/Resources/Data/{SceneManager.GetActiveScene().name}_SpawnInfo.asset");
		AssetDatabase.CreateAsset(asset, assetPathAndName);
		AssetDatabase.SaveAssets();
		Selection.activeObject = asset;
	}

	private void ClearSelectionAndSave() { 
		Debug.Log("Clear Selection and Save");
		SaveSpawnInfo();
		SpawnInfo.Clear();
		buttonsEnabled = false;
		asset = null;
	}

	private void SaveSpawnInfo() {
		if( asset != null ) {
			var si = SpawnInfo.Select(s => s.Info).ToArray();
			EditorUtility.SetDirty(asset);
			(asset as SpawnExport).SpawnInfo = si;
			AssetDatabase.SaveAssets();
		}
	}

	void OnGUI() {		
		GUIStyle style = new GUIStyle() { margin = new RectOffset(0, 0, 5, 0) };
		EditorGUILayout.BeginHorizontal(style, GUILayout.Width(600));
		GUI.enabled = buttonsEnabled;
		if(GUILayout.Button("+ Add New Spawn")) {
			SpawnInfo.Add(new SpawnInfoGUI());
		}
		
		GUI.enabled = buttonsEnabled && SpawnInfo.Count > 0;
		if (GUILayout.Button("- Remove Spawn")) {
			var focused = GUI.GetNameOfFocusedControl();
			if (focused.Length > 0) {
				int index = Convert.ToInt32(focused.Substring(focused.LastIndexOf('_')+1));
				SpawnInfo.RemoveAt(index);
			
				int nextIndex = Mathf.Min( index, SpawnInfo.Count - 1 );
				GUI.FocusControl($"_{nextIndex}");
			}
		}
		GUI.enabled = true;

		if (GUILayout.Button("Save")) {
			SaveSpawnInfo();
		}
		EditorGUILayout.EndHorizontal();

		for (int i = 0; i < SpawnInfo.Count; i++) {
			EditorGUILayout.BeginVertical("box", GUILayout.Width(750));
			SpawnInfo[i].Render(i);
			EditorGUILayout.EndVertical();
		}
	}
}
