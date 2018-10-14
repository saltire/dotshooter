using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
	public GameObject arrowPrefab;
	public Counter shotCounter;

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
	Vector2 currentDirection;
	bool atLastPoint = true;
	Vector2 moveVelocity = Vector2.zero;
	List<Arrow> arrows;

	Camera cam;
	GridBuilder grid;

	void Start() {
		cam = (Camera)FindObjectOfType(typeof(Camera));
		grid = (GridBuilder)FindObjectOfType(typeof(GridBuilder));

		targetMask = LayerMask.GetMask("Targets");

		lastPoint = grid.GetPointAtPos(transform.position);

		arrows = new List<Arrow>();
		UpdateArrows();
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

				Arrow touchingArrow = arrows
					.FirstOrDefault(arrow => arrow.GetComponent<Collider2D>().OverlapPoint(touchPos));

				if (touchState == TouchState.IDLE && touch.phase == TouchPhase.Began) {
					if (fireTrigger.OverlapPoint(touchPos) && laserCooldownRemaining <= 0) {
						Fire();
					}
					else if (touchingArrow != null) {
						touchState = TouchState.MOVING;
						touchStartLocalPos = localTouchPos;
						fingerId = touch.fingerId;
						currentDirection = touchingArrow.point.position - transform.position;
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

						// Reset movement.
						currentDirection = Vector2.zero;
						UpdateArrows();
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

		shotCounter.IncrementCount(1);
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
		Vector2 touchDirection = localTouchPos - touchStartLocalPos;

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
			foreach (Point point in grid.GetAllPoints()) {
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
	}

	void UpdateArrows() {
		// Clear all arrows.
		foreach (Arrow arrow in arrows) {
			Destroy(arrow.gameObject);
		}
		arrows = new List<Arrow>();

		// If at a point, show arrows for all connected points.
		// Otherwise, show arrows to the two adjacent points.
		List<Point> arrowPoints = atLastPoint ? lastPoint.connections :
			new List<Point>() { lastPoint, nextPoint };

		foreach (Point arrowPoint in arrowPoints) {
			// Create an arrow pointing to this point.
			float pointAngle = Vector2.SignedAngle(Vector2.up, arrowPoint.position - transform.position);
			GameObject arrow = Instantiate(arrowPrefab, transform.position,
				Quaternion.Euler(0, 0, pointAngle));
			arrow.transform.parent = transform;
			Arrow arrowScript = arrow.GetComponent<Arrow>();
			arrowScript.point = arrowPoint;
			arrows.Add(arrowScript);

			// If at a point and moving, fade out arrows that aren't aligned with current direction.
			if (atLastPoint && currentDirection != Vector2.zero) {
				Vector2 pointDirection = arrowPoint.position - transform.position;
				float dirDiff = Vector2.Angle(currentDirection, pointDirection);
				if (dirDiff != 0 && dirDiff != 180) {
					arrow.GetComponent<SpriteRenderer>().color -= new Color(0, 0, 0, .75f);
				}
			}
		}
	}
}
