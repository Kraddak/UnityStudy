using k4k.wfc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace k4k.map {
    public class CellObstacle : Cell {
        public CellObstacle(Vector2Int coorGrid, int terrainHeight) : base(coorGrid, terrainHeight) {
            // No additional initialization needed for CellSimple.
        }

        public override void AssignTerrain(TerrainController terrainController, TerrainController.TerrainConfig terrainConf) {

            System.Random prng = new System.Random(coorGrid.x + coorGrid.y);
            float obstacleScaler = 100f; // how pointy is the obstacle, 0 not pointy
            float pointyHeightScaler = 0.1f;

            terrainController.SetCell(coorGrid.x, coorGrid.y, (x, y) => {
                return this.terrainHeight + pointyHeightScaler * Mathf.PerlinNoise(
                    (float)prng.NextDouble() * obstacleScaler,
                    (float)prng.NextDouble() * obstacleScaler) * Mathf.Abs(1 - terrainConf.HeightRatio)
                    + terrainConf.HeightRatio;
            });
        }

        public override bool IsWalkable(ref Placeable placedObject) {
            placedObject = null;
            return false;
        }

        public override string ToString() {
            return $"{base.ToString()}, Obstacle";
        }



    }
}