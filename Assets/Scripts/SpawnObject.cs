using System;
using UnityEngine;

public enum SpawnType {
	Small,
	Jail,
	Laser,
	Blind,
	Special
}

public interface ISpawnable {
	void Init(SpawnProperties props); // can/should be replaced with some sort of factory
	event EventHandler Destroyed;
}

public class SpawnObject {
	public GameObject Object { get; set; }
	public bool ReadyToSpawn { get; set; } = false;
	public SpawnObjectInfo SpawnInfo { get; set; }
	public float SpawnTime { 
		get => SpawnInfo.SpawnTime;
		set => SpawnInfo.SpawnTime = value;
	}

	public SpawnObject(GameObject obj, SpawnObjectInfo info) {
		Object = obj;
		SpawnInfo = info;
	}

	public void Update(float deltaTime) {
		if (ReadyToSpawn) {
			SpawnTime -= deltaTime;
		}
	}

	// Spawn if  1) parentId object defined  &&  (parent died || time since parentSpawn > SpawnTime)
	//           2) parentId object undefined AND time > SpawnTime
	public ISpawnable TrySpawn() {
		return ReadyToSpawn && SpawnTime < 0 ? Spawn() : null;
	}

	public ISpawnable Spawn() {
		ReadyToSpawn = false;
		return UnityEngine.Object.Instantiate(Object).GetComponent<MonoBehaviour>() as ISpawnable;
	}
}