using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using k4k.wfc;

namespace k4k.map {
    public class CellSlope : CellFloor {

        Vector2Int slopeDirection;
        public Vector2Int SlopeDirection { get => slopeDirection; }

        public CellSlope(Vector2Int coorGrid, int wfc_height, Vector2Int slopeDirection) : base(coorGrid, wfc_height) {
            this.slopeDirection = slopeDirection;
        }


        public override void AssignTerrain(TerrainController terrainController, TerrainController.TerrainConfig terrainConf) {
            // direction.x and direction.y are inverted on purpose, since the unity terrain object uses an inverted system of coordinates
            int invertedX = slopeDirection.y;
            int invertedY = slopeDirection.x;
            /* cellSize = 6
             * [0,0] [1,0] [2,0] [3,0] [4,0] [5,0]
             * [0,1] [1,1] [2,1] [3,1] [4,1] [5,1]
             * [0,2] [1,2] [2,2] [3,2] [4,2] [5,2]
             * [0,3] [1,3] [2,3] [3,3] [4,3] [5,3]
             * [0,4] [1,4] [2,4] [3,4] [4,4] [5,4]   
             * [0,5] [1,5] [2,5] [3,5] [4,5] [5,5] */

            /* A stair step scheme
                        - Step 1 [0,0] [1,0] height 1
                        - Step 2 [2,0] [3,0] height 2 
                        We can infer that a step is always composed of 2 points
                        And that the height of the two points is the same, but changes in the next step.
                        WARNING, this is valid only if the direction is right though */
            var cellSizeOddEven = terrainConf.CellSizeOddEven;
            var cellSize = terrainConf.CellSize;
            var heightStairStep = terrainConf.HeightStairStep;
            var heightRatio = terrainConf.HeightRatio;
            var heightUnitTotal = terrainConf.HeightUnitTotal;
            var heightCell = terrainConf.HeightCell;

            var x = coorGrid.x;
            var y = coorGrid.y;


            terrainController.SetCell(x, y, (x, y) => {
                int stepMultiplierX;
                int stepMultiplierY;
                if (invertedX > 0 || invertedY > 0) {
                    stepMultiplierX = (int)((x + 1 + cellSizeOddEven) / 2) * Mathf.Abs(invertedX);
                    stepMultiplierY = (int)((y + 1 + cellSizeOddEven) / 2) * Mathf.Abs(invertedY);
                } else {
                    stepMultiplierX = (int)(((cellSize - x) + 1 + cellSizeOddEven) / 2) * Mathf.Abs(invertedX);
                    stepMultiplierY = (int)(((cellSize - y) + 1 + cellSizeOddEven) / 2) * Mathf.Abs(invertedY);
                }

                var heightValue =
                    (stepMultiplierX * heightStairStep) +
                    (stepMultiplierY * heightStairStep)
                ;

                float h = Constants.MapUnitToInterval(this.terrainHeight, heightRatio, heightUnitTotal, heightCell);
                //Debug.Log($"[{stepMultiplierX}, {stepMultiplierY}] = {heightValue} + {h} = {heightValue + h}");

                return heightValue + h;
            });        
        }

        /*
        override public void SetCoorReal(Vector3[] squareVertexes) {
            base.SetCoorReal(squareVertexes);
            this.slopeDirection = SquareOrientation(squareVertexes);
        }
        */


        public override string ToString() {
            return $"{base.ToString()}, Slope";
        }

    }
}
