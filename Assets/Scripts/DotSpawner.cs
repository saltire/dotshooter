using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotSpawner : MonoBehaviour {
	public int rows = 5;
	public int columns = 9;
	public float spacing = 0.5f;

	public Color[] colors = {
		Color.red,
		Color.yellow,
		Color.green,
		Color.blue,
	};

	public GameObject dotPrefab;

	public void SpawnDots() {
		RemoveDots();

		Vector3 origin = transform.position -
			new Vector3((float)(columns - 1) / 2 * spacing, (float)(rows - 1) / 2 * spacing);

		for (int x = 0; x < columns; x++) {
			for (int y = 0; y < rows; y++) {
				GameObject dot = Instantiate<GameObject>(dotPrefab,
					origin + new Vector3(x * spacing, y * spacing), Quaternion.identity);
				dot.transform.parent = transform;

				dot.GetComponent<DotScript>().SetColor(colors[Random.Range(0, colors.Length)]);
			}
		}
	}

	public void RemoveDots() {
		while (transform.childCount > 0) {
			DestroyImmediate(transform.GetChild(0).gameObject);
		}
	}
}
