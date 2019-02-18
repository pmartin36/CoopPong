using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager> {

	private float _timeScale = 1f;
	public float TimeScale {
		get {
			return _timeScale;
		}
		set {
			Time.timeScale = value;
			_timeScale = value;
		}
	}

	public ContextManager ContextManager;
	public LevelManager LevelManager {
		get {
			return ContextManager as LevelManager;
		}
		set {
			ContextManager = value;
		}
	}
	public HouseManager HouseManager {
		get
		{
			return ContextManager as HouseManager;
		}
		set
		{
			ContextManager = value;
		}
	}

	private bool InProgressSceneSwitch = false;

	public PlayerStyle Player1Style;
	public PlayerStyle Player2Style;

	public void Awake() {
		Player1Style = new PlayerStyle(new Color(0.5f, 0.5f, 0.9f));
		Player2Style = new PlayerStyle(Color.green);
	}

	public void HandleInput(InputPackage p) {
		ContextManager.HandleInput(p);
	}

	public void ReloadLevel() {
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public bool EnterPlayableLevel(string sceneName, IEnumerator leavingSceneCoroutine) {
		if(InProgressSceneSwitch) {
			return false;
		}

		StartCoroutine(SwitchScreen(sceneName, leavingSceneCoroutine));
		return true;
	}

	public bool LeavePlayableLevel(string sceneName, IEnumerator leavingSceneCoroutine) {
		if (InProgressSceneSwitch) {
			return false;
		}

		StartCoroutine(SwitchScreen(sceneName, leavingSceneCoroutine));
		return true;
	}

	public void ToggleSoundOn() {
		
	}

	public void PlayerLost(string reason) {

	}

	private IEnumerator SwitchScreen(string sceneName, IEnumerator leavingSceneCoroutine) {
		InProgressSceneSwitch = true;
		var currentScene = SceneManager.GetActiveScene();
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

		asyncLoad.allowSceneActivation = false;
		yield return StartCoroutine(leavingSceneCoroutine);
		yield return new WaitUntil(() => asyncLoad.progress >= 0.9f); //when allowsceneactive is false, progress stops at .9f
		asyncLoad.allowSceneActivation = true;
	}
}