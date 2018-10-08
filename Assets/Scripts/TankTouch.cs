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

	TouchState touchState = TouchState.IDLE;
	int fingerId;

	public GameObject laserPrefab;
	public Transform laserSpawnPoint;
	public float laserCooldown = .5f;
	GameObject laserBeam;
	float laserCooldownRemaining = 0;
	int targetMask;

	public float smoothTurnTime = .1f;
	public float maxTurnSpeed = 500;
	public float maxAngle = 90;
	float touchStartAngleOffset;
	float turnVelocity = 0;

	public float smoothMoveTime = .015f;
	public float maxMoveSpeed = 50;
	public float snapDistance = 0.15f;
	Vector2 touchStartLocalPos;
	Point lastPoint;
	Point nextPoint;
	bool atPoint = true;
	Vector2 moveVelocity = Vector2.zero;

	Camera cam;
	GridSpawner grid;

	void Start() {
		cam = (Camera)FindObjectOfType(typeof(Camera));
		grid = (GridSpawner)FindObjectOfType(typeof(GridSpawner));

		targetMask = LayerMask.GetMask("Targets");

		lastPoint = grid.GetPointAtPos(transform.position);
	}

	void Update() {
		if (laserCooldownRemaining > 0) {
			laserCooldownRemaining -= Time.deltaTime;

			if (laserCooldownRemaining > 0) {
				Material mat = laserBeam.GetComponent<SpriteRenderer>().material;
				Color newColor = mat.color;
				newColor.a = laserCooldownRemaining / laserCooldown;
				mat.color = newColor;
			}
			else {
				Destroy(laserBeam);
			}
		}

		if (Input.touchCount > 0) {
			foreach (Touch touch in Input.touches) {
				Vector2 touchPos = cam.ScreenToWorldPoint(touch.position);
				Vector2 localTouchPos = touchPos - (Vector2)transform.position;

				if (touchState == TouchState.IDLE && touch.phase == TouchPhase.Began) {
					if (fireTrigger.OverlapPoint(touchPos) && laserCooldownRemaining <= 0) {
						Fire();
					}
					else if (moveTrigger.OverlapPoint(touchPos)) {
						touchState = TouchState.MOVING;
						touchStartLocalPos = localTouchPos;
						fingerId = touch.fingerId;
					}
					else if (turnTrigger.OverlapPoint(touchPos)) {
						touchState = TouchState.TURNING;
						touchStartAngleOffset = tankBottom.localEulerAngles.z +
							Vector2.SignedAngle(localTouchPos, Vector2.up);
						fingerId = touch.fingerId;
					}
				}
			  else if (touchState != TouchState.IDLE && touch.fingerId == fingerId) {
					if (touch.phase == TouchPhase.Ended) {
						touchState = TouchState.IDLE;
						fingerId = -1;
					}
					else {
						if (touchState == TouchState.TURNING && turnTrigger.OverlapPoint(touchPos)) {
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

	void Fire() {
		laserBeam = Instantiate<GameObject>(laserPrefab, laserSpawnPoint.position,
			laserSpawnPoint.rotation);
		laserBeam.transform.parent = laserSpawnPoint;
		laserCooldownRemaining = laserCooldown;

		RaycastHit2D[] hits = Physics2D.RaycastAll(laserSpawnPoint.position,
			laserSpawnPoint.rotation * Vector3.up, Mathf.Infinity, targetMask);

		foreach (RaycastHit2D hit in hits) {
			hit.transform.GetComponent<TargetScript>().Explode();
		}
	}

	void TurnTank(Vector2 localTouchPos) {
		float touchAngle = Vector2.SignedAngle(localTouchPos, Vector2.up);
		float targetAngle = touchStartAngleOffset - touchAngle;
		float smoothAngle = Mathf.SmoothDampAngle(tankBottom.localEulerAngles.z, targetAngle,
			ref turnVelocity, smoothTurnTime, maxTurnSpeed);
		smoothAngle = (smoothAngle + 180) % 360 - 180;

		tankBottom.localEulerAngles = new Vector3(0, 0,
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
