using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpawnProperties : ScriptableObject { }
[Serializable]
public class SpawnExport : ScriptableObject {
	public SpawnObjectInfo[] SpawnInfo;
}

public class Spawner {
	
	public List<SpawnObject> SpawnObjects;
	private SpawnObject NextSpawn;

	private GameObject SlowEnemyPrefab;
	private GameObject JailEnemyPrefab;

	private int spawnIndex;
	private Dictionary<int, List<int>> dependencies;

	public Spawner() {
		SpawnObjects = new List<SpawnObject>();
		LoadAllSpawnedObjects();

		Vector3 pos = new Vector3(UnityEngine.Random.Range(-12f, 12f), UnityEngine.Random.Range(-8f, 8f));
		SpawnObjects.Add(
			new SpawnObject(SlowEnemyPrefab, new SpawnObjectInfo(-1, 1, new SlowEnemyProperties(pos), SpawnType.Slow))
		);
		pos = new Vector3(UnityEngine.Random.Range(-12f, 12f), UnityEngine.Random.Range(-8f, 8f));
		SpawnObjects.Add(
			new SpawnObject(JailEnemyPrefab, new SpawnObjectInfo(-1, 5, new JailEnemyProperties(pos), SpawnType.Jail))
		);

		dependencies = new Dictionary<int, List<int>>();
		for(int i = 0; i < SpawnObjects.Count; i++) {
			var spawnedObject = SpawnObjects[i];
			int parentId = spawnedObject.SpawnInfo.ParentId;
			if (parentId >= 0) {
				if( dependencies.ContainsKey(parentId) ) {
					dependencies.Add(parentId, new List<int>() { i });
				}
				else {
					dependencies[parentId].Add(i);
				}
			}
			else {
				spawnedObject.ReadyToSpawn = true;
			}
		}
	}

	public void LoadAllSpawnedObjects() {
		SlowEnemyPrefab = Resources.Load<GameObject>("Prefabs/Slow Enemy");
		JailEnemyPrefab = Resources.Load<GameObject>("Prefabs/Jail Enemy");
	}

	public void Update(float deltaTime) {
		for (int i = 0; i < SpawnObjects.Count; i++) {
			SpawnObject obj = SpawnObjects[i];
			obj.Update(deltaTime);
			ISpawnable spawn = obj.TrySpawn();
			if(spawn != null) {
				// spawn successful
				spawn.Init(obj.SpawnInfo.Properties); // switch to correct spawn properties object
				if( dependencies.ContainsKey(i)) {
					for(int j = 0; j < dependencies.Count; j++) {
						SpawnObjects[j].ReadyToSpawn = true;
					}
				}
			}
		}
	}
}