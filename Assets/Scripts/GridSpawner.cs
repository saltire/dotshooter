using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSpawner : MonoBehaviour {
	public int rows = 5;
	public int columns = 9;
	public float spacing = 0.5f;
	public Vector2 startPos;

	public GameObject pointPrefab;
	public GameObject linePrefab;
	public GameObject tankPrefab;

	Quaternion right = Quaternion.AngleAxis(-90, Vector3.forward);

	public void SpawnGrid() {
		RemoveGrid();

		Vector3 origin = transform.position -
			new Vector3((float)(columns - 1) / 2 * spacing, (float)(rows - 1) / 2 * spacing);

		for (int x = 0; x < columns; x++) {
			for (int y = 0; y < rows; y++) {
				Vector3 pointPos = origin + new Vector3(x * spacing, y * spacing);

				GameObject point = Instantiate<GameObject>(pointPrefab, pointPos, Quaternion.identity);
				point.transform.parent = transform;

				if (x < columns - 1) {
					GameObject line = Instantiate<GameObject>(linePrefab, pointPos, right);
					line.transform.parent = transform;
				}
				if (y < rows - 1) {
					GameObject line = Instantiate<GameObject>(linePrefab, pointPos, Quaternion.identity);
					line.transform.parent = transform;
				}
			}
		}

		GameObject tank = Instantiate<GameObject>(tankPrefab,
			origin + new Vector3(startPos.x * spacing, startPos.y * spacing, -1), Quaternion.identity);
		tank.transform.parent = transform;
	}

	public void RemoveGrid() {
		while (transform.childCount > 0) {
			DestroyImmediate(transform.GetChild(0).gameObject);
		}
	}
}
