using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DotSpawner))]
public class DotSpawnerEditor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		DotSpawner spawner = (DotSpawner)target;

		if (GUILayout.Button("Spawn Dots")) {
			spawner.SpawnDots();
		}

		if (GUILayout.Button("Remove Dots")) {
			spawner.RemoveDots();
		}
	}
}
