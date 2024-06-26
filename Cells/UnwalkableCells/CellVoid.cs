using k4k.wfc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace k4k.map {
    public class CellVoid : Cell {
        /*
        public CellVoid(Vector2Int coorGrid, Vector3 coorReal) : base(coorGrid, coorReal) {
            // No additional initialization needed for CellSimple.
        }
        */
        public CellVoid(Vector2Int coorGrid, int height) : base(coorGrid, height) {
            // No additional initialization needed for CellSimple.
        }

        public override void AssignTerrain(TerrainController terrainController, TerrainController.TerrainConfig terrainConf) {
            terrainController.SetCell(coorGrid, 0f);
        }

        public override bool IsWalkable(ref Placeable placedObject) {
            placedObject = null;
            return false;
        }


        public override string ToString() {
            return $"{base.ToString()}, Void";
        }



    }
}