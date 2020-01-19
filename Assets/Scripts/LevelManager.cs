using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct Point {
	public Vector3 position;
	public List<Point> connections;
}

[ExecuteAlways]
public class LevelManager : MonoBehaviour {
	public GameObject[] levelPrefabs;
	public GameObject tankPrefab;
	public GameObject targetPrefab;

	public Color[] targetColors = {
		Color.red,
		Color.yellow,
		Color.green,
		Color.blue,
	};

	int currentLevel = 0;

	UIManager ui;

	Dictionary<Vector2, Point> points = new Dictionary<Vector2, Point>();

	List<GameObject> targets = new List<GameObject>();

	void Awake() {
		ui = (UIManager)FindObjectOfType(typeof(UIManager));
	}

	void Start() {
		if (Application.IsPlaying(gameObject)) {
			LoadLevel();
		}
	}

	public void NextLevel() {
		currentLevel = (currentLevel + 1) % levelPrefabs.Length;
		LoadLevel();
	}

	public void LoadLevel() {
		LoadLevel(levelPrefabs[currentLevel]);
	}

	public void LoadLevel(GameObject levelToLoad) {
		ClearLevel();

		GameObject level = Instantiate<GameObject>(
			levelToLoad, transform.position, Quaternion.identity);
		level.transform.parent = transform;

		PathTemplate pathTemplate = level.GetComponentInChildren<PathTemplate>();
		LoadPoints(pathTemplate);

		SpawnTank(GetPointAtPos(pathTemplate.startingPoint.position));
		SpawnTargets(level.GetComponentInChildren<TargetTemplate>());

		ui.targetCounter.SetCount(targets.Count);
		ui.shotCounter.SetCount(0);
	}

	public void ClearLevel() {
		DestroyImmediate(GameObject.FindGameObjectWithTag("Level"));
		DestroyImmediate(GameObject.FindGameObjectWithTag("Player"));

		ui.targetCounter.SetCount(0);
		ui.shotCounter.SetCount(0);
	}

	// Path points

	void LoadPoints(PathTemplate template) {
		points.Clear();

		// Create a lookup table of all points, indexed by local position.
		foreach (Transform point in template.transform) {
			if (point.CompareTag("Point")) {
				points[point.localPosition] = new Point() {
					position = point.position,
					connections = new List<Point>(),
				};
			}
		}

		// For each line, find and connect the points touching its collider.
		LayerMask pointMask = LayerMask.GetMask("Points");
		ContactFilter2D pointFilter = new ContactFilter2D();
		pointFilter.SetLayerMask(pointMask);

		foreach (Transform line in template.transform) {
			if (line.CompareTag("Line")) {
				Vector2 lineVector = line.localRotation * line.localScale;
				CapsuleCollider2D lineCollider = line.GetComponent<CapsuleCollider2D>();
				lineCollider.size = new Vector2(1, 1 + 1 / line.localScale.y);

				// Find all point colliders touching the line.
				Collider2D[] pointColls = new Collider2D[10];
				lineCollider.OverlapCollider(pointFilter, pointColls);

				// Convert to a list of points touching the line, ordered by their position along the line.
				Point[] linePoints = pointColls
					.Where(coll => coll != null && coll.transform.parent == template.transform)
					.Select(coll => points[coll.transform.localPosition])
					.OrderBy(point => Vector2.Dot(point.position - line.position, lineVector))
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
	}

	public List<Point> GetAllPoints() {
		return points.Values.ToList();
	}

	public Point GetPointAtPos(Vector3 worldPos) {
		Vector2 localPos = transform.InverseTransformPoint(worldPos);

		if (points.ContainsKey(localPos)) {
			return points[localPos];
		}

		throw new System.Exception("No point at position " + worldPos);
	}

	// Tank

	void SpawnTank(Point startingPoint) {
		GameObject tank = Instantiate<GameObject>(
			tankPrefab, tankPrefab.transform.position, Quaternion.identity);
		tank.transform.parent = transform;
		TankTouch tankTouch = tank.GetComponent<TankTouch>();
		tankTouch.marchingAnts.gameObject.SetActive(true);
		tankTouch.MoveToPoint(startingPoint);
	}

	// Targets

	void SpawnTargets(TargetTemplate template) {
		targets.Clear();

		foreach (Transform target in template.transform) {
			targets.Add(target.gameObject);
			target.GetComponent<TargetScript>()
				.SetColor(targetColors[Random.Range(0, targetColors.Length)]);
		}
	}

	public void DestroyTarget(TargetScript target) {
		target.Explode();

		targets.Remove(target.gameObject);
		ui.targetCounter.IncrementCount(-1);

		if (targets.Count == 0) {
			ui.successPanel.SetActive(true);
		}
	}
}
