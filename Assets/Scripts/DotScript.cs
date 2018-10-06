using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotScript : MonoBehaviour {
	public void SetColor(Color color) {
		GetComponent<SpriteRenderer>().color = color;
	}
}
