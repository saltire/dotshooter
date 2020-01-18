using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
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
		foreach (Transform target in template.transform) {
			targetPositions.Add(target.localPosition);
			target.GetComponent<TargetScript>().SetColor(colors[Random.Range(0, colors.Length)]);
			activeTargets.Add(target.gameObject);
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
