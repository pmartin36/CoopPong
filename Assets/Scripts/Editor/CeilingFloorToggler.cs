using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CeilingFloorToggler : EditorWindow
{
	public bool ShowingFloor;
	public bool disabled;

	public GameObject Camera;
	public GameObject Levels;
	public GameObject Ceiling;
	public GameObject Floor;

	[MenuItem("Window/CeilingFloor Toggle")]
	public static void ShowWindow() {
		EditorWindow.GetWindow<CeilingFloorToggler>("Ceiling/Floor Toggler");
	}

	private void OnEnable() {
		this.minSize = new Vector2(0,0);
		EditorApplication.playModeStateChanged -= PlayModeStateChanged;
		EditorApplication.playModeStateChanged += PlayModeStateChanged;

		Camera = GameObject.Find("Main Camera");
		Levels = GameObject.Find("Levels Container");
		Ceiling = Levels?.transform.Find("Ceiling")?.gameObject;
		Floor = Levels?.transform.Find("Floor")?.gameObject;
		if(Camera == null || Levels == null || Ceiling == null || Floor == null) {
			disabled = true;
			Debug.Log($"Camera: {(Camera == null?"Good":"null")}, Levels: {(Levels == null ? "Good" : "null")}, Ceiling: {(Ceiling == null ? "Good" : "null")}, Floor: {(Floor == null ? "Good" : "null")}");
		}
		else {
			ShowingFloor = Floor.activeInHierarchy;
		}
	}

	private void OnDisable() {
		EditorApplication.playModeStateChanged -= PlayModeStateChanged;
	}

	public void PlayModeStateChanged(PlayModeStateChange state) {
		if (state == PlayModeStateChange.ExitingEditMode) {
			// switching to play mode - saving
			Debug.Log("switching to play mode");
			ShowFloor();
			disabled = true;
		}
		else if(state == PlayModeStateChange.EnteredEditMode) {
			disabled = false;
		}
	}

	void OnGUI() {
		EditorGUIUtility.labelWidth = 100;
		EditorGUI.BeginDisabledGroup(disabled);
		bool t = EditorGUILayout.Toggle("Showing Floor", ShowingFloor, "toggle");
		if(t != ShowingFloor) {
			Toggle();
		}
		EditorGUI.EndDisabledGroup();
	}

	public void Toggle() {
		if(ShowingFloor) {
			ShowCeiling();
		}
		else {
			ShowFloor();
		}
	}

	public void ShowCeiling() {
		Ceiling.SetActive(true);
		Floor.SetActive(false);
		Levels.transform.rotation = Quaternion.Euler(180, 0, 0);

		var cpos = Camera.transform.position;
		var crot = Camera.transform.rotation.eulerAngles;
		Camera.transform.position = new Vector3(cpos.x, Mathf.Abs(cpos.y), cpos.z);
		Camera.transform.rotation = Quaternion.Euler(Mathf.Abs(crot.x > 180 ? crot.x - 360 : crot.x), crot.y, crot.z);
		ShowingFloor = false;
	}

	public void ShowFloor() {
		Ceiling.SetActive(false);
		Floor.SetActive(true);
		Levels.transform.rotation = Quaternion.Euler(0,0,0);

		var cpos = Camera.transform.position;
		var crot = Camera.transform.rotation.eulerAngles;
		Camera.transform.position = new Vector3(cpos.x, -Mathf.Abs(cpos.y), cpos.z);
		Camera.transform.rotation = Quaternion.Euler(-Mathf.Abs(crot.x > 180 ? crot.x - 360 : crot.x), crot.y, crot.z);
		ShowingFloor = true;
	}
}
