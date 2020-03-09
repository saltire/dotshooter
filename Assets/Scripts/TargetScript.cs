using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetScript : MonoBehaviour {
	protected LevelManager level;

	protected int x;
	protected int y;

	void Awake() {
		level = FindObjectOfType<LevelManager>();
	}

	public void SetPos(int px, int py) {
		x = px;
		y = py;
	}

	public void SetColor(Color color) {
		GetComponent<SpriteRenderer>().color = color;

		ParticleSystem.MainModule main = GetComponent<ParticleSystem>().main;
		main.startColor = color;
	}

	public virtual void Explode() {
		SpriteRenderer spriter = GetComponent<SpriteRenderer>();
		if (spriter.enabled) {
			spriter.enabled = false;
			GetComponent<Collider2D>().enabled = false;

			GetComponent<ParticleSystem>().Play();
		}
	}
}
