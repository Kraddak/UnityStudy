using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using k4k.wfc;

namespace k4k.map {
    public class CellFloor : Cell {

        Placeable placedObject;
        /*
        public CellSimple(Vector2Int coorGrid, Vector3 coorReal) : base(coorGrid, coorReal) {
            // No additional initialization needed for CellSimple.
        }
        */
        public CellFloor(Vector2Int coorGrid, int terrainHeight) : base(coorGrid, terrainHeight) {
            // No additional initialization needed for CellSimple.
        }
        public override bool IsWalkable(ref Placeable placedObject) {
            placedObject = this.placedObject;
            return this.placedObject == null ? true : this.placedObject.IsWalkable();
        }

        /*
        public override float TerrainHeightLambda(int x, int y) {
            return this.terrainHeight;
        }*/

        public override string ToString() {
            return $"{base.ToString()}, Floor";
        }

        public override void AssignTerrain(TerrainController terrainController, TerrainController.TerrainConfig terrainConf) {
            terrainController.SetCell(coorGrid, this.terrainHeight);
        }
    }
}