using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace k4k.map {
    [RequireComponent(typeof(Terrain)), ExecuteAlways]
    public class TerrainController : MonoBehaviour {
        public static TerrainController Instance;
        static private string ID = "TerrainController";

        public class TerrainConfig {
            public int StartSize { get; set; }
            public int EndSize { get; set; }
            public int CellMapSize { get; set; }
            public int CellSize { get; set; }
            public int HeightUnitTotal { get; set; }
            public float HeightRatio { get; set; }
            public int CellSizeOddEven { get; set; }
            public float HeightCell { get; set; }
            public float HeightStairStep { get; set; }
            public float NumberOfStairSteps { get; set; }
            public float CellSideLength { get; set; }
            public float TerrainStartOffsetY { get; set; }
            public float TerrainStartOffsetX { get; set; }
            public int MapSize { get; set; }
        }



        private Terrain terrain;
        private int terrainResolution;
        private int modifiedResolution;
        private float[,] mesh;
        private Vector3 point0;
        private TerrainConfig config;
        public int MapSize => config.MapSize ;

        public float getTerrainHeight() {
            if (terrain == null)
                throw new System.Exception($"{ID} Trying to access the terrain.heightmapHeight, but terrain was null!");
            return terrain.terrainData.size.y;
        }


        /*****************************************************************************
         ************************* LYFECYCLE *****************************************
         *****************************************************************************/

        private void Awake() {
            if (Instance != null) {
                Debug.Log("There is more than one instance of TerrainController " + transform + " - " + Instance);
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        private void OnEnable() {
            terrain = GetComponent<Terrain>();
        }
        private void OnValidate() {
            terrain = GetComponent<Terrain>();
        }



        /*****************************************************************************
         ************************* PRIVATE *******************************************
         *****************************************************************************/
        private void CalculateNewDimension(int cellSize, int heightUnitTotal, float heightRatio) {
            this.config = new TerrainConfig();

            this.config.CellSize = cellSize;
            this.config.HeightUnitTotal = heightUnitTotal;
            this.config.HeightRatio = heightRatio;

            this.config.CellSizeOddEven = cellSize % 2;
            this.config.HeightCell = Constants.CalculateIntervalSize(heightRatio, 1f, heightUnitTotal);
            this.config.NumberOfStairSteps = (cellSize / 2);
            this.config.HeightStairStep = config.HeightCell / config.NumberOfStairSteps;

            terrainResolution = terrain.terrainData.heightmapResolution;
            mesh = new float[terrainResolution, terrainResolution];
            modifiedResolution = terrainResolution - 2;

            // Making the used map smaller in comparison to the full map, but of an extremely small number
            int remainder = modifiedResolution % cellSize;
            config.StartSize = 1;
            config.EndSize = terrainResolution - 1;

            for (int i = 0; i < remainder; i++) {
                if (i % 2 == 0)
                    config.StartSize++;
                else
                    config.EndSize--;
            }
            config.CellMapSize = (config.EndSize - config.StartSize) / cellSize;

            Debug.Log($"{ID}:\n resolution (modified): {modifiedResolution}, cellSize: {cellSize}, resolution % cellsize = {remainder}\n" +
                        $" newStart: {config.StartSize}, newEnd: {config.EndSize}, cellMapSize: {config.CellMapSize}\n" +
                        $" is map divisible by cell ({cellSize}): {(config.EndSize - config.StartSize) % cellSize == 0}");


            var square = GetCellSquare(0, 0);
            this.config.CellSideLength = Mathf.Abs(square[0].x - square[2].x);/*
            |   __
            |  |  | <- This is a cell of the terrain
            |xx|__|
            |   z
            |___z______
            We are looking for the "xx" length, and "zz" length. Which should be equal */
            this.config.TerrainStartOffsetY = GetPosition(0, config.StartSize - 1).z;
            this.config.TerrainStartOffsetX = GetPosition(config.StartSize - 1, 0).x;
            //Util.DrawPoint(zz, zz + Vector3.up * 50, Color.black, 20);
            //Util.DrawPoint(xx, xx + Vector3.up * 50, Color.white, 20);
            
        }




        /*****************************************************************************
         ************************* TERRAIN HANDLING **********************************
         *****************************************************************************/
        public void Reset(int cellSize, int heightUnitTotal, float heightRatio) {
            CalculateNewDimension(cellSize, heightUnitTotal, heightRatio);
            for (int x = 0; x < terrainResolution; x++) {
                for (int y = 0; y < terrainResolution; y++) {
                    mesh[x, y] = 0;
                }
            }
            terrain.terrainData.SetHeights(0, 0, mesh);
        }

        public void Apply() {
            terrain.terrainData.SetHeights(0, 0, mesh);
        }
        public GridSystem Apply(string[,] inputPattern) {
            GridSystem gridSystem = new GridSystem(
                this.config.CellSideLength,
                config.TerrainStartOffsetX,
                config.TerrainStartOffsetY, 
                inputPattern.GetLength(0), 
                inputPattern.GetLength(1));
            
            Util.MapMatrix(inputPattern, (coor, pattern) => {
                var cell = Cell.ParseStringToCell(coor, pattern, inputPattern);
                SetCell(cell);
                gridSystem.SetCell(cell);
                return cell;
            });

            terrain.terrainData.SetHeights(0, 0, mesh);
            return gridSystem;
        }


        // CellY and CellX are inverted on purpose, since the terrain uses an inverted system of coordinates
        // The curve parameter function should return a value in the range [0, heightOffset]
        public void SetCell(int cellY, int cellX, System.Func<int, int, float> curve, bool debug = false) {
            if (!areValideCoordinates(cellX, cellY)) {
                Debug.LogError($"{ID}:\n SetCell overflow: ({cellX}, {cellY})\n" +
                    $" mapDimension was {config.CellMapSize}");
                return;
            }
            string debugString = "";

            for (int x = 0; x < config.CellSize; x++) {
                for (int y = 0; y < config.CellSize; y++) {
                    /* cellSize = 5
                     * [0,0] [1,0] [2,0] [3,0] [4,0]
                     * [0,1] [1,1] [2,1] [3,1] [4,1]
                     * [0,2] [1,2] [2,2] [3,2] [4,2]
                     * [0,3] [1,3] [2,3] [3,3] [4,3]
                     * [0,4] [1,4] [2,4] [3,4] [4,4] */

                    var modifiedX = (config.StartSize + cellX * config.CellSize + x);
                    var modifiedY = (config.StartSize + cellY * config.CellSize + y);

                    if (modifiedX >= terrainResolution || modifiedY >= terrainResolution) {
                        Debug.LogError($"{ID}:\n SetCell.mesh overflow: ({cellX}, {cellY})\n" +
                            $"Mesh coordinates: ({modifiedX}, {modifiedY})\n" +
                            $" resolution was {terrainResolution}");
                        return;
                    }/*
                        Consider that heightOffset is the basic terrain height.
                        curve(x,y) gives me a new height. But this height could be whatever.
                        If it is higher than heightOffset, then I might as well use it.
                        If it isn't higher */
                    mesh[modifiedX, modifiedY] = Mathf.Clamp01(curve(x, y));
                    if (debug)
                        Debug.Log($"{debugString}\n[{modifiedX},{modifiedY}] = {mesh[modifiedX, modifiedY]}");
                    debugString = "";
                }
            }
        }


        // Apply a function to every cell in the terrain
        public void MapCells(System.Func<int, int, float> cellFunction) {
            for (int cellX = 0; cellX < config.CellMapSize; cellX++) {
                for (int cellY = 0; cellY < config.CellMapSize; cellY++) {
                    SetCell(cellX, cellY, cellFunction);
                }
            }
        }


        /*****************************************************************************
         ************************* DEFAULT TERRAIN HANDLING **************************
         *****************************************************************************/
        public void SetCell(int cellX, int cellY, float newHeight) {
            SetCell(cellX, cellY, (_, _) => newHeight);
        }
        public void SetCell(Vector2Int point, float newHeight) {
            SetCell(point.x, point.y, (_, _) => newHeight);
        }
        public void SetCell(Vector2Int point, int unitHeight) {
            SetCell(point, Constants.MapUnitToInterval(unitHeight, config.HeightRatio, config.HeightUnitTotal, config.HeightCell));
        }


        public void SetCell(Cell cell) {
            cell.AssignTerrain(this, this.config);
            //SetCell(cell.CoorGrid.x, cell.CoorGrid.y, cell.TerrainHeightLambda);
            cell.SetCoorWorld(GetCellSquare(cell.CoorGrid));

        }

        /*****************************************************************************
         ************************* COORDINATE MANIPULATIONS **************************
         *****************************************************************************/
        public bool areValideCoordinates(Vector2Int coordinates) {
            return coordinates.x >= 0 && coordinates.x < config.CellMapSize &&
                   coordinates.y >= 0 && coordinates.y < config.CellMapSize;
        }
        public bool areValideCoordinates(int x, int y) {
            return x >= 0 && x < config.CellMapSize &&
                   y >= 0 && y < config.CellMapSize;
        }



        // Function to get the world position of a point in the heightmap
        private Vector3 GetPosition(int x, int z) {
            TerrainData terrainData = terrain.terrainData;

            // Calculate the size of each cell in world units
            float pointSizeX = terrainData.size.x / (this.terrainResolution - 1);
            float pointSizeY = terrainData.size.z / (terrainResolution - 1);

            // Calculate the position of the specified cell in world space
            float worldPosX = terrain.transform.position.x + x * pointSizeX;
            float worldPosZ = terrain.transform.position.z + z * pointSizeY;

            // Get the height at the specified cell
            float normalizedHeight = mesh[z,x];
            float worldPosY = terrain.transform.position.y + normalizedHeight * terrainData.size.y;
            //Debug.Log($"mesh[{x},{z}] = {normalizedHeight}, = {worldPosY}");
            //float worldPosY = terrain.transform.position.y + normalizedHeight * terrainData.size.y;

            // Create a Vector3 for the world position (including height)
            Vector3 worldPosition = new Vector3(worldPosX, worldPosY, worldPosZ);
            //Util.DrawPoint(worldPosition, worldPosition + Vector3.up * 5, Color.black, 20);

            return worldPosition;
        }


        private Vector3[] GetCellSquare(Vector2Int coor) {
            return GetCellSquare(coor.x, coor.y);
        }
        private Vector3[] GetCellSquare(int x, int z) {
            if (!areValideCoordinates(x, z)) {
                Debug.LogError($"GetPosition({x}{z}): out of bounds");
                return new Vector3[] {Vector3.zero};
            }

            int modX = x * config.CellSize;
            int modZ = z * config.CellSize;

            return new Vector3[] {
                GetPosition(modX + config.StartSize, modZ + config.StartSize),
                GetPosition(modX + config.StartSize, modZ + config.StartSize + config.CellSize - 1),
                GetPosition(modX + config.StartSize + config.CellSize -1, modZ + config.StartSize),
                GetPosition(modX + config.StartSize + config.CellSize -1, modZ + config.StartSize + config.CellSize - 1),
            };
        }








    }
}