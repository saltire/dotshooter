using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct Point {
	public Vector3 position;
	public List<Point> connections;
}

public class PathBuilder : MonoBehaviour {
	Dictionary<Vector2, Point> points = new Dictionary<Vector2, Point>();
	GameObject template;

	public void LoadPathTemplate(GameObject templatePrefab) {
		Destroy(template);

		// Get the positions of all placeholders in the template, and remove the template.
		template = Instantiate<GameObject>(templatePrefab, transform.position, Quaternion.identity);
		template.transform.parent = transform;

		// Create a lookup table of all points, indexed by local position.
		points.Clear();
		foreach (Transform pointObj in template.transform) {
			if (pointObj.CompareTag("Point")) {
				points[pointObj.localPosition] = new Point() {
					position = pointObj.position,
					connections = new List<Point>(),
				};
			}
		}

		// For each line, connect the point at its origin with the point touching its collider.
		LayerMask pointMask = LayerMask.GetMask("Points");
		ContactFilter2D pointFilter = new ContactFilter2D();
		pointFilter.SetLayerMask(pointMask);

		foreach (GameObject lineObj in GameObject.FindGameObjectsWithTag("Line")) {
			Vector2 lineVector = lineObj.transform.localRotation * lineObj.transform.localScale;

			// Find all point colliders touching the line.
			Collider2D[] lineColls = new Collider2D[10];
			lineObj.GetComponent<Collider2D>().OverlapCollider(pointFilter, lineColls);

			// Convert to a list of points touching the line, ordered by their position along the line.
			Point[] linePoints = lineColls
				.Where(coll => coll != null)
				.Select(coll => points[coll.transform.localPosition])
				.OrderBy(point => Vector2.Dot(point.position - lineObj.transform.position, lineVector))
				.ToArray();

			// Connect each point to its adjacent points.
			for (int i = 0; i < linePoints.Length; i++) {
				if (i > 0) {
					linePoints[i].connections.Add(linePoints[i - 1]);
				}
				if (i < linePoints.Length - 1) {
					linePoints[i].connections.Add(linePoints[i + 1]);
				}
			}
		}
	}

	public void MoveTankToStart(TankTouch tank) {
		tank.MoveToPoint(GetPointAtPos(template.GetComponent<PathTemplate>().startingPoint.position));
	}

	public List<Point> GetAllPoints() {
		return points.Values.ToList();
	}

	public Point GetPointAtPos(Vector3 worldPos) {
		Vector2 localPos = transform.InverseTransformPoint(worldPos);

		if (points.ContainsKey(localPos)) {
			return points[localPos];
		}

		throw new Exception("No point at position " + worldPos);
	}
}
