using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetScript : MonoBehaviour {
	public Counter counter { get; set; }

	public void SetColor(Color color) {
		GetComponent<SpriteRenderer>().color = color;

		ParticleSystem.MainModule main = GetComponent<ParticleSystem>().main;
		main.startColor = color;
	}

	public void Explode() {
		SpriteRenderer spriter = GetComponent<SpriteRenderer>();
		if (spriter.enabled) {
			spriter.enabled = false;
			GetComponent<Collider2D>().enabled = false;

			GetComponent<ParticleSystem>().Play();

			counter.IncrementCount(-1);
		}
	}
}
