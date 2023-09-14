using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace k4k.map{
    public static class Noise {

        public static float[,] GenerateNoiseMap(int sizeX, int sizeZ, float scale) {
            float[,] noiseMap = new float[sizeX,sizeZ];

            if (scale <= 0) {
                scale = 0.0001f;
            }

            for (int z = 0; z < sizeZ; z++) {
                for (int x = 0; x < sizeX; x++) {
                    float sampleX = x / scale;
                    float sampleY = z / scale;

                    float perlinValue = Mathf.PerlinNoise (sampleX, sampleY);
                    noiseMap [x, z] = perlinValue;
                }
            }

            return noiseMap;
        }



		public static float[,] GenerateNoiseMap(int sizeX, int sizeZ, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset) {
			float[,] noiseMap = new float[sizeX,sizeZ];

			System.Random prng = new System.Random (seed);
			Vector2[] octaveOffsets = new Vector2[octaves];
			for (int i = 0; i < octaves; i++) {
				float offsetX = prng.Next (-100000, 100000) + offset.x;
				float offsetY = prng.Next (-100000, 100000) + offset.y;
				octaveOffsets [i] = new Vector2 (offsetX, offsetY);
			}

			if (scale <= 0) {
				scale = 0.0001f;
			}

			float maxNoiseHeight = float.MinValue;
			float minNoiseHeight = float.MaxValue;

			float halfWidth = sizeX / 2f;
			float halfHeight = sizeZ / 2f;


			for (int z = 0; z < sizeZ; z++) {
				for (int x = 0; x < sizeX; x++) {
			
					float amplitude = 1;
					float frequency = 1;
					float noiseHeight = 0;

					for (int i = 0; i < octaves; i++) {
						float sampleX = (x-halfWidth) / scale * frequency + octaveOffsets[i].x;
						float sampleY = (z-halfHeight) / scale * frequency + octaveOffsets[i].y;

						float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;
						noiseHeight += perlinValue * amplitude;

						amplitude *= persistance;
						frequency *= lacunarity;
					}

					if (noiseHeight > maxNoiseHeight) {
						maxNoiseHeight = noiseHeight;
					} else if (noiseHeight < minNoiseHeight) {
						minNoiseHeight = noiseHeight;
					}
					noiseMap [x, z] = noiseHeight;
				}
			}

			for (int y = 0; y < sizeZ; y++) {
				for (int x = 0; x < sizeX; x++) {
					noiseMap [x, y] = Mathf.InverseLerp (minNoiseHeight, maxNoiseHeight, noiseMap [x, y]);
				}
			}

			return noiseMap;
		}



		public static float[,] AddIslandsToMap(float[,] heightMap, float voidHeight){
			int width = heightMap.GetLength(0);
			int height = heightMap.GetLength(1);

			float[,] ris = new float[width,height];

			// Iterate through each cell in the height map
			for (int x = 0; x < width; x++){
				for (int y = 0; y < height; y++){
					// Set cells less than voidHeight to -1
					if (heightMap[x, y] < voidHeight){
						ris[x, y] = -1;
					}
					else{
						// Set boundary cells to 0
						if (IsShoreLineCell(heightMap, voidHeight, x, y)){
							ris[x, y] = 0;
						}else{
							ris[x, y] = heightMap[x, y] - voidHeight;
						}
					}
				}
			}
			return ris;
		}


		public static void AddIslandsToMap(float[,] heightMap1, float[,] heightMap2, float voidHeight){
			int width = heightMap1.GetLength(0);
			int height = heightMap1.GetLength(1);

			// Iterate through each cell in the height map
			for (int x = 0; x < width; x++){
				for (int y = 0; y < height; y++){
					// Set cells less than voidHeight to -1
					if (heightMap1[x, y] < voidHeight){
						heightMap1[x, y] = -1;
						heightMap2[x, y] = -1;
					}
					else{
						// Set boundary cells to 0
						if (IsShoreLineCell(heightMap1, voidHeight, x, y)){
							heightMap1[x, y] = voidHeight;
							heightMap2[x, y] = voidHeight;
						}
					}
				}
			}
		}








		public static bool IsShoreLineCell(float[,] heightMatrix, float voidHeight, int i, int j){
			if (heightMatrix[i,j] >= voidHeight){ // the cell is land
				// Check the 8 neighboring cells to see if they are sea
				for (int x = i-1; x <= i+1; x++)
					for (int y = j-1; y <= j+1; y++)
						if (x >= 0 && x < heightMatrix.GetLength(0) && y >= 0 && y < heightMatrix.GetLength(1))
							if (heightMatrix[x,y] < voidHeight) // the neighboring cell is sea
								return true;

				// If none of the neighboring cells are sea, then this is not a shore cell
				return false;
			}else{
				// This is a sea cell, and sea cells are not shore cells
				return false;
			}
		}




    }
}