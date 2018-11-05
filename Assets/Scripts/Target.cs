using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GenerateRandomPosition();
    }

    public void GenerateRandomPosition() {
		transform.position = new Vector2( Random.Range(-15f, 15f), Random.Range(-8f, 8f) );
	}

	public void OnTriggerEnter2D(Collider2D collision) {
		GenerateRandomPosition();
	}
}
