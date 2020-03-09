using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombTargetScript : TargetScript {
  public override void Explode() {
    base.Explode();

    TargetScript[][] targetGrid = GetComponentInParent<TargetTilemapSpawner>().targetGrid;
    ExplodeNeighbor(targetGrid[x - 1][y]);
    ExplodeNeighbor(targetGrid[x + 1][y]);
    ExplodeNeighbor(targetGrid[x][y - 1]);
    ExplodeNeighbor(targetGrid[x][y + 1]);
  }

  void ExplodeNeighbor(TargetScript neighbor) {
    if (neighbor != null) {
      level.DestroyTarget(neighbor);
    }
  }
}
