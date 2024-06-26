using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;




namespace k4k.map{
    public class GridSystem {
        private Dictionary<Vector2Int, Cell> grid;
        private  float cellSideLength;
        public float CellSideLength {
            get { return cellSideLength; }
        }
        private  float terrainStartOffsetX;
        private  float terrainStartOffsetY;
        private  int dimX;
        private  int dimY;

        public  GridSystem(float cellSideLength, float terrainStartOffsetX, float terrainStartOffsetY, int dimX, int dimY) {
            grid = new Dictionary<Vector2Int, Cell>();
            this.cellSideLength = cellSideLength;
            this.terrainStartOffsetX = terrainStartOffsetX;
            this.terrainStartOffsetY = terrainStartOffsetY;
            this.dimX = dimX;
            this.dimY = dimY;

            Debug.Log($"dimx = {dimX}, dimy = {dimY}, offx = {terrainStartOffsetX}, offy = {terrainStartOffsetY}, side = {cellSideLength}");
        }

        public ICollection<Vector2Int> GetMapKeys => grid.Keys;

        public bool GetCell(Vector2Int v, out Cell cell) {
            return grid.TryGetValue(v, out cell);
        }


        public bool GetCell(int x, int y, out Cell cell) {
            return GetCell(new Vector2Int(x, y), out cell);
        }
        public bool GetCell((int, int) coors, out Cell cell) {
            return GetCell(coors.Item1, coors.Item2, out cell);
        }
        public bool GetCell(Vector3 v, out Cell cell) {
            float x = v.x - terrainStartOffsetX;
            float y = v.z - terrainStartOffsetY;

            // Calculate if over upper border
            x = x >= dimX ? dimX - 1 : x;
            y = y >= dimX ? dimY - 1 : y;

            // Calculate if over lower border
            x = x < 0 ? 0 : x;
            y = y < 0 ? 0 : y;

            Debug.Log($"{x} -> {Mathf.RoundToInt(x / cellSideLength)}, {y} -> {Mathf.RoundToInt(y / cellSideLength)}");
            bool ris = GetCell(Mathf.RoundToInt(x / cellSideLength), Mathf.RoundToInt(y / cellSideLength), out cell);

            return ris;
        }



        public bool SetCell(Cell c) {
            if (c.CoorGrid.x >= this.dimX || c.CoorGrid.y >= this.dimY)
                return false;
            return grid.TryAdd(c.CoorGrid, c);
        }
    
    
        public void Clear() {
            this.grid.Clear();
        }



        /*
            public GridPosition GetGridPosition(Vector3 worldPosition){
                return new GridPosition(
            Mathf.RoundToInt(worldPosition.x / cellSize), 
            Mathf.RoundToInt(worldPosition.z / cellSize)
        );
    }
        */
    }
}