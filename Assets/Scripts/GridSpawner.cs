using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Point {
	public int x;
	public int y;
	public Transform transform;
	public Vector3 position;
	public List<Point> connections;
}

public class GridSpawner : MonoBehaviour {
	public int rows = 5;
	public int columns = 9;
	public float spacing = 0.5f;
	public Vector2Int startPos;

	public GameObject pointPrefab;
	public GameObject linePrefab;
	public Transform tank;

	public Dictionary<Vector2Int, Point> points { get; private set; }

	Quaternion right = Quaternion.AngleAxis(-90, Vector3.forward);

	public void SpawnGrid() {
		RemoveGrid();

		Vector3 origin = transform.TransformPoint(GetLocalOrigin());

		for (int x = 0; x < columns; x++) {
			for (int y = 0; y < rows; y++) {
				Vector3 pointPos = origin + new Vector3(x, y) * spacing;

				GameObject pointObj = Instantiate<GameObject>(pointPrefab, pointPos, Quaternion.identity);
				pointObj.transform.parent = transform;

				if (x < columns - 1) {
					GameObject line = Instantiate<GameObject>(linePrefab, pointPos, right);
					line.transform.localScale = new Vector3(1, spacing, 1);
					line.transform.parent = transform;
				}
				if (y < rows - 1) {
					GameObject line = Instantiate<GameObject>(linePrefab, pointPos, Quaternion.identity);
					line.transform.localScale = new Vector3(1, spacing, 1);
					line.transform.parent = transform;
				}
			}
		}

		tank.position = origin + new Vector3(
			Mathf.Clamp(startPos.x, 0, columns - 1) * spacing,
			Mathf.Clamp(startPos.y, 0, rows - 1) * spacing,
			tank.position.z);
	}

	public void RemoveGrid() {
		while (transform.childCount > 0) {
			DestroyImmediate(transform.GetChild(0).gameObject);
		}
	}

	public void Awake() {
		points = new Dictionary<Vector2Int, Point>();

		foreach (GameObject pointObj in GameObject.FindGameObjectsWithTag("Point")) {
			Vector2 pos = GetLocalGridPos(pointObj.transform.position);
			Vector2Int posInt = new Vector2Int((int)pos.x, (int)pos.y);
			points[posInt] = new Point() {
				x = posInt.x,
				y = posInt.y,
				position = pointObj.transform.position,
				connections = new List<Point>(),
			};
		}

		foreach (KeyValuePair<Vector2Int, Point> kv in points) {
			Point point = kv.Value;

			if (point.x > 0) {
				point.connections.Add(points[new Vector2Int(point.x - 1, point.y)]);
			}
			if (point.x < columns - 1) {
				point.connections.Add(points[new Vector2Int(point.x + 1, point.y)]);
			}
			if (point.y > 0) {
				point.connections.Add(points[new Vector2Int(point.x, point.y - 1)]);
			}
			if (point.y < rows - 1) {
				point.connections.Add(points[new Vector2Int(point.x, point.y + 1)]);
			}
		}
	}

	Vector3 GetLocalOrigin() {
		return new Vector3(-(float)(columns - 1) / 2 * spacing, -(float)(rows - 1) / 2 * spacing);
	}

	Vector2 GetLocalGridPos(Vector3 worldPos) {
		Vector3 origin = GetLocalOrigin();
		return (transform.InverseTransformPoint(worldPos) - origin) / spacing;
	}

	public Point GetPointAtPos(Vector3 pos) {
		Vector3 localPos = GetLocalGridPos(pos);
		Vector2Int posInt = new Vector2Int((int)localPos.x, (int)localPos.y);

		if (points.ContainsKey(posInt)) {
			return points[posInt];
		}

		throw new Exception("No point at position " + pos);
	}
}
