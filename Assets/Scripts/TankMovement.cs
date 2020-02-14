using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankMovement : MonoBehaviour {
	public TankCannon cannon;

	Point lastPoint;
	Point nextPoint;
	bool atLastPoint = true;
	Vector2 startPos;
	Vector2 currentDirection;
	Vector2 moveVelocity = Vector2.zero;
	public float smoothMoveTime = .015f;
	public float maxMoveSpeed = 50;
	public float snapDistance = 0.15f;

	public List<Arrow> arrows { get; private set; } = new List<Arrow>();
	public GameObject arrowPrefab;
	public float arrowDistance = 1.5f;

	LevelManager level;

	void Awake() {
		level = (LevelManager)FindObjectOfType(typeof(LevelManager));
	}

	public void StartMove(Vector2 localTouchPos, Arrow touchingArrow) {
		startPos = localTouchPos;
		currentDirection = touchingArrow == null ? Vector2.zero :
			(Vector2)(touchingArrow.point.position - transform.position);
	}

	public void ContinueMove(Vector2 localTouchPos) {
		Vector2 touchDirection = localTouchPos - startPos;

		// If tank is at a point, pick an adjacent point to move toward.
		if (atLastPoint) {
			float minTouchDiff = 360;
			foreach (Point connectedPoint in lastPoint.connections) {
				Vector2 pointDirection = connectedPoint.position - transform.position;
				float dirDiff = Vector2.Angle(currentDirection, pointDirection);
				float touchDiff = Vector2.Angle(touchDirection, pointDirection);

				// Consider only points whose direction aligns with current direction,
				// then find the one whose direction is closest to the direction of touch movement,
				// and assign it to nextPoint.
				if ((dirDiff == 0 || dirDiff == 180) && touchDiff < minTouchDiff) {
					minTouchDiff = touchDiff;
					nextPoint = connectedPoint;
					atLastPoint = false;
				}
			}
		}

		// Calculate the move distance along the current direction.
		Vector2 nextPointDelta = nextPoint.position - transform.position;
		Vector2 lastPointDelta = transform.position - lastPoint.position;
		float moveDist = Mathf.Clamp(Vector2.Dot(touchDirection, nextPointDelta.normalized),
			-lastPointDelta.magnitude, nextPointDelta.magnitude);

		Vector2 targetPos = (Vector2)transform.position + nextPointDelta.normalized * moveDist;
		Vector2 smoothPos = Vector2.SmoothDamp((Vector2)transform.position, targetPos,
			ref moveVelocity, smoothMoveTime, maxMoveSpeed);

		// Snap to a point if close enough.
		if (!atLastPoint) {
			foreach (Point point in level.GetAllPoints()) {
				if (Vector2.Distance(smoothPos, point.position) <= snapDistance) {
					smoothPos = new Vector2(point.position.x, point.position.y);
					nextPoint = lastPoint;
					lastPoint = point;
					atLastPoint = true;
				}
			}
		}

		transform.position = new Vector3(smoothPos.x, smoothPos.y, transform.position.z);
		UpdateArrows();
		cannon.UpdateMarchingAnts();
	}

	public void EndMove() {
		currentDirection = Vector2.zero;
		UpdateArrows();
	}

	public void MoveToPoint(Point point) {
		lastPoint = point;
		atLastPoint = true;

		transform.position = new Vector3(point.position.x, point.position.y, transform.position.z);
		transform.localEulerAngles = Vector3.zero;

		UpdateArrows();
		cannon.UpdateMarchingAnts();
	}

	void ClearArrows() {
		foreach (Arrow arrow in arrows) {
			Destroy(arrow.gameObject);
		}
		arrows.Clear();
	}

	public void UpdateArrows() {
		ClearArrows();

		// If at a point, show arrows for all connected points.
		// Otherwise, show arrows to the two adjacent points.
		List<Point> arrowPoints = atLastPoint ? lastPoint.connections :
			new List<Point>() { lastPoint, nextPoint };

		foreach (Point arrowPoint in arrowPoints) {
			// Create an arrow pointing to this point.
			float pointAngle = Vector2.SignedAngle(Vector2.up, arrowPoint.position - transform.position);
			GameObject arrow = Instantiate(arrowPrefab,
				transform.position + new Vector3(0, arrowDistance, 0), Quaternion.identity);
			arrow.transform.RotateAround(transform.position, Vector3.forward, pointAngle);
			arrow.transform.parent = transform;
			Arrow arrowScript = arrow.GetComponent<Arrow>();
			arrowScript.point = arrowPoint;
			arrows.Add(arrowScript);

			// If at a point and moving, fade out arrows that aren't aligned with current direction.
			if (atLastPoint && currentDirection != Vector2.zero) {
				Vector2 pointDirection = arrowPoint.position - transform.position;
				float dirDiff = Vector2.Angle(currentDirection, pointDirection);
				if (dirDiff != 0 && dirDiff != 180) {
					Util.SetSpriteAlpha(arrow.GetComponent<SpriteRenderer>(), .25f);
				}
			}
		}
	}
}
