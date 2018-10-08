using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetScript : MonoBehaviour {
	public void SetColor(Color color) {
		GetComponent<SpriteRenderer>().color = color;

		ParticleSystem.MainModule main = GetComponent<ParticleSystem>().main;
		main.startColor = color;
	}

	public void Explode() {
		GetComponent<SpriteRenderer>().enabled = false;
		GetComponent<Collider2D>().enabled = false;

		GetComponent<ParticleSystem>().Play();
	}
}
