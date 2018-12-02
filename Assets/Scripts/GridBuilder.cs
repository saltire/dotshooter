using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct Point {
	public Vector3 position;
	public List<Point> connections;
}

public class GridBuilder : MonoBehaviour {
	public Transform startingPoint;
	public TankTouch tank;

	Dictionary<Vector2, Point> points;

	void Awake() {
		// Create a lookup table of all points, indexed by local position.
		points = new Dictionary<Vector2, Point>();

		foreach (GameObject pointObj in GameObject.FindGameObjectsWithTag("Point")) {
			points[pointObj.transform.localPosition] = new Point() {
				position = pointObj.transform.position,
				connections = new List<Point>(),
			};
		}

		// For each line, connect the point at its origin with the point touching its collider.
		LayerMask pointMask = LayerMask.GetMask("Points");
		ContactFilter2D pointFilter = new ContactFilter2D();
		pointFilter.SetLayerMask(pointMask);

		foreach (GameObject lineObj in GameObject.FindGameObjectsWithTag("Line")) {
			Point srcPoint = points[lineObj.transform.localPosition];

			Collider2D[] destPoints = new Collider2D[1];
			lineObj.GetComponent<Collider2D>().OverlapCollider(pointFilter, destPoints);
			Point destPoint = points[destPoints[0].transform.localPosition];

			srcPoint.connections.Add(destPoint);
			destPoint.connections.Add(srcPoint);
		}
	}

	void Start() {
		MoveTankToStart();
	}

	public void MoveTankToStart() {
		tank.MoveToPoint(GetPointAtPos(startingPoint.position));
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
