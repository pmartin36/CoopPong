using System;
using UnityEngine;

public enum SpawnType {
	Slow,
	Jail,
	Blind,
	Boss
}

public interface ISpawnable {
	void Init(SpawnProperties props); // can/should be replaced with some sort of factory
	event EventHandler Destroyed;
}

public class SpawnObjectInfo {
	public int ParentId { get; set; }
	public GameObject Object { get; set; }
	public SpawnProperties Properties { get; set; }
	public SpawnType SpawnType { get; set; }

	public SpawnObjectInfo(int parentId, GameObject obj, SpawnProperties properties, SpawnType spawnType) {
		ParentId = parentId;
		Object = obj;
		Properties = properties;
		SpawnType = spawnType;
	}


}

public class SpawnObject {
	public float SpawnTime { get; set; }
	public bool ReadyToSpawn { get; set; } = false;
	public SpawnObjectInfo SpawnInfo { get; set; }

	public SpawnObject(float spawnTime, SpawnObjectInfo info) {
		SpawnTime = spawnTime;
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
		return UnityEngine.Object.Instantiate(SpawnInfo.Object).GetComponent<MonoBehaviour>() as ISpawnable;
	}
}