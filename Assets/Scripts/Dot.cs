using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DotType {
	Normal,
	Required,
	Extra,
	Enemy
}

public class Dot : MonoBehaviour
{
	public DotType DotType;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void TryDestroy(GameObject collision) {
		OnDestroyEffect(collision);
		this.gameObject.Destroy();
	}

	public virtual void OnDestroyEffect(GameObject collision) {
		switch (DotType) {		
			case DotType.Required:
				// remove from required
				break;
			case DotType.Extra:
				// award bonus star
				break;
			case DotType.Enemy: // spawn enemy
			case DotType.Normal: // chance to drop powerup
				// handled in derived classes
				break;
		}
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		if(collision.CompareTag("Ball")) {
			TryDestroy(collision.gameObject);
		}
	}
}
