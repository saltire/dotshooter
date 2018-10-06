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
	public float maxAngle = 90;

	Point lastPoint;
	Point nextPoint;
	bool atPoint = true;

	TouchState touchState = TouchState.IDLE;
	int fingerId;

	Camera cam;
	GridSpawner grid;
	GameObject gun;

	void Start() {
		cam = (Camera)FindObjectOfType(typeof(Camera));
		grid = (GridSpawner)FindObjectOfType(typeof(GridSpawner));
		gun = GameObject.FindGameObjectWithTag("Gun");

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
							gun.transform.TransformPoint(turnHandleOffset));

						float moveHandleDist = Vector2.Distance(touchPos,
							transform.TransformPoint(moveHandleOffset));

						if (fireButtonDist < fireButtonRadius) {
							Debug.Log("FIRE!");
						}
						else if (turnHandleDist < turnHandleRadius) {
							touchState = TouchState.TURNING;
							fingerId = touch.fingerId;
						}
						else if (moveHandleDist < moveHandleRadius) {
							touchState = TouchState.MOVING;
							fingerId = touch.fingerId;
						}
					}
				}
			}
			else {
				foreach (Touch touch in Input.touches) {
					if (touch.fingerId == fingerId) {
						if (touch.phase == TouchPhase.Ended) {
							touchState = TouchState.IDLE;
							fingerId = -1;
						}
						else if (touch.phase == TouchPhase.Moved) {
							if (touchState == TouchState.TURNING) {
								TurnTank(cam.ScreenToWorldPoint(touch.position));
							}
							else if (touchState == TouchState.MOVING) {
								MoveTank(cam.ScreenToWorldPoint(touch.position));
							}
						}
					}
				}
			}
		}
	}

	void TurnTank(Vector2 targetPos) {
		float targetAngle = Vector2.SignedAngle(Vector2.up, targetPos - (Vector2)transform.position);
		gun.transform.localEulerAngles = new Vector3(0, 0,
			Mathf.Clamp(targetAngle, -maxAngle, maxAngle));
	}

	void MoveTank(Vector2 targetPos) {
		Vector2 handlePos = transform.TransformPoint(moveHandleOffset);
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
		float moveDist = Mathf.Clamp(Vector2.Dot(deltaPos, nextPointDelta.normalized),
			-lastPointDelta.magnitude, nextPointDelta.magnitude);
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
