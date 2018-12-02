using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetTemplate : MonoBehaviour {
	public GameObject targetPrefab;

	public void SpawnPlaceholdersOnGrid(int rows, int columns, float spacing) {
		while (transform.childCount > 0) {
			DestroyImmediate(transform.GetChild(0).gameObject);
		}

		// Spawn placeholders in a grid pattern.
		Vector3 origin = transform.position -
			new Vector3((float)(columns - 1) / 2 * spacing, (float)(rows - 1) / 2 * spacing);

		for (int x = 0; x < columns; x++) {
			for (int y = 0; y < rows; y++) {
				GameObject target = Instantiate<GameObject>(targetPrefab,
					origin + new Vector3(x * spacing, y * spacing), Quaternion.identity);
				target.transform.parent = transform;
			}
		}
	}

	public void RemovePlaceholders() {
		while (transform.childCount > 0) {
			DestroyImmediate(transform.GetChild(0).gameObject);
		}
	}
}
