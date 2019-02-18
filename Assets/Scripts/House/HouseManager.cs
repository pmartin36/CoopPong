using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(HouseInputManager))]
public class HouseManager : ContextManager
{
	public HousePlayer Player1;
	public HousePlayer Player2;

	[SerializeField]
	private Image Cover;

	public override void Start() {
		base.Start();
		if (Player1 != null) {
			Player1.SetStyle(GameManager.Instance.Player1Style);
		}
		if (Player2 != null) {
			Player2.SetStyle(GameManager.Instance.Player2Style);
		}
	}

	public override void HandleInput(InputPackage housePackage) {
		var p = housePackage as HouseInputPackage;
		if(Player1 != null) {
			Player1.HandleInput(p.LeftPlayerVertical, p.LeftPlayerHorizontal, p.LeftPlayerStart, p.LeftPlayerBoost, p.LeftPlayerCtrl);
		}
		if(Player2 != null) {
			Player2.HandleInput(p.RightPlayerVertical, p.RightPlayerVertical, p.RightPlayerStart, p.RightPlayerBoost, p.RightPlayerCtrl);
		}
	}

	public void EnterLevel(string sceneName) {
		GameManager.Instance.EnterPlayableLevel(sceneName, ShowCover());
	}

	IEnumerator ShowCover() {
		Color start = Color.clear;
		Color end = Color.black;

		float time = 0;

		while (time < 1f) {
			Cover.color = Color.Lerp(start, end, time);
			time += Time.deltaTime;
			yield return null;
		}
	}
}
