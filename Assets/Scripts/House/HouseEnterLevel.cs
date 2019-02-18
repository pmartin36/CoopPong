using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HouseEnterLevel : MonoBehaviour
{
	public string Level;

	public void OnTriggerEnter2D(Collider2D collision) {
		Debug.Log("Can enter level " + Level);
	}

	public void OnTriggerExit2D(Collider2D collision) {
		Debug.Log("Left area to enter level " + Level);
	}
}
