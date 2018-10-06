using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankTouch : MonoBehaviour {
	public float fireButtonRadius = 0.3f;

	public Vector2 turnHandleOffset;
	public float turnHandleRadius = 0.3f;

	public Vector2 moveHandleOffset;
	public float moveHandleRadius = 0.3f;

	Camera cam;

	void Start() {
		cam = (Camera)FindObjectOfType(typeof(Camera));
	}

	void Update() {
		if (Input.touchCount > 0) {
			foreach (Touch touch in Input.touches) {
				if (touch.phase == TouchPhase.Began) {
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
					}
					else if (moveHandleDist < moveHandleRadius) {
						Debug.Log("Moving");
					}
				}
			}
		}
	}
}
