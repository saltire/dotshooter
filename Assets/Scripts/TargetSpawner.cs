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
	List<GameObject> targets = new List<GameObject>();
	List<GameObject> activeTargets = new List<GameObject>();

	void Awake() {
		ui = (UIManager)FindObjectOfType(typeof(UIManager));
	}

	public void LoadTargetTemplate(TargetTemplate template) {
		// Get the positions of all placeholders in the template, and deactivate the template.
		targetPositions.Clear();
		foreach (Transform placeholder in template.transform) {
			targetPositions.Add(placeholder.localPosition);
		}
		template.gameObject.SetActive(false);
	}

	void Start() {
		SpawnTargets();
	}

	public void SpawnTargets() {
		// Remove any existing targets.
		foreach (GameObject target in targets) {
			Destroy(target);
		}
		targets.Clear();
		activeTargets.Clear();
	
		// Spawn targets at the positions of each placeholder.
		foreach (Vector2 pos in targetPositions) {
			GameObject target = Instantiate<GameObject>(targetPrefab, pos, Quaternion.identity);
			target.transform.parent = transform;

			TargetScript targetScript = target.GetComponent<TargetScript>();
			targetScript.SetColor(colors[Random.Range(0, colors.Length)]);

			targets.Add(target);
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
