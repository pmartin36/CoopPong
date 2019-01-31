using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreen : MonoBehaviour
{
	public Image background;

    void Start() {
        LevelManager.RemainingLivesChanged += LivesChanged;
		background = GetComponent<Image>();
    }

	private void OnDestroy() {
		LevelManager.RemainingLivesChanged -= LivesChanged;
	}

	public void LivesChanged(object sender, int e) {
		if(e <= 0) {
			StartCoroutine(Darken());
		}
	}

	IEnumerator Darken() {
		Color start = Color.clear;
		Color end = Color.black;

		float time = 0;
		var wait = new WaitForEndOfFrame();

		while(time < 1f) {
			background.color = Color.Lerp(start, end, time);
			time += Time.deltaTime;
			yield return wait;
		}
	}
}
