using k4k.map;
using k4k.wfc;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




namespace k4k {
    [RequireComponent(typeof(TerrainController))]
    public class EditorTerrain : MonoBehaviour {
        public static EditorTerrain Instance;


        public bool autoUpdate;
        [Range(2,20)] [SerializeField] int cellSize = 6;
        [Range(0, .75f)] [SerializeField] float heightRatio = .5f;
        [Range(5, 40)] [SerializeField] int totalHeight = 20;
        [SerializeField] int seed = 0; // Change this seed to generate different maps


        private GridSystem grid;
        public GridSystem Grid {
            get { return grid; }
            //private set { grid = value; }
        }

        private TerrainController terrainController;
        private RandomMapGenerator rngMapGen;
        private float inverseHeightOffset;

        /*****************************************************************************
         ************************* LYFECYCLE *****************************************
         *****************************************************************************/

        private void Awake() {
            if (Instance != null) {
                Debug.Log("There is more than one instance of EditorTerrain " + transform + " - " + Instance);
                Destroy(gameObject);
                return;
            }
            Instance = this;
            inverseHeightOffset = Mathf.Abs(1 - heightRatio);
        }
        private void OnEnable() {
            terrainController = GetComponent<TerrainController>();
        }
        private void OnValidate() {
            terrainController = GetComponent<TerrainController>();
        }
        private void Start() {
        }


        public void GenerateMap() {
            inverseHeightOffset = Mathf.Abs(1 - heightRatio);
            var tHeight = terrainController.getTerrainHeight();
            this.transform.position = new Vector3(this.transform.position.x, -tHeight * heightRatio, this.transform.position.z);

            terrainController.Reset(cellSize, totalHeight, heightRatio);

            //this.grid = terrainController.Apply(example1);
            this.grid = terrainController.Apply(example2);

            if (this.grid.GetCell((0, 0), out Cell cell))
                cell.Show();
        }


        /*****************************************************************************
         ************************* PRIVATE *******************************************
         *****************************************************************************/






        static string[,] example1 = new string[,]{
            // 01    02    03    04       05    06    07    08       09    10    11    12
            { "F0", "F0", "F0", "F0",    "F0", "F0", "F0", "F0",    "F1", "F0", "F0", "F0"  }, // 01
            { "F0", "F0", "D0", "D1",    "D2", "F3", "F3", "F0",    "F0", "F0", "S0", "F0"  }, // 02
            { "F0", "O0", "D0", "D1",    "D2", "F3", "F3", "F0",    "F0", "F0", "S1", "F0"  }, // 03
            { "F0", "F0", "F0", "F0",    "F0", "F3", "F3", "F0",    "F1", "F0", "S2", "F0"  }, // 04
            
            { "F0", "F0", "F0", "F0",    "F9", "F8", "F8", "F9",    "V0", "V0", "F3", "F0"  }, // 05
            { "F0", "O0", "F0", "F7",    "D7", "F8", "F8", "F8",    "V0", "V0", "F3", "F0"  }, // 06
            { "F0", "F0", "F0", "W6",    "F8", "F8", "F8", "F8",    "V0", "V0", "W2", "F0"  }, // 07
            { "O0", "F0", "F0", "W5",    "F9", "F8", "F8", "F9",    "V0", "V0", "F2", "V0"  }, // 08
            
            { "F0", "F0", "F1", "F5",    "A4", "A3", "A2", "F2",    "F2", "F2", "F2", "V0"  }, // 09
            { "S0", "S0", "F0", "F0",    "V0", "V0", "V0", "V0",    "V0", "V0", "V0", "V0"  }, // 10
            { "F1", "F1", "A0", "F0",    "V0", "V0", "V0", "V0",    "V0", "V0", "V0", "V0"  }, // 11
            { "F1", "F1", "A0", "F0",    "V0", "V0", "V0", "V0",    "V0", "V0", "V0", "V0"  }, // 12
        };



        static string[,] example2 = new string[,]{
            { "F0", "F0",   "F0",   "F0", "F0",},
            { "F0", "F0",   "F0",   "S0", "F0",},

            { "F0", "F0",   "F0",   "W0", "F0",},

            { "F0", "D0",   "A0",   "F0", "F0",},
            { "F0", "F0",   "F0",   "F0", "F0",},
        };

        static string[,] matrix2 = new string[,]{
            { "F3", "A2", "F2"},
            { "V0", "V0", "W1"},
            { "F0", "F0", "F1"},
        };





    }

}



/*


        static string[,] example1 = new string[,]{
            // 01    02    03    04    05    06    07    08    09    10
            { "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0" }, // 01
            { "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0" }, // 02
            { "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0" }, // 03
            { "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0" }, // 04
            { "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0" }, // 05
            { "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0" }, // 06
            { "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0" }, // 07
            { "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0" }, // 08
            { "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0" }, // 09
            { "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0", "V0" }, // 10
        };
 * */