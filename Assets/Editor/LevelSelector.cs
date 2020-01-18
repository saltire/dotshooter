using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelManager))]
public class LevelSelector : Editor {
  LevelManager levelManager;

	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		LevelManager levelManager = (LevelManager)target;

		if (GUILayout.Button("Build Level")) {
      GenericMenu menu = new GenericMenu();

      foreach (GameObject levelPrefab in levelManager.levelPrefabs) {
        menu.AddItem(new GUIContent(levelPrefab.name), false,
          () => levelManager.LoadLevel(levelPrefab));
      }

      menu.ShowAsContext();
		}

    if (GUILayout.Button("Clear Level")) {
      levelManager.ClearLevel();
    }
	}
}
