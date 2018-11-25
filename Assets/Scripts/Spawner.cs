using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Spawner {
	
	public List<SpawnObject> SpawnObjects;
	private SpawnObject NextSpawn;

	private GameObject SlowEnemyPrefab;
	private GameObject JailEnemyPrefab;

	private int spawnIndex;
	private Dictionary<int, List<int>> dependencies;

	public Spawner(SpawnObjectInfo[] spawnInfo) {
		LoadAllSpawnedObjects();
		SpawnObjects = spawnInfo.Select( info => ConstructSpawnObjectFromInfo(info) ).ToList();
		dependencies = new Dictionary<int, List<int>>();
		for (int i = 0; i < SpawnObjects.Count; i++) {
			var spawnedObject = SpawnObjects[i];
			int parentId = spawnedObject.SpawnInfo.ParentId;
			if (parentId >= 0) {
				if (dependencies.ContainsKey(parentId)) {
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

	private SpawnObject ConstructSpawnObjectFromInfo(SpawnObjectInfo info) {
		GameObject o = null;
		switch (info.SpawnType) {
			case SpawnType.Slow:
				o = SlowEnemyPrefab;
				break;
			case SpawnType.Jail:
				o = JailEnemyPrefab;
				break;
			case SpawnType.Blind:
				break;
			case SpawnType.Boss:
				break;
			default:
				break;
		}
		return new SpawnObject(o, info);
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