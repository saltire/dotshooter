using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum TouchState {
	IDLE,
	TURNING,
	MOVING,
};

public class TankTouch : MonoBehaviour {
	public Transform tankBottom;
	public Collider2D fireTrigger;
	public Collider2D turnTrigger;
	public Collider2D moveTrigger;

	public float smoothTurnTime = .1f;
	public float maxTurnSpeed = 500;
	public float maxAngle = 90;

	public float smoothMoveTime = .015f;
	public float maxMoveSpeed = 50;
	public float snapDistance = 0.15f;

	float turnVelocity = 0;
	Vector2 moveVelocity = Vector2.zero;

	Point lastPoint;
	Point nextPoint;
	bool atPoint = true;

	TouchState touchState = TouchState.IDLE;
	Vector2 touchStartLocalPos;
	float touchStartAngleOffset;
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
						Vector2 touchPos = cam.ScreenToWorldPoint(touch.position);
						touchStartLocalPos = touchPos - (Vector2)transform.position;
						float touchStartAngle = Vector2.SignedAngle(touchStartLocalPos, Vector2.up);
						touchStartAngleOffset = gun.transform.localEulerAngles.z +
							Vector2.SignedAngle(touchStartLocalPos, Vector2.up);

						if (fireTrigger.OverlapPoint(touchPos)) {
							Debug.Log("FIRE!");
						}
						else if (moveTrigger.OverlapPoint(touchPos)) {
							touchState = TouchState.MOVING;
							fingerId = touch.fingerId;
						}
						else if (turnTrigger.OverlapPoint(touchPos)) {
							touchState = TouchState.TURNING;
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
						else {
							Vector2 localTouchPos = cam.ScreenToWorldPoint(touch.position) - transform.position;

							if (touchState == TouchState.TURNING) {
								TurnTank(localTouchPos);
							}
							else if (touchState == TouchState.MOVING) {
								MoveTank(localTouchPos);
							}
						}
					}
				}
			}
		}
	}

	void TurnTank(Vector2 localTouchPos) {
		float touchAngle = Vector2.SignedAngle(localTouchPos, Vector2.up);
		float targetAngle = touchStartAngleOffset - touchAngle;
		float smoothAngle = Mathf.SmoothDampAngle(gun.transform.localEulerAngles.z, targetAngle,
			ref turnVelocity, smoothTurnTime, maxTurnSpeed);
		smoothAngle = (smoothAngle + 180) % 360 - 180;

		gun.transform.localEulerAngles = new Vector3(0, 0,
			Mathf.Clamp(smoothAngle, -maxAngle, maxAngle));
	}

	void MoveTank(Vector2 localTouchPos) {
		Vector2 deltaPos = localTouchPos - touchStartLocalPos;

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

		Vector2 targetPos = (Vector2)transform.position + nextPointDelta.normalized * moveDist;
		Vector2 smoothPos = Vector2.SmoothDamp((Vector2)transform.position, targetPos,
			ref moveVelocity, smoothMoveTime, maxMoveSpeed);

		foreach (Point point in grid.points.Values) {
			if (Vector2.Distance(smoothPos, point.position) <= snapDistance) {
				smoothPos = new Vector2(point.position.x, point.position.y);
				lastPoint = point;
				atPoint = true;
			}
		}

		transform.position = new Vector3(smoothPos.x, smoothPos.y, transform.position.z);
	}
}
