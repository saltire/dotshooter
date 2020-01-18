using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LevelManager : MonoBehaviour {
	public GameObject tankPrefab;
	public GameObject[] levelPrefabs;

	int currentLevel = 0;
	GameObject level;
	GameObject tank;
	PathBuilder paths;
	TargetSpawner targets;
	UIManager ui;

	void Awake() {
		paths = GetComponent<PathBuilder>();
		targets = GetComponent<TargetSpawner>();
		ui = (UIManager)FindObjectOfType(typeof(UIManager));
	}

	void Start() {
		if (Application.IsPlaying(gameObject)) {
			LoadLevel();
		}
	}

	public void NextLevel() {
		currentLevel = (currentLevel + 1) % levelPrefabs.Length;
		LoadLevel();
	}

	public void LoadLevel() {
		LoadLevel(levelPrefabs[currentLevel]);
	}

	public void LoadLevel(GameObject levelToLoad) {
		ClearLevel();

		level = Instantiate<GameObject>(levelToLoad, transform.position, Quaternion.identity);
		level.transform.parent = transform;

		paths.LoadPathTemplate(level.GetComponentInChildren<PathTemplate>());
		targets.LoadTargetTemplate(level.GetComponentInChildren<TargetTemplate>());

		tank = Instantiate<GameObject>(tankPrefab, tankPrefab.transform.position,
			Quaternion.identity);
		tank.transform.parent = transform;
		TankTouch tankTouch = tank.GetComponent<TankTouch>();
		tankTouch.marchingAnts.gameObject.SetActive(true);
		tankTouch.MoveToPoint(paths.GetStartingPoint());
	}

	public void ClearLevel() {
		DestroyImmediate(GameObject.FindGameObjectWithTag("Level"));
		DestroyImmediate(GameObject.FindGameObjectWithTag("Player"));

		ui.targetCounter.SetCount(0);
		ui.shotCounter.SetCount(0);
	}
}
