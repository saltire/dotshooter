using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Counter : MonoBehaviour {
	public string label = "";
	public Color countColor = Color.red;
	public int count = 0;

	string hexColor;
	Text text;

	void Awake() {
		hexColor = string.Format("#{0:x2}{1:x2}{2:x2}ff",
			(int)(255 * countColor.r), (int)(255 * countColor.g), (int)(255 * countColor.b));

		text = GetComponent<Text>();

		UpdateText();
	}

	public void SetCount(int newCount) {
		count = newCount;
		UpdateText();
	}

	public void IncrementCount(int increment) {
		count += increment;
		UpdateText();
	}

	void UpdateText() {
		text.text = label + " <color=" + hexColor + ">" + count + "</color>";
	}
}
