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

  public TargetScript[][] targetGrid { get; private set; }

  void Awake() {
    GetComponent<TilemapRenderer>().enabled = false;
    Tilemap tilemap = GetComponent<Tilemap>();
    List<TargetScript> targets = new List<TargetScript>();

    Dictionary<string, TargetScript> targetPrefabs =
      FindObjectOfType<LevelManager>().targetPrefabs;

    int xSize = tilemap.cellBounds.xMax - tilemap.cellBounds.xMin + 1;
    int ySize = tilemap.cellBounds.yMax - tilemap.cellBounds.yMin + 1;
    targetGrid = new TargetScript[xSize][];

    for (int x = 0; x < xSize; x++) {
      targetGrid[x] = new TargetScript[ySize];

      for (int y = 0; y < ySize; y++) {
        Vector3Int tilePos = new Vector3Int(
          x + tilemap.cellBounds.xMin, y + tilemap.cellBounds.yMin, 0);
        TileBase tile = tilemap.GetTile(tilePos);
        if (tile != null && targetPrefabs.ContainsKey(tile.name)) {
          TargetScript target = Instantiate<TargetScript>(targetPrefabs[tile.name],
            tilemap.GetCellCenterWorld(tilePos), Quaternion.identity);
          target.transform.parent = transform;
          targetGrid[x][y] = target;
          target.SetPos(x, y);
          target.SetColor(targetColors[Random.Range(0, targetColors.Length)]);
          targets.Add(target);
        }
      }
    }

    FindObjectOfType<LevelManager>().SetTargets(targets);
  }
}
