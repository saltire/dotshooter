using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchScript : MonoBehaviour {
	void Start() {

	}

	void Update() {
		if (Input.touchCount > 0) {
			string str = "";
			foreach (Touch touch in Input.touches) {
				str += "ID: " + touch.fingerId + " Phase: " + touch.phase + " Taps: " + touch.tapCount + " X: " + touch.position.x + " Y: " + touch.position.y;
			}
			Debug.Log(str);
		}
	}
}
