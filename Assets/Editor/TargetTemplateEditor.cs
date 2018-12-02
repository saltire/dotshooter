using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TargetTemplate))]
public class TargetTemplateEditor : Editor {
	int rows = 5;
	int cols = 7;
	float spacing = 0.8f;

	public override void OnInspectorGUI() {
		TargetTemplate template = (TargetTemplate)target;

		DrawDefaultInspector();
		EditorGUILayout.Space();

		rows = EditorGUILayout.IntField("Rows", rows);
		cols = EditorGUILayout.IntField("Columns", cols);
		spacing = EditorGUILayout.FloatField("Spacing", spacing);

		if (GUILayout.Button("Spawn Targets")) {
			template.SpawnPlaceholdersOnGrid(rows, cols, spacing);
		}

		if (GUILayout.Button("Remove Targets")) {
			template.RemovePlaceholders();
		}
	}
}
