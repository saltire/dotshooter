using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSpawner : MonoBehaviour {
	public Color[] colors = {
		Color.red,
		Color.yellow,
		Color.green,
		Color.blue,
	};

	public GameObject targetPrefab;

	UIManager ui;

	List<Vector2> targetPositions = new List<Vector2>();
	List<GameObject> activeTargets = new List<GameObject>();

	void Awake() {
		ui = (UIManager)FindObjectOfType(typeof(UIManager));
	}

	public void LoadTargetTemplate(GameObject templatePrefab) {
		// Get the positions of all placeholders in the template, and remove the template.
		GameObject template = Instantiate<GameObject>(templatePrefab, new Vector3(0, 0, -1000), 
			Quaternion.identity);

		targetPositions.Clear();
		foreach (Transform placeholder in template.transform) {
			targetPositions.Add(placeholder.localPosition);
		}
		Destroy(template);
	}

	void Start() {
		SpawnTargets();
	}

	public void SpawnTargets() {
		// Remove any existing targets.
		foreach (GameObject target in activeTargets) {
			Destroy(target);
		}
		activeTargets.Clear();
	
		// Spawn targets at the positions of each placeholder.
		foreach (Vector2 pos in targetPositions) {
			GameObject target = Instantiate<GameObject>(targetPrefab, pos, Quaternion.identity);
			target.transform.parent = transform;

			TargetScript targetScript = target.GetComponent<TargetScript>();
			targetScript.SetColor(colors[Random.Range(0, colors.Length)]);

			activeTargets.Add(target);
		}

		// Reset the UI counters.
		ui.targetCounter.SetCount(activeTargets.Count);
		ui.shotCounter.SetCount(0);
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
