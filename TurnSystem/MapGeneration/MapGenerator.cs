using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;



namespace k4k.map{
    public class MapGenerator : MonoBehaviour{

        public enum DrawMode {NoiseMap, ColourMap, FalloffMap, Mesh};
        public DrawMode drawMode = DrawMode.NoiseMap;

        [Range(1,100)] public int sizeX = 5;
        [Range(1,100)] public int sizeZ = 5;
        [Range(1,100)] public int noiseScale = 5;

        [Range(1,8)] public int octaves = 1;
        [Range(0,1)] public float persistance;
        [Range(1,5)] public float lacunarity;
        [Range(0,100)] public float heightMultiplier;
        [Range(0,1)] public float islandHeight;
        public AnimationCurve meshHeightCurve;
        public int seed;
        public Vector2 offset;
	    public TerrainType[] regions;

	    public bool useFalloff;
	    public bool autoUpdate;

        [SerializeField] GameObject pointObject; 

        float[,] falloffMap;
        private Dictionary<Vector3, GameObject> points = new Dictionary<Vector3, GameObject>();

        private GameMapHash gameMap;// = new GameMapHash();


        void Awake() {
            falloffMap = FalloffGenerator.GenerateFalloffMap (sizeX, sizeZ);
        }

        private void Start() {
            GenerateNewMap();
        }



        private void CleanMap(){
            foreach (GameObject point in points.Values)
                Destroy(point);
            points.Clear();
            //cellMap.Map((cells) => cells.Clear());
        }

        public void CreateNoiseMap(){
            float[,] noiseMap = Noise.GenerateNoiseMap (sizeX, sizeZ, seed, noiseScale, octaves, persistance, lacunarity, offset);

            if(useFalloff){
                for (int z = 0; z < sizeZ; z++) 
                    for (int x = 0; x < sizeX; x++)
                        noiseMap [x, z] = Mathf.Clamp01(noiseMap [x, z] - falloffMap [x, z]);
            }

            if(islandHeight > 0)
                noiseMap = Noise.AddIslandsToMap(noiseMap, islandHeight);

            Color[] colourMap = new Color[sizeX * sizeZ];
            for (int z = 0; z < sizeZ; z++) {
                for (int x = 0; x < sizeX; x++) {
                    float currentHeight = noiseMap [x, z];
                    for (int i = 0; i < regions.Length; i++) {
				        
                        if (currentHeight <= regions[i].height) {
                            colourMap [z * sizeX + x] = regions [i].colour;
                            break;
                        }
                    }
                }
            }

		    MapDisplay display = FindObjectOfType<MapDisplay> ();
            if (drawMode == DrawMode.NoiseMap) {
                display.DrawTexture (TextureGenerator.TextureFromHeightMap(noiseMap));
            } else if (drawMode == DrawMode.ColourMap) {
                display.DrawTexture (TextureGenerator.TextureFromColourMap(colourMap, sizeX, sizeZ));
            }else if (drawMode == DrawMode.FalloffMap) {
			    display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(sizeX, sizeZ)));
		    } else if (drawMode == DrawMode.Mesh) {
                if(islandHeight == 0)
                    display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, heightMultiplier,meshHeightCurve), 
                        TextureGenerator.TextureFromColourMap (colourMap, sizeX, sizeZ));
                else{
                    //Noise.AddIslandsToMap(noiseMap, islandHeight);
                    MeshData meshData = MeshGenerator.GenerateTerrainMesh(noiseMap, -1, heightMultiplier,meshHeightCurve);
                    display.DrawMesh(meshData, 
                        TextureGenerator.TextureFromColourMap (colourMap, sizeX, sizeZ));

                    gameMap = new GameMapHash(meshData.vertices, meshData.sizeX, meshData.sizeZ);
                }
		    }

        }



        public void GenerateNewMap(){
            CleanMap();
            CreateNoiseMap();
        }


        public void PopulateMap(){
            GenerateNewMap();

            List<List<(int, int)>> zones = Pathfinder.GetZones(gameMap);

            Debug.Log($"Found {zones.Count} zones");
            
            if(zones.Count <= 0)
                return;
            

            foreach (var coordinates in zones[0]){
                MapCell cell = gameMap.GetMapCell(coordinates);
                if(cell != null)
                    Util.DrawPoint(cell.Position, cell.Position + Vector3.up*5, Color.magenta);
            }

            if(zones.Count <= 1)
                return;

            foreach (var coordinates in zones[1]){
                MapCell cell = gameMap.GetMapCell(coordinates);
                if(cell != null)
                    Util.DrawPoint(cell.Position, cell.Position + Vector3.up*5, Color.blue);
            }
        }



        void OnValidate() {
            if (sizeX < 1) {
                sizeX = 1;
            }
            if (sizeZ < 1) {
                sizeZ = 1;
            }
            if (lacunarity < 1) {
                lacunarity = 1;
            }
            if (octaves < 0) {
                octaves = 0;
            }

            falloffMap = FalloffGenerator.GenerateFalloffMap (sizeX, sizeZ);
        }




        [System.Serializable]
        public struct TerrainType {
            public string name;
            public float height;
            public Color colour;
        }


    }
}