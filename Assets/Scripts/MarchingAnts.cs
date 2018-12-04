using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingAnts : MonoBehaviour {
	public float speed = .5f;

	Material mat;

	void Awake() {
		LineRenderer line = GetComponent<LineRenderer>();
		mat = line.material;
	}

	void Update() {
		mat.mainTextureOffset = new Vector2(-(Time.time % speed) / speed, 0);
	}
}
