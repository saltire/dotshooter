using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
	public GameObject tankPrefab;
	public GameObject[] pathTemplatePrefabs;
	public GameObject[] targetTemplatePrefabs;

	int currentLevel = 0;

	TankTouch tank;
	PathBuilder paths;
	TargetSpawner targets;

	void Awake() {
		paths = GetComponent<PathBuilder>();
		targets = GetComponent<TargetSpawner>();

		paths.LoadPathTemplate(pathTemplatePrefabs[currentLevel]);
		targets.LoadTargetTemplate(targetTemplatePrefabs[currentLevel]);
	}

	void Start() {
		GameObject tankObj = Instantiate<GameObject>(tankPrefab, transform.position, 
			Quaternion.identity);
		tank = tankObj.GetComponent<TankTouch>();
		
		StartLevel();
	}

	public void NextLevel() {
		currentLevel = (currentLevel + 1) % targetTemplatePrefabs.Length;
		paths.LoadPathTemplate(pathTemplatePrefabs[currentLevel]);
		targets.LoadTargetTemplate(targetTemplatePrefabs[currentLevel]);
		StartLevel();
	}

	void StartLevel() {
		paths.MoveTankToStart(tank);
		targets.SpawnTargets();
	}
}
