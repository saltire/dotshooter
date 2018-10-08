using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TargetSpawner))]
public class TargetSpawnerEditor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		TargetSpawner spawner = (TargetSpawner)target;

		if (GUILayout.Button("Spawn Targets")) {
			spawner.SpawnTargets();
		}

		if (GUILayout.Button("Remove Targets")) {
			spawner.RemoveTargets();
		}
	}
}
