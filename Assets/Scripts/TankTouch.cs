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
	public SpriteRenderer turnCircle;
	public Collider2D fireTrigger;
	public Collider2D turnTrigger;
	public GameObject arrowPrefab;

	TouchState touchState = TouchState.IDLE;
	int fingerId;

	public LineRenderer marchingAnts;
	public LineRenderer laserBeam;
	public GameObject laserBouncePrefab;
	public Transform laserSpawnPoint;
	public float laserCooldown = .5f;
	public float maxLaserDistance = 100;
	List<GameObject> laserBeams;
	float laserCooldownRemaining = 0;
	int targetMask;
	int surfaceMask;

	public float smoothTurnTime = .1f;
	public float maxTurnSpeed = 500;
	public float maxAngle = 90;
	float touchStartAngleOffset;
	float turnVelocity = 0;

	public float smoothMoveTime = .015f;
	public float maxMoveSpeed = 50;
	public float snapDistance = 0.15f;
	public float arrowDistance = 1.25f;
	Vector2 touchStartLocalPos;
	Point lastPoint;
	Point nextPoint;
	Vector2 currentDirection;
	bool atLastPoint = true;
	Vector2 moveVelocity = Vector2.zero;
	List<Arrow> arrows;

	Camera cam;
	PathBuilder paths;
	TargetSpawner targets;
	UIManager ui;

	void Awake() {
		cam = (Camera)FindObjectOfType(typeof(Camera));
		paths = (PathBuilder)FindObjectOfType(typeof(PathBuilder));
		targets = (TargetSpawner)FindObjectOfType(typeof(TargetSpawner));
		ui = (UIManager)FindObjectOfType(typeof(UIManager));

		targetMask = LayerMask.GetMask("Targets");
		surfaceMask = LayerMask.GetMask("Surfaces");

		arrows = new List<Arrow>();
	}

	void Update() {
		if (laserCooldownRemaining > 0) {
			laserCooldownRemaining -= Time.deltaTime;

			if (laserCooldownRemaining > 0) {
				Util.SetMaterialAlpha(laserBeam.material, laserCooldownRemaining / laserCooldown);
			}
			else {
				laserBeam.gameObject.SetActive(false);
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
						// Start moving.
						touchState = TouchState.MOVING;
						touchStartLocalPos = localTouchPos;
						fingerId = touch.fingerId;
						currentDirection = touchingArrow.point.position - transform.position;

						// Dim turning circle while moving.
						Util.SetSpriteAlpha(turnCircle, .25f);
					}
					else if (turnTrigger.OverlapPoint(touchPos)) {
						// Start turning.
						touchState = TouchState.TURNING;
						touchStartAngleOffset = tankBottom.localEulerAngles.z +
							Vector2.SignedAngle(localTouchPos, Vector2.up);
						fingerId = touch.fingerId;

						// Dim arrows while turning.
						foreach (Arrow arrow in arrows) {
							Util.SetSpriteAlpha(arrow.GetComponent<SpriteRenderer>(), .25f);
						}
					}
				}
			  else if (touchState != TouchState.IDLE && touch.fingerId == fingerId) {
					if (touch.phase == TouchPhase.Ended) {
						touchState = TouchState.IDLE;
						fingerId = -1;

						// Reset movement and UI.
						currentDirection = Vector2.zero;
						UpdateArrows();
						Util.SetSpriteAlpha(turnCircle, 1);
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
		marchingAnts.gameObject.SetActive(false);
		laserBeam.gameObject.SetActive(true);

		Vector3[] positions = GetLaserPositions(true);
		laserBeam.positionCount = positions.Length;
		laserBeam.SetPositions(positions);
		
		Util.SetMaterialAlpha(laserBeam.material, 1);

		laserCooldownRemaining = laserCooldown;
		ui.shotCounter.IncrementCount(1);
	}

	Vector3[] GetLaserPositions(bool firing) {
		Vector2 origin = laserSpawnPoint.position;
		Vector2 direction = laserSpawnPoint.rotation * Vector3.up;
		float totalDistance = 0;

		List<Vector2> positions = new List<Vector2>();
		positions.Add(origin);

		do {
			RaycastHit2D hit = Physics2D.Raycast(origin, direction, Mathf.Infinity, surfaceMask);
			Vector2 nextPosition;
			Vector2 nextDirection = Vector2.zero;
			float segmentDistance;

			if (hit.collider != null) {
				// Hit a collider: store the hit position and bounce the laser.
				// Move the reflection point back a bit to make sure we are outside of the collider.
				nextPosition = hit.point - direction * .01f;
				nextDirection = Vector2.Reflect(direction, hit.normal);
				segmentDistance = hit.distance;

				if (firing) {
					// Fire off some particles from the bounce point.
					Instantiate(laserBouncePrefab, hit.point, Quaternion.FromToRotation(Vector2.up, hit.normal));
				}
			}
			else {
				// No collision: extend laser to max distance.
				segmentDistance = maxLaserDistance - totalDistance;
				nextPosition = origin + direction * segmentDistance;
			}

			if (firing) {
				// Destroy all targets this laser segment touches.
				RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, segmentDistance, targetMask);
				foreach (RaycastHit2D targetHit in hits) {
					targets.DestroyTarget(targetHit.transform.GetComponent<TargetScript>());
				}
			}
			
			positions.Add(nextPosition);

			origin = nextPosition;
			direction = nextDirection;
			totalDistance += segmentDistance;
		}
		while (totalDistance < maxLaserDistance);

		return positions
			.Select(p => new Vector3(p.x, p.y, laserSpawnPoint.position.z))
			.ToArray();
	}

	void TurnTank(Vector2 localTouchPos) {
		float touchAngle = Vector2.SignedAngle(localTouchPos, Vector2.up);
		float targetAngle = touchStartAngleOffset - touchAngle;
		float smoothAngle = Mathf.SmoothDampAngle(tankBottom.localEulerAngles.z, targetAngle,
			ref turnVelocity, smoothTurnTime, maxTurnSpeed);
		smoothAngle = (smoothAngle + 180) % 360 - 180;

		tankBottom.localEulerAngles = new Vector3(0, 0,
			Mathf.Clamp(smoothAngle, -maxAngle, maxAngle));
			
		UpdateMarchingAnts();
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
			foreach (Point point in paths.GetAllPoints()) {
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
		UpdateMarchingAnts();
	}

	public void MoveToPoint(Point point) {
		lastPoint = point;
		atLastPoint = true;

		transform.position = new Vector3(point.position.x, point.position.y, transform.position.z);
		tankBottom.localEulerAngles = Vector3.zero;

		UpdateArrows();
		UpdateMarchingAnts();
	}

	void ClearArrows() {
		foreach (Arrow arrow in arrows) {
			Destroy(arrow.gameObject);
		}
		arrows = new List<Arrow>();
	}

	void UpdateArrows() {
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

	void UpdateMarchingAnts() {
		if (marchingAnts.gameObject.activeSelf) {
			Vector3[] positions = GetLaserPositions(false);
			marchingAnts.positionCount = positions.Length;
			marchingAnts.SetPositions(positions);
		}
	}
}
