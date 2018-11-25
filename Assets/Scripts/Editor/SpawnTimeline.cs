using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using System.Linq;

class SpawnTimeline : EditorWindow {

	public List<SpawnInfoGUI> SpawnInfo = new List<SpawnInfoGUI>();
	ScriptableObject asset;
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
		SaveSpawnInfo();
		SpawnInfo.Clear();
		buttonsEnabled = false;
		asset = null;
	}

	private void SaveSpawnInfo() {
		if( asset != null ) {
			var si = SpawnInfo.Select(s => new SpawnObjectInfo(
                s.Info.ParentId,
                s.Info.SpawnTime,
                ScriptableObject.CreateInstance<SpawnProperties>(),
                s.Info.SpawnType
                )).ToArray();
			EditorUtility.SetDirty(asset);
			(asset as SpawnExport).SpawnInfo = si;
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
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
