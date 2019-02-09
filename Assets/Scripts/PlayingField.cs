using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayingField : MonoBehaviour
{
	Transform boundaries;
	Transform boundaryTop;
	Transform boundaryBottom;
	public MinMax MinMax { get; private set; }

	public void OnEnable() {
		boundaries = transform.Find("Boundaries");
		boundaryBottom = boundaries.Find("Boundary Bottom");
		boundaryTop = boundaries.Find("Boundary Top");

		CalculateBoundaries();
	}

	public void CalculateBoundaries() {
		var actualTop = boundaryTop.position.y > boundaryBottom.position.y ? boundaryTop : boundaryBottom;
		var actualBottom = boundaryTop == actualTop ? boundaryBottom : boundaryTop;

		if (GameManager.Instance.LevelManager != null) {
			GameManager.Instance.LevelManager.PlayingField = this;
		}
		MinMax = new MinMax(
			actualBottom.position.y + 0.5f,
			actualTop.position.y - 0.5f
		);
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
