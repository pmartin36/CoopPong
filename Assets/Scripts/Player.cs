using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerSide {
	Left,
	Right
}

public class Player : MonoBehaviour
{
	public float BaseSpeed;

	private float MovementInput;
	private float MoveSpeed;

	private static float yMaximum;

	public float YMove {
		get { return MovementInput * MoveSpeed; }
	}

    // Start is called before the first frame update
    void Start()
    {
        MoveSpeed = BaseSpeed;
		yMaximum = Camera.main.orthographicSize - (transform.localScale.y / 2f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void FixedUpdate() {
		float modifiedY = Mathf.Clamp(transform.position.y + MovementInput * MoveSpeed * Time.fixedDeltaTime, -yMaximum, yMaximum);
		transform.position = new Vector3(transform.position.x, modifiedY);
	}

	public void HandleInput(float vertical, float horizontal, bool horizontalDown) {
		MovementInput = vertical;
	}
}
