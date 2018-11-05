using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(InputManager))]
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

	public void Awake() {
		
	}

	public void Start () {

	}

	public void HandleInput(InputPackage p) {
		ContextManager.HandleInput(p);
	}

	public void ReloadLevel() {
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void ToggleSoundOn() {
		
	}

	public void PlayerLost(string reason) {

	}
}