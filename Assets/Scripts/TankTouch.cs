using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum TouchState {
	IDLE,
	TURNING,
	MOVING,
};

public class TankTouch : MonoBehaviour {
	public float fireButtonRadius = 0.3f;
	public Vector2 turnHandleOffset;
	public float turnHandleRadius = 0.3f;
	public Vector2 moveHandleOffset;
	public float moveHandleRadius = 0.3f;

	public float snapDistance = 0.2f;

	Point lastPoint;
	Point nextPoint;
	bool atPoint = true;

	TouchState touchState = TouchState.IDLE;
	int fingerId;

	Camera cam;
	GridSpawner grid;

	void Start() {
		cam = (Camera)FindObjectOfType(typeof(Camera));
		grid = (GridSpawner)FindObjectOfType(typeof(GridSpawner));
		lastPoint = grid.GetPointAtPos(transform.position);
	}

	void Update() {
		if (Input.touchCount > 0) {
			if (touchState == TouchState.IDLE) {
				foreach (Touch touch in Input.touches) {
					if (touchState == TouchState.IDLE && touch.phase == TouchPhase.Began) {
						Vector3 touchPos = cam.ScreenToWorldPoint(touch.position);

						float fireButtonDist = Vector2.Distance(touchPos, transform.position);

						float turnHandleDist = Vector2.Distance(touchPos,
							(Vector2)transform.position + turnHandleOffset);

						float moveHandleDist = Vector2.Distance(touchPos,
							(Vector2)transform.position + moveHandleOffset);

						if (fireButtonDist < fireButtonRadius) {
							Debug.Log("FIRE!");
						}
						else if (turnHandleDist < turnHandleRadius) {
							Debug.Log("Turning");
							touchState = TouchState.TURNING;
						}
						else if (moveHandleDist < moveHandleRadius) {
							Debug.Log("Moving");
							touchState = TouchState.MOVING;
							fingerId = touch.fingerId;
						}
					}
				}
			}
			else if (touchState == TouchState.MOVING) {
				foreach (Touch touch in Input.touches) {
					if (touch.fingerId == fingerId) {
						if (touch.phase == TouchPhase.Ended) {
							touchState = TouchState.IDLE;
							fingerId = -1;
						}
						else if (touch.phase == TouchPhase.Moved) {
							MoveTank(cam.ScreenToWorldPoint(touch.position));
						}
					}
				}
			}
		}
	}

	void MoveTank(Vector2 targetPos) {
		Vector2 handlePos = (Vector2)transform.position + moveHandleOffset;
		Vector2 deltaPos = targetPos - handlePos;

		if (atPoint) {
			float minAngle = 360;
			foreach (Point connectedPoint in lastPoint.connections) {
				float pointAngle = Vector2.Angle(deltaPos, connectedPoint.position - transform.position);
				if (pointAngle < minAngle) {
					minAngle = pointAngle;
					nextPoint = connectedPoint;
					atPoint = false;
				}
			}
		}

		Vector2 nextPointDelta = nextPoint.position - transform.position;
		Vector2 lastPointDelta = transform.position - lastPoint.position;
		float moveDistUnclamped = Vector2.Dot(deltaPos, nextPointDelta.normalized);
		float moveDist = Mathf.Clamp(Vector2.Dot(deltaPos, nextPointDelta.normalized),
			-lastPointDelta.magnitude, nextPointDelta.magnitude);
		Debug.Log(moveDistUnclamped + " " + moveDist);
		Vector2 move = nextPointDelta.normalized * moveDist;
		transform.position += (Vector3)move;

		foreach (Point point in grid.points.Values) {
			if (Vector2.Distance(transform.position, point.position) <= snapDistance) {
				transform.position = new Vector3(point.position.x, point.position.y, transform.position.z);
				lastPoint = point;
				atPoint = true;
			}
		}
	}
}
