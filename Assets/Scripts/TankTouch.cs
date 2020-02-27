using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

enum TouchState {
	IDLE,
	FIRING,
	TURNING,
	MOVING,
};

public class TankTouch : MonoBehaviour {
	public TankCannon cannon;
	public TankMovement movement;

  public Collider2D fireTrigger;
	public Collider2D moveTrigger;
	public Collider2D turnTrigger;

	public float moveThreshold = .2f;

	Camera cam;

	TouchState touchState = TouchState.IDLE;
	int fingerId;
	Vector2 localStartPos;

	void Awake() {
		cam = (Camera)FindObjectOfType(typeof(Camera));
  }

  void Update() {
		if (Input.touchCount > 0) {
			foreach (Touch touch in Input.touches) {
				Vector2 touchPos = cam.ScreenToWorldPoint(touch.position);
				Vector2 localTouchPos = touchPos - (Vector2)transform.position;

				if (touchState == TouchState.IDLE && touch.phase == TouchPhase.Began) {
					// Touch starting

          fingerId = touch.fingerId;
					localStartPos = localTouchPos;

          Arrow touchingArrow = movement.arrows
            .FirstOrDefault(arrow => arrow.GetComponent<Collider2D>().OverlapPoint(touchPos));

					if (fireTrigger.OverlapPoint(touchPos)) {
						touchState = TouchState.FIRING;
					}
					else if (turnTrigger.OverlapPoint(touchPos)) {
						touchState = TouchState.TURNING;
            cannon.StartTurn(localTouchPos);
					}
					else if (touchingArrow != null || moveTrigger.OverlapPoint(touchPos)) {
						touchState = TouchState.MOVING;
            movement.StartMove(localTouchPos, touchingArrow);
					}
				}
			  else if (touchState != TouchState.IDLE && touch.fingerId == fingerId) {
					if (touch.phase != TouchPhase.Ended) {
						// Touch continuing

						if (touchState == TouchState.FIRING) {
							// If the player hits fire then moves their finger a certain amount,
							// start treating the touch as a move instead of a fire.
							if (Vector2.Distance(localTouchPos, localStartPos) >= moveThreshold) {
								touchState = TouchState.MOVING;
								movement.ContinueMove(localTouchPos, localStartPos);
							}
						}
						else if (touchState == TouchState.TURNING) {
							cannon.ContinueTurn(localTouchPos);
						}
						else if (touchState == TouchState.MOVING) {
							movement.ContinueMove(localTouchPos, localStartPos);
						}
					}
					else {
						// Touch ending

						if (touchState == TouchState.FIRING) {
							cannon.Fire();
						}
						else if (touchState == TouchState.TURNING) {
							cannon.EndTurn();
						}
						else if (touchState == TouchState.MOVING) {
							movement.EndMove();
						}

						touchState = TouchState.IDLE;
						fingerId = -1;
					}
				}
			}
		}
  }
}
