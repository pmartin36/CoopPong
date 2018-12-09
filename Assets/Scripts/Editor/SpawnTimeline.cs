using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using System.Linq;

class SpawnTimeline : EditorWindow {

	SpawnExport asset;
	public bool buttonsEnabled = false;

	[MenuItem("Window/Spawn Timeline")]
	public static void ShowWindow() {
		EditorWindow.GetWindow<SpawnTimeline>("Spawn Timeline");
	}

	private void OnEnable() {
		EditorApplication.playModeStateChanged -= PlayModeStateChanged;
		EditorApplication.playModeStateChanged += PlayModeStateChanged;
	}

	private void OnDisable() {
		EditorApplication.playModeStateChanged -= PlayModeStateChanged;
	}

	public void PlayModeStateChanged(PlayModeStateChange state) {
		if(state == PlayModeStateChange.ExitingEditMode) {
			// switching to play mode - saving
			Debug.Log("switching to play mode");
			SaveSpawnInfo();
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
			}
		}
		else if ( Selection.activeTransform != null ) {			
			LevelManager lm = Selection.activeTransform.GetComponent<LevelManager>();
			if( lm != null ) {
				Debug.Log("Selected Level Manager");
				buttonsEnabled = true;
				if( lm.SpawnDataAsset != null ) {
					if(asset != lm.SpawnDataAsset) {
						asset = lm.SpawnDataAsset;
					}
				} 
				else {
					CreateSpawnData();
					SpawnExport e = asset as SpawnExport;
					e.SpawnInfo = new List<SpawnObjectInfo>();
					lm.SpawnDataAsset = e;
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
		asset = ScriptableObject.CreateInstance<SpawnExport>();
		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath($"Assets/Resources/Data/{SceneManager.GetActiveScene().name}_SpawnInfo.asset");
		AssetDatabase.CreateAsset(asset, assetPathAndName);
		AssetDatabase.SaveAssets();
		Selection.activeObject = asset;
	}

	private void ClearSelectionAndSave() { 
		SaveSpawnInfo();
		buttonsEnabled = false;
		asset = null;
	}

	private void SaveSpawnInfo() {
		if( asset != null ) {
			asset.Save();
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	}

	void OnGUI() {	
		GUIStyle style = new GUIStyle() { margin = new RectOffset(0, 0, 5, 0) };
		EditorGUILayout.BeginHorizontal(style, GUILayout.Width(600));
		GUI.enabled = buttonsEnabled;
		if(GUILayout.Button("+ Add New Spawn")) {
			var si = ScriptableObject.CreateInstance<SpawnObjectInfo>();
			si.InitInEditor();
			AssetDatabase.AddObjectToAsset(si, asset);
			asset.SpawnInfo.Add(si);
			SaveSpawnInfo();
		}
		
		GUI.enabled = buttonsEnabled && asset.SpawnInfo.Count > 0;
		if (GUILayout.Button("- Remove Spawn")) {
			var focused = GUI.GetNameOfFocusedControl();
			if (focused.Length > 0) {
				int index = Convert.ToInt32(focused.Substring(focused.LastIndexOf('_')+1));
				var removed = asset.SpawnInfo[index];
				AssetDatabase.RemoveObjectFromAsset(removed);
				asset.SpawnInfo.RemoveAt(index);
			
				int nextIndex = Mathf.Min( index, asset.SpawnInfo.Count - 1 );
				GUI.FocusControl($"_{nextIndex}");
				SaveSpawnInfo();
			}
		}
		GUI.enabled = true;

		if (GUILayout.Button("Save")) {
			SaveSpawnInfo();
		}
		EditorGUILayout.EndHorizontal();

		if (asset != null) {
			for (int i = 0; i < asset.SpawnInfo.Count; i++) {
				EditorGUILayout.BeginVertical("box", GUILayout.Width(750));
				asset.SpawnInfo[i].Render(i);
				EditorGUILayout.EndVertical();
			}
		}
	}
}
