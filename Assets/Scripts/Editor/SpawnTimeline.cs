using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using System.Linq;

public class SpawnInfoGUI {
	public SpawnObjectInfo Info;
	public bool Expanded = false;
	public bool PropertiesExpanded = false;

	private Editor editor;

	public SpawnInfoGUI() : this(new SpawnObjectInfo(-1, 0, new SpawnProperties(), (SpawnType)999)) { }

	public SpawnInfoGUI(SpawnObjectInfo info) {
		Info = info;
		editor = Editor.CreateEditor(Info.Properties);
	}

	public void Render(int i) {
		GUI.SetNextControlName($"_{i}");
		Expanded = EditorGUILayout.Foldout(Expanded, $"Id: {i}  --  ParentId: {Info.ParentId}  --  Time: {Info.SpawnTime}  --  {Info.SpawnType.ToString()}");
		if(Expanded) {
			EditorGUI.indentLevel++;

			// Base Properties
			Info.ParentId = EditorGUILayout.IntField("Parent Id", Info.ParentId);
			Info.SpawnTime = EditorGUILayout.FloatField("Spawn Time", Info.SpawnTime);

			var currentType = Info.SpawnType;
			Info.SpawnType = (SpawnType)EditorGUILayout.EnumPopup("Type", Info.SpawnType);
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
				editor = Editor.CreateEditor(Info.Properties);
			}

			PropertiesExpanded = EditorGUILayout.Foldout(PropertiesExpanded, "Properties");
			if(PropertiesExpanded) {
				EditorGUI.indentLevel++;		
				editor.OnInspectorGUI();
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

	void OnSelectionChange() {	
		if ( Selection.activeObject is SpawnExport ) {
			buttonsEnabled = true;
			SpawnExport s = Selection.activeObject as SpawnExport;
			if(asset != s) {
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
					lm.SpawnData = asset as SpawnExport;
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
		EditorGUILayout.BeginHorizontal(GUILayout.Width(300));
		GUI.enabled = buttonsEnabled;
		if(GUILayout.Button("+ Add New Spawn")) {
			SpawnInfo.Add(new SpawnInfoGUI());
		}
		
		GUI.enabled = buttonsEnabled && SpawnInfo.Count > 0;
		if (GUILayout.Button("- Remove Spawn")) {
			var focused = GUI.GetNameOfFocusedControl();
			int index = Convert.ToInt32(focused.Substring(focused.LastIndexOf('_')+1));
			SpawnInfo.RemoveAt(index);
			
			int nextIndex = Mathf.Min( index, SpawnInfo.Count - 1 );
			GUI.FocusControl($"_{nextIndex}");
		}
		GUI.enabled = true;
		EditorGUILayout.EndHorizontal();

		for (int i = 0; i < SpawnInfo.Count; i++) {
			SpawnInfo[i].Render(i);
		}
	}
}
