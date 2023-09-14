using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace k4k.map{
    public static class MeshGenerator {

        public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve) {
            int width = heightMap.GetLength (0);
            int height = heightMap.GetLength (1);
            float topLeftX = (width - 1) / -2f;
            float topLeftZ = (height - 1) / 2f;

            MeshData meshData = new MeshData (width, height);
            int vertexIndex = 0;

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {

                    meshData.vertices [vertexIndex] = new Vector3(
                        topLeftX + x, 
                        heightCurve.Evaluate(heightMap [x, y]) * heightMultiplier,
                        topLeftZ - y);
                    meshData.uvs [vertexIndex] = new Vector2 (x / (float)width, y / (float)height);

                    if (x < width - 1 && y < height - 1) {
                        meshData.AddTriangle (vertexIndex, vertexIndex + width + 1, vertexIndex + width);
                        meshData.AddTriangle (vertexIndex + width + 1, vertexIndex, vertexIndex + 1);
                    }

                    vertexIndex++;
                }
            }

            return meshData;
        }

 


        public static MeshData GenerateTerrainMesh(float[,] heightMap, float voidHeight, float heightMultiplier, AnimationCurve heightCurve){
            int sizeX = heightMap.GetLength(0);
            int sizeZ = heightMap.GetLength(1);
            MeshData meshData = new MeshData(sizeX, sizeZ);

            int vertexIndex = 0;
            for (int x = 0; x < sizeX; x++){
                for (int z = 0; z < sizeZ; z++){

                    if (heightMap[x, z] > voidHeight){
                        Vector3 vertex = new Vector3(x, heightCurve.Evaluate(heightMap [x, z]) * heightMultiplier, z);
                        meshData.vertices[vertexIndex] = vertex;
                        // Set UV coordinates based on vertex position
                        meshData.uvs[vertexIndex] = new Vector2((float)x / sizeX, (float)z / sizeZ);

                        // Check neighboring vertices to create triangles
                        if (x < sizeX - 1 && z < sizeZ - 1 && 
                            heightMap[x + 1, z] > voidHeight && 
                            heightMap[x, z + 1] > voidHeight && 
                            heightMap[x + 1, z + 1] > voidHeight)
                        {
                            int a = vertexIndex;                // (x,   z)
                            int b = vertexIndex + sizeZ;        // (x+1, z)
                            int c = vertexIndex + sizeZ + 1;    // (x+1, z+1)
                            int d = vertexIndex + 1;            // (x,   z+1)

                            if(!(heightMap[x,z] == heightMap[x+1,z+1] && heightMap[x,z] == heightMap[x+1,z]))
                                meshData.AddTriangle(a, c, b);
                            if(!(heightMap[x+1,z+1] == heightMap[x,z] && heightMap[x,z] == heightMap[x,z+1]))
                                meshData.AddTriangle(c, a, d);
                        }
                    }

                    vertexIndex++;
                }
            }

            return meshData;
        }




   
    }



    public class MeshData {
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uvs;

        int triangleIndex;
        public int sizeX;
        public int sizeZ;

        public MeshData(int meshWidth, int meshHeight) {
            vertices = new Vector3[meshWidth * meshHeight];
            uvs = new Vector2[meshWidth * meshHeight];
            triangles = new int[(meshWidth-1)*(meshHeight-1)*6];
            sizeX = meshWidth;
            sizeZ = meshHeight;
        }

        public void AddTriangle(int a, int b, int c) {
            triangles [triangleIndex] = a;
            triangles [triangleIndex + 1] = b;
            triangles [triangleIndex + 2] = c;
            triangleIndex += 3;
        }

        public Mesh CreateMesh() {
            Mesh mesh = new Mesh ();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals ();
            return mesh;
        }
    } 


}