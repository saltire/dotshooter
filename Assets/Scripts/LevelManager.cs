using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
	public GameObject[] levelTemplatePrefabs;

	int currentLevel = 0;

	TargetSpawner spawner;

	void Awake() {
		spawner = GetComponent<TargetSpawner>();
		spawner.LoadTemplate(levelTemplatePrefabs[currentLevel]);
	}

	void Start() {
		StartLevel();
	}

	public void NextLevel() {
		currentLevel = (currentLevel + 1) % levelTemplatePrefabs.Length;
		spawner.LoadTemplate(levelTemplatePrefabs[currentLevel]);
		StartLevel();
	}

	void StartLevel() {
		spawner.SpawnTargets();
	}
}
