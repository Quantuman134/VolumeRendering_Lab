using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace UnityVolumeRendering
{
    public class VolumeSegmentationImporter
    {
        string filePath;
        private int dimX;
        private int dimY;
        private int dimZ;

        public VolumeSegmentationImporter(string filePath, int dimX, int dimY, int dimZ)
        {
            this.filePath = filePath;
            this.dimX = dimX;
            this.dimY = dimY;
            this.dimZ = dimZ;
        }

        public VolumeSegmentationDataset Import()
        {
            int x = 0;
            int y = 0;
            int z = 0;
            int segmentationID = 0;
            VolumeSegmentationDataset vs = new VolumeSegmentationDataset(dimX,dimY,dimZ);
            int[] data = new int[dimX * dimY * dimZ];

            if (!File.Exists(filePath))
            {
                Debug.LogError("The file does not exist: " + filePath);
                return null;
            }

            StreamReader sr = new StreamReader(filePath);
            for (int v_num = 0; v_num < dimX * dimY * dimZ; v_num++)
            {
                string line = sr.ReadLine();
                int string_index = 0;
                int[] value = new int[4];
                for (int num = 0; num < 4; num++)
                {
                    string temp = null;
                    while (line[string_index] != ',')
                    {
                        temp = temp + line[string_index];
                        string_index++;
                        if (string_index >= line.Length)
                        {
                            break;
                        }
                    }
                    value[num] = int.Parse(temp);
                    string_index++;
                }

                //point0 -- x, point1 -- y, point2 -- z 
                segmentationID = value[0];
                x = value[1];
                y = value[2];
                z = value[3];
                data[x + dimX * y + dimX * dimY * z] = segmentationID;
            }

            vs.data = data;
            return vs;
        }
    }
}
