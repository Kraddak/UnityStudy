using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace k4k.map{
public static class FalloffGenerator {

        public static float[,] GenerateFalloffMap(int sizeX, int sizeZ) {
            float[,] map = new float[sizeX,sizeZ];

            for (int z = 0; z < sizeZ; z++) {
                for (int x = 0; x < sizeX; x++) {
                    float a = x / (float)sizeX * 2 - 1;
                    float b = z / (float)sizeZ * 2 - 1;
                    //float a = z / (float)sizeX * 2 - 1;
                    //float b = x / (float)sizeZ * 2 - 1;

                    float value = Mathf.Max (Mathf.Abs (a), Mathf.Abs (b));
                    map [x, z] = Evaluate(value);
                }
            }

            return map;
        }

        static float Evaluate(float value) {
            float a = 3;
            float b = 6.2f;

            return Mathf.Pow (value, a) / (Mathf.Pow (value, a) + Mathf.Pow (b - b * value, a));
        }



    }
}