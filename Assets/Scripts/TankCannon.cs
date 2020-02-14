using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TankCannon : MonoBehaviour {
  public TankMovement movement;

	float touchStartAngleOffset;
	float turnVelocity = 0;
	public float smoothTurnTime = .1f;
	public float maxTurnSpeed = 500;
	public float maxAngle = 90;

	float laserCooldownRemaining = 0;
	public float laserCooldown = .5f;
	public float maxLaserDistance = 100;
	public LineRenderer laserBeam;
	public GameObject laserBouncePrefab;
	int targetMask;
	int surfaceMask;

	public LineRenderer marchingAnts;

	LevelManager level;
	UIManager ui;

  void Awake() {
		targetMask = LayerMask.GetMask("Targets");
		surfaceMask = LayerMask.GetMask("Surfaces");

		level = (LevelManager)FindObjectOfType(typeof(LevelManager));
		ui = (UIManager)FindObjectOfType(typeof(UIManager));
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
  }

	public void StartTurn(Vector2 localTouchPos) {
		touchStartAngleOffset = transform.localEulerAngles.z +
			Vector2.SignedAngle(localTouchPos, Vector2.up);

		// Dim arrows while turning.
		foreach (Arrow arrow in movement.arrows) {
			Util.SetSpriteAlpha(arrow.GetComponent<SpriteRenderer>(), .25f);
		}
	}

	public void ContinueTurn(Vector2 localTouchPos) {
		float touchAngle = Vector2.SignedAngle(localTouchPos, Vector2.up);
		float targetAngle = touchStartAngleOffset - touchAngle;
		float smoothAngle = Mathf.SmoothDampAngle(transform.localEulerAngles.z, targetAngle,
			ref turnVelocity, smoothTurnTime, maxTurnSpeed);
		smoothAngle = (smoothAngle + 180) % 360 - 180;

		transform.localEulerAngles = new Vector3(0, 0,
			Mathf.Clamp(smoothAngle, -maxAngle, maxAngle));

		UpdateMarchingAnts();
	}

  public void EndTurn() {
    movement.UpdateArrows();
  }

  public void Fire() {
    if (laserCooldownRemaining > 0) {
      return;
    }

		marchingAnts.gameObject.SetActive(false);
		laserBeam.gameObject.SetActive(true);

		Vector3[] positions = GetLaserPositions(true);
		laserBeam.positionCount = positions.Length;
		laserBeam.SetPositions(positions);

		Util.SetMaterialAlpha(laserBeam.material, 1);

		laserCooldownRemaining = laserCooldown;
		ui.shotCounter.IncrementCount(1);
  }

	public void UpdateMarchingAnts() {
		if (marchingAnts.gameObject.activeSelf) {
			Vector3[] positions = GetLaserPositions(false);
			marchingAnts.positionCount = positions.Length;
			marchingAnts.SetPositions(positions);
		}
	}

	Vector3[] GetLaserPositions(bool firing) {
		Vector3 origin = laserBeam.transform.position;
		Vector2 direction = laserBeam.transform.rotation * Vector3.up;
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
				nextPosition = origin + (Vector3)direction * segmentDistance;
			}

			if (firing) {
				// Destroy all targets this laser segment touches.
				RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, segmentDistance, targetMask);
				foreach (RaycastHit2D targetHit in hits) {
					level.DestroyTarget(targetHit.transform.GetComponent<TargetScript>());
				}
			}

			positions.Add(nextPosition);

			origin = nextPosition;
			direction = nextDirection;
			totalDistance += segmentDistance;
		}
		while (totalDistance < maxLaserDistance);

		return positions
			.Select(p => new Vector3(p.x, p.y, laserBeam.transform.position.z))
			.ToArray();
	}
}
