using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetSpawner : MonoBehaviour {
	public int rows = 5;
	public int columns = 9;
	public float spacing = 0.5f;
	public Counter counter;

	public Color[] colors = {
		Color.red,
		Color.yellow,
		Color.green,
		Color.blue,
	};

	public GameObject targetPrefab;

	void Awake() {
		SpawnTargets();
	}

	void Start() {
		counter.SetCount(rows * columns);
	}

	public void SpawnTargets() {
		RemoveTargets();

		Vector3 origin = transform.position -
			new Vector3((float)(columns - 1) / 2 * spacing, (float)(rows - 1) / 2 * spacing);

		for (int x = 0; x < columns; x++) {
			for (int y = 0; y < rows; y++) {
				GameObject target = Instantiate<GameObject>(targetPrefab,
					origin + new Vector3(x * spacing, y * spacing), Quaternion.identity);
				target.transform.parent = transform;

				TargetScript targetScript = target.GetComponent<TargetScript>();
				targetScript.SetColor(colors[Random.Range(0, colors.Length)]);
				targetScript.counter = counter;
			}
		}
	}

	public void RemoveTargets() {
		while (transform.childCount > 0) {
			DestroyImmediate(transform.GetChild(0).gameObject);
		}
	}
}
