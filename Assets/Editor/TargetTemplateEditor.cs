using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TargetTemplate))]
public class TargetTemplateEditor : Editor {
	Vector2 origin = new Vector2(0, 4.5f);

	int rows = 5;
	int cols = 7;
	float spacing = 0.8f;

	int hexRadius = 4;
	float hexSpacing = 0.8f;

	public override void OnInspectorGUI() {
		TargetTemplate template = (TargetTemplate)target;

		DrawDefaultInspector();

		EditorGUILayout.Space();

		origin = EditorGUILayout.Vector2Field("Origin Point", origin);
		
		EditorGUILayout.Space();

		rows = EditorGUILayout.IntField("Rows", rows);
		cols = EditorGUILayout.IntField("Columns", cols);
		spacing = EditorGUILayout.FloatField("Spacing", spacing);

		if (GUILayout.Button("Spawn Targets on Grid")) {
			template.SpawnPlaceholdersOnGrid(origin, rows, cols, spacing);
		}

		EditorGUILayout.Space();

		hexRadius = EditorGUILayout.IntField("Hex Radius", hexRadius);
		hexSpacing = EditorGUILayout.FloatField("Spacing", hexSpacing);

		if (GUILayout.Button("Spawn Targets on Hex")) {
			template.SpawnPlaceholdersOnHex(origin, hexRadius, hexSpacing);
		}
		
		EditorGUILayout.Space();

		if (GUILayout.Button("Remove Targets")) {
			template.RemovePlaceholders();
		}
	}
}
