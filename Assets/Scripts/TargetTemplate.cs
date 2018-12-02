using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetTemplate : MonoBehaviour {
	public GameObject targetPrefab;

	public void SpawnPlaceholdersOnGrid(Vector2 origin, int rows, int columns, float spacing) {
		RemovePlaceholders();

		// Spawn placeholders in a grid pattern.
		Vector2 topLeft = (Vector2)transform.position + origin -
			new Vector2((float)(columns - 1) / 2 * spacing, (float)(rows - 1) / 2 * spacing);

		for (int x = 0; x < columns; x++) {
			for (int y = 0; y < rows; y++) {
				GameObject target = Instantiate<GameObject>(targetPrefab,
					topLeft + new Vector2(x * spacing, y * spacing), Quaternion.identity);
				target.transform.parent = transform;
			}
		}
	}

	public void SpawnPlaceholdersOnHex(Vector2 origin, int radius, float spacing) {
		RemovePlaceholders();

		Vector2 center = (Vector2)transform.position + origin;
		Vector2 edge = Quaternion.Euler(0, 0, 30) * Vector2.up * spacing;

		if (radius > 0) {
			GameObject target = Instantiate<GameObject>(targetPrefab, center, Quaternion.identity);
			target.transform.parent = transform;
		}

		for (int r = 1; r < radius; r++) {
			for (int i = 0; i < 6; i++) {
				GameObject target = Instantiate<GameObject>(targetPrefab,
					center + Vector2.right * spacing * r, Quaternion.identity);
				target.transform.rotation = Quaternion.identity;
				target.transform.parent = transform;

				for (int j = 1; j < r; j++) {
					GameObject neighbor = Instantiate<GameObject>(targetPrefab,
						(Vector2)target.transform.position + edge * j, Quaternion.identity);
					neighbor.transform.rotation = Quaternion.identity;
					neighbor.transform.parent = transform;
					neighbor.transform.RotateAround(center, Vector3.forward, 60 * i);
				}
				
				target.transform.RotateAround(center, Vector3.forward, 60 * i);
			}
		}
	}

	public void RemovePlaceholders() {
		while (transform.childCount > 0) {
			DestroyImmediate(transform.GetChild(0).gameObject);
		}
	}
}
