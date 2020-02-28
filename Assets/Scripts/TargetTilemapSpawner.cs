using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TargetTilemapSpawner : MonoBehaviour {
  public TargetScript targetPrefab;
	public Color[] targetColors = {
		Color.red,
		Color.yellow,
		Color.green,
		Color.blue,
	};

  void Awake() {
    GetComponent<TilemapRenderer>().enabled = false;
    Tilemap tilemap = GetComponent<Tilemap>();
    List<GameObject> targets = new List<GameObject>();

    for (int x = tilemap.cellBounds.xMin; x <= tilemap.cellBounds.xMax; x++) {
      for (int y = tilemap.cellBounds.yMin; y <= tilemap.cellBounds.yMax; y++) {
        Vector3Int tilePos = new Vector3Int(x, y, 0);
        TileBase tile = tilemap.GetTile(tilePos);
        if (tile != null) {
          TargetScript target = Instantiate<TargetScript>(targetPrefab,
            tilemap.GetCellCenterWorld(tilePos), Quaternion.identity);
          target.transform.parent = transform;
          target.SetColor(targetColors[Random.Range(0, targetColors.Length)]);
          targets.Add(target.gameObject);
        }
      }
    }

    FindObjectOfType<LevelManager>().SetTargets(targets);
  }
}
