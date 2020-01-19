using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LevelManager : MonoBehaviour {
	public GameObject[] levelPrefabs;
	public GameObject tankPrefab;
	public GameObject targetPrefab;

	public Color[] targetColors = {
		Color.red,
		Color.yellow,
		Color.green,
		Color.blue,
	};

	int currentLevel = 0;
	PathBuilder paths;
	UIManager ui;

	List<Vector2> targetPositions = new List<Vector2>();
	List<GameObject> activeTargets = new List<GameObject>();

	void Awake() {
		paths = GetComponent<PathBuilder>();
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

		GameObject level = Instantiate<GameObject>(
			levelToLoad, transform.position, Quaternion.identity);
		level.transform.parent = transform;

		paths.LoadPathTemplate(level.GetComponentInChildren<PathTemplate>());
		SpawnTank(paths.GetStartingPoint());
		SpawnTargets(level.GetComponentInChildren<TargetTemplate>());

		ui.targetCounter.SetCount(activeTargets.Count);
		ui.shotCounter.SetCount(0);
	}

	public void ClearLevel() {
		DestroyImmediate(GameObject.FindGameObjectWithTag("Level"));
		DestroyImmediate(GameObject.FindGameObjectWithTag("Player"));

		ui.targetCounter.SetCount(0);
		ui.shotCounter.SetCount(0);
	}

	void SpawnTank(Point startingPoint) {
		GameObject tank = Instantiate<GameObject>(
			tankPrefab, tankPrefab.transform.position, Quaternion.identity);
		tank.transform.parent = transform;
		TankTouch tankTouch = tank.GetComponent<TankTouch>();
		tankTouch.marchingAnts.gameObject.SetActive(true);
		tankTouch.MoveToPoint(startingPoint);
	}

	void SpawnTargets(TargetTemplate template) {
		targetPositions.Clear();
		activeTargets.Clear();

		foreach (Transform target in template.transform) {
			targetPositions.Add(target.localPosition);
			activeTargets.Add(target.gameObject);
			target.GetComponent<TargetScript>()
				.SetColor(targetColors[Random.Range(0, targetColors.Length)]);
		}
	}

	public void DestroyTarget(TargetScript target) {
		target.Explode();

		activeTargets.Remove(target.gameObject);
		ui.targetCounter.IncrementCount(-1);

		if (activeTargets.Count == 0) {
			ui.successPanel.SetActive(true);
		}
	}
}
