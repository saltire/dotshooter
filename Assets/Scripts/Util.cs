using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util {
  public static void Log(params object[] items) {
    string str = "";
    foreach (object item in items) {
      str += item;
      str += " ";
    }
    Debug.Log(str);
  }

	public static void SetSpriteAlpha(SpriteRenderer spriter, float alpha) {
		Color newColor = spriter.color;
		newColor.a = alpha;
		spriter.color = newColor;
	}

	public static void SetMaterialAlpha(Material mat, float alpha) {
		Color newColor = mat.color;
		newColor.a = alpha;
		mat.color = newColor;
	}
}
