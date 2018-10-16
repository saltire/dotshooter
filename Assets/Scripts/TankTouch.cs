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
	public Counter shotCounter;

	TouchState touchState = TouchState.IDLE;
	int fingerId;

	public GameObject laserPrefab;
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
	GridBuilder grid;

	void Start() {
		cam = (Camera)FindObjectOfType(typeof(Camera));
		grid = (GridBuilder)FindObjectOfType(typeof(GridBuilder));

		targetMask = LayerMask.GetMask("Targets");
		surfaceMask = LayerMask.GetMask("Surfaces");

		lastPoint = grid.GetPointAtPos(transform.position);

		arrows = new List<Arrow>();
		UpdateArrows();
	}

	void Update() {
		if (laserCooldownRemaining > 0) {
			laserCooldownRemaining -= Time.deltaTime;

			if (laserCooldownRemaining > 0) {
				Material mat = laserBeams[0].GetComponent<SpriteRenderer>().material;
				Color newColor = mat.color;
				newColor.a = laserCooldownRemaining / laserCooldown;
				mat.color = newColor;

				foreach (GameObject laserBeam in laserBeams) {
					laserBeam.GetComponent<SpriteRenderer>().material = mat;
				}
			}
			else {
				foreach (GameObject laserBeam in laserBeams) {
					Destroy(laserBeam);
				}
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
						SetSpriteAlpha(turnCircle, .25f);
					}
					else if (turnTrigger.OverlapPoint(touchPos)) {
						// Start turning.
						touchState = TouchState.TURNING;
						touchStartAngleOffset = tankBottom.localEulerAngles.z +
							Vector2.SignedAngle(localTouchPos, Vector2.up);
						fingerId = touch.fingerId;

						// Dim arrows while turning.
						foreach (Arrow arrow in arrows) {
							SetSpriteAlpha(arrow.GetComponent<SpriteRenderer>(), .25f);
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
						SetSpriteAlpha(turnCircle, 1);
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
		// Get a list of laser beam segments (more than one if the laser bounced off a surface).
		laserBeams = FireLaser(laserSpawnPoint.position, laserSpawnPoint.rotation * Vector3.up);

		foreach (GameObject laserBeam in laserBeams) {
			laserBeam.transform.parent = laserSpawnPoint;

			// Explode all targets this laser beam touches.
			RaycastHit2D[] hits = Physics2D.RaycastAll(laserBeam.transform.position,
				laserBeam.transform.rotation * Vector3.up, laserBeam.transform.localScale.y, targetMask);
			foreach (RaycastHit2D hit in hits) {
				hit.transform.GetComponent<TargetScript>().Explode();
			}
		}

		laserCooldownRemaining = laserCooldown;
		shotCounter.IncrementCount(1);
	}

	List<GameObject> FireLaser(Vector2 origin, Vector2 direction, float totalDistance = 0) {
		GameObject laser = Instantiate(laserPrefab, origin,
			Quaternion.FromToRotation(Vector2.up, direction));

		List<GameObject> lasers;

		RaycastHit2D hit = Physics2D.Raycast(origin, direction, Mathf.Infinity, surfaceMask);
		if (hit.collider == null || totalDistance > maxLaserDistance) {
			// No hits - laser will continue to infinity (or max distance reached).
			// This is the final laser segment, so return a new list.
			lasers = new List<GameObject>();
		}
		else {
			// Hit a surface - scale laser to match the distance to the surface, and get list of bounces.
			laser.transform.localScale = new Vector3(1, hit.distance, 1);
			Vector2 bounceDir = Vector2.Reflect(direction, hit.normal);
			// Move the reflection point a bit to make sure we are outside of the collider.
			lasers = FireLaser(hit.point + bounceDir * .01f, bounceDir, totalDistance + hit.distance);

			// Fire off some particles from the bounce point.
			Instantiate(laserBouncePrefab, hit.point, Quaternion.FromToRotation(Vector2.up, hit.normal));
		}

		lasers.Add(laser);
		return lasers;
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
					SetSpriteAlpha(arrow.GetComponent<SpriteRenderer>(), .25f);
				}
			}
		}
	}

	void SetSpriteAlpha(SpriteRenderer spriter, float alpha) {
		Color newColor = spriter.color;
		newColor.a = alpha;
		spriter.color = newColor;
	}
}
