using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using k4k.wfc;
using System;

namespace k4k.map {
    public abstract class Cell {

        protected Vector2Int coorGrid;
        protected int terrainHeight;
        protected Vector3 coorReal;

        public Vector2Int CoorGrid { get => coorGrid; }
        public Vector3 CoorReal { get => coorReal; }
        public int TerrainHeight { get => terrainHeight; }

        protected Cell(Vector2Int coorGrid, int heightGrid) {
            this.coorGrid = coorGrid;
            this.terrainHeight = heightGrid;
        }

        public void SetCoorWorld(Vector3 newCoorReal) {
            coorReal = newCoorReal;
        }
        public void SetCoorWorld(Vector3[] terrainCellSquare) {
            coorReal = CalculateSquareCenter(terrainCellSquare);
        }


        /*****************************************************************************
         ************************* ABSTRACT ******************************************
         *****************************************************************************/
        public abstract bool IsWalkable(ref Placeable placedObject);

        //public abstract float TerrainHeightLambda(int x, int y);
        // public abstract ??? UnitFall( ??? );
        public abstract void AssignTerrain(TerrainController terrainController, TerrainController.TerrainConfig terrainConf);


        /*****************************************************************************
         ************************* STATIC ********************************************
         *****************************************************************************/

        public static Vector3 CalculateSquareCenter(Vector3[] squareVertexes) {
            if (squareVertexes.Length < 4) {
                Debug.LogError("Insufficient number of vertexes for a square.");
                return Vector3.zero; // or some default value indicating an error
            }

            // Calculate the average of x, y, and z coordinates
            float averageX = 0;
            float averageY = 0;
            float averageZ = 0;

            for (int i = 0; i < squareVertexes.Length; i++) {
                averageX += squareVertexes[i].x;
                averageY += squareVertexes[i].y;
                averageZ += squareVertexes[i].z;
            }

            averageX /= squareVertexes.Length;
            averageY /= squareVertexes.Length;
            averageZ /= squareVertexes.Length;

            return new Vector3(averageX, averageY, averageZ);
        }

        public static Vector3 SquareOrientation(Vector3[] squareVertexes) {
            if (squareVertexes.Length < 4) {
                // This really shouldn't happen
                Debug.LogError("Insufficient number of vertexes for a square.");
                return Vector3.up;
            }

            float marginOfError = 0.0001f; // Adjust this based on your needs

            float baseY = squareVertexes[0].y;

            for (int i = 1; i < squareVertexes.Length; i++) {
                if (Mathf.Abs(squareVertexes[i].y - baseY) > marginOfError) {
                    // The square is not parallel to the x-z plane."

                    // Calculate the normal vector of the square's plane
                    Vector3 side1 = squareVertexes[1] - squareVertexes[0];
                    Vector3 side2 = squareVertexes[3] - squareVertexes[0];
                    // Downward direction vector (normal)
                    return Vector3.Cross(side1, side2).normalized;
                }
            }

            // The square is parallel to the x-z plane.
            return Vector3.up;
        }


        // Function to parse a string and return a T object based on the string
        public static Cell ParseStringToCell((int,int) coors, string pattern, string[,] matrix) {
            if (pattern.Length != 2) {
                throw new ArgumentException("Input string must be of length 2");
            }
            int dimX = matrix.GetLength(0);
            int dimY = matrix.GetLength(1);

            // Split the string into letter and number
            string letter = pattern.Substring(0, 1); // Extract the first character
            string numberStr = pattern.Substring(1);  // Extract the rest of the string


            // Convert the string representation of the number to an integer
            if (!int.TryParse(numberStr, out int height)) {
                throw new ArgumentException("Invalid number format");
            }


            Cell res;
            Vector2Int convertedCoords = FromMatrixCorsToVector(coors, dimX, dimY);

            // Define switch cases based on the letter
            switch (letter) {
                case "F":
                    res = new CellFloor(convertedCoords, height);
                    break;
                case "V":
                    res = new CellVoid(convertedCoords, height);
                    break;
                case "O":
                    res = new CellObstacle(convertedCoords, height);
                    break;
                case "W":
                    res = new CellSlope(convertedCoords, height, Vector2Int.up);
                    break;
                case "A":
                    res = new CellSlope(convertedCoords, height, Vector2Int.left);
                    break;
                case "S":
                    res = new CellSlope(convertedCoords, height, Vector2Int.down);
                    break;
                case "D":
                    res = new CellSlope(convertedCoords, height, Vector2Int.right);
                    break;
                // Add more cases as needed
                default:
                    throw new ArgumentException("Invalid letter");
            }

            return res;
        }

        /*****************************************************************************
         ************************* UTILITY *******************************************
         *****************************************************************************/
        public static Vector2Int FromMatrixCorsToVector((int, int) coors, int dimX, int dimY) {
            return new Vector2Int(coors.Item2, dimX - coors.Item1 - 1);
        }

        public void Show() {
            Util.DrawPoint(coorReal, coorReal + Vector3.up * 5, Color.magenta, 20);
        }
        public void Show(Color c) {
            Util.DrawPoint(coorReal, coorReal + Vector3.up * 5, c, 20);
        }

        public override string ToString() {
            return $"{this.CoorGrid}, h = {terrainHeight}";
        }

    }
}