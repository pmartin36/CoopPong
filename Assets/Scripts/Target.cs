using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
	SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
		spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void GenerateRandomPosition() {
		transform.position = new Vector2( Random.Range(-15f, 15f), Random.Range(-8f, 8f) );
		spriteRenderer.color = new Color( Random.Range(0.7f, 1f), Random.Range(0.7f, 1f), Random.Range(0.7f, 1f));
	}

	public void OnTriggerEnter2D(Collider2D collision) {
		GenerateRandomPosition();
	}
}
