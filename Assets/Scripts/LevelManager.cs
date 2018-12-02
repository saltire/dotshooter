using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
	public GameObject tankPrefab;
	public GameObject[] levelPrefabs;

	int currentLevel = 0;
	GameObject level;

	TankTouch tank;
	PathBuilder paths;
	TargetSpawner targets;

	void Awake() {
		paths = GetComponent<PathBuilder>();
		targets = GetComponent<TargetSpawner>();

		LoadLevel();
	}

	void Start() {
		GameObject tankObj = Instantiate<GameObject>(tankPrefab, tankPrefab.transform.position, 
			Quaternion.identity);
		tank = tankObj.GetComponent<TankTouch>();
		
		StartLevel();
	}

	public void NextLevel() {
		currentLevel = (currentLevel + 1) % levelPrefabs.Length;
		LoadLevel();
		StartLevel();
	}

	void LoadLevel() {
		Destroy(level);

		level = Instantiate<GameObject>(levelPrefabs[currentLevel], transform.position,
			Quaternion.identity);
		level.transform.parent = transform;

		paths.LoadPathTemplate(level.GetComponentInChildren<PathTemplate>());
		targets.LoadTargetTemplate(level.GetComponentInChildren<TargetTemplate>());
	}

	void StartLevel() {
		paths.MoveTankToStart(tank);
		targets.SpawnTargets();
	}
}
