//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//public class Spawner {
	
//	public List<SpawnObject> SpawnObjects;
//	private int SpawnedObjects = 0;
//	private SpawnObject NextSpawn;

//	private GameObject SmallEnemyPrefab;
//	private GameObject JailEnemyPrefab;
//	private GameObject LaserEnemyPrefab;

//	private int spawnIndex;
//	private Dictionary<int, List<int>> dependencies;

//	public Spawner(List<SpawnObjectInfo> spawnInfo) {
//		LoadAllSpawnedObjects();
//		SpawnObjects = spawnInfo.Select( info => ConstructSpawnObjectFromInfo(info) ).ToList();
//		dependencies = new Dictionary<int, List<int>>();
//		for (int i = 0; i < SpawnObjects.Count; i++) {
//			var spawnedObject = SpawnObjects[i];
//			int parentId = spawnedObject.SpawnInfo.ParentId;
//			if (parentId >= 0) {
//				if (dependencies.ContainsKey(parentId)) {
//					dependencies[parentId].Add(i);
//				}
//				else {	
//					dependencies.Add(parentId, new List<int>() { i });
//				}
//			}
//			else {
//				spawnedObject.ReadyToSpawn = true;
//			}
//		}
//	}

//	private SpawnObject ConstructSpawnObjectFromInfo(SpawnObjectInfo info) {
//		GameObject o = null;
//		var si = SpawnObjectInfo.CreateCopyFromAsset(info);
//		switch (si.SpawnType) {
//			case SpawnType.Small:
//				o = SmallEnemyPrefab;
//				break;
//			case SpawnType.Jail:
//				o = JailEnemyPrefab;
//				break;
//			case SpawnType.Laser:
//				o = LaserEnemyPrefab;
//				break;
//			case SpawnType.Blind:
//				break;
//			case SpawnType.Special:
//				break;
//			default:
//				break;
//		}
//		return new SpawnObject(o, si);
//	}

//	public void LoadAllSpawnedObjects() {
//		SmallEnemyPrefab = Resources.Load<GameObject>("Prefabs/Small Enemy");
//		JailEnemyPrefab = Resources.Load<GameObject>("Prefabs/Jail Enemy");
//		LaserEnemyPrefab = Resources.Load<GameObject>("Prefabs/Laser Enemy");
//	}

//	public void Update(float deltaTime) {
//		for (int i = SpawnedObjects; i < SpawnObjects.Count; i++) {
//			SpawnObject obj = SpawnObjects[i];
//			obj.Update(deltaTime);
//			ISpawnable spawn = obj.TrySpawn();
//			if(spawn != null) {
//				// spawn successful
//				spawn.Init(obj.SpawnInfo.Properties);
//				if( dependencies.ContainsKey(i)) {
//					foreach(int d in dependencies[i]) {
//						SpawnObjects[d].ReadyToSpawn = true;
//					}
//				}
//				SpawnedObjects++;
//			}
//		}
//	}
//}