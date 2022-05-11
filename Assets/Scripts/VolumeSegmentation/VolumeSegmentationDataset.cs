using System;
using UnityEngine;

namespace UnityVolumeRendering
{
    [Serializable]
    public class VolumeSegmentationDataset : ScriptableObject
    {
        [SerializeField]
        public int[] data = null;
        [SerializeField]
        public int dimX, dimY, dimZ;
        [SerializeField]
        static public int seg_num = LocalTransferFunctionDatabase.seg_num;//待修改 

        private int minDataValue = int.MaxValue;
        private int maxDataValue = int.MinValue;
        private Texture3D dataTexture = null;

        public VolumeSegmentationDataset(int dimX, int dimY, int dimZ)
        {
            this.dimX = dimX;
            this.dimY = dimY;
            this.dimZ = dimZ;
            data = new int[dimX * dimY * dimZ];
            for (int i = 0; i < dimX * dimY * dimZ; i++)
            {
                data[i] = 0;
            }
        }

        public Texture3D GetDataTexture()
        {
            if (dataTexture == null)
            {
                dataTexture = CreateTextureInternal();
            }
            return dataTexture;
        }

        public int GetMinDataValue()
        {
            if (minDataValue == int.MaxValue)
            {
                CalculateValueBounds();
            }
            return minDataValue;
        }

        public int GetMaxDataValue()
        {
            if (maxDataValue == int.MinValue)
            {
                CalculateValueBounds();
            }
            return maxDataValue;
        }

        private void CalculateValueBounds()
        {
            minDataValue = int.MaxValue;
            maxDataValue = int.MinValue;
            int dim = dimX * dimY * dimZ;
            for (int i = 0; i < dim; i++)
            {
                int val = data[i];
                minDataValue = Math.Min(minDataValue, val);
                maxDataValue = Math.Max(maxDataValue, val);
            }
        }

        private Texture3D CreateTextureInternal()
        {
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RHalf) ? TextureFormat.RHalf : TextureFormat.RFloat;
            Texture3D texture = new Texture3D(dimX, dimY, dimZ, texformat, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            int minValue = GetMinDataValue();
            int maxValue = GetMaxDataValue();

            Color[] cols = new Color[data.Length];
            for (int x = 0; x < dimX; x++)
            {
                for (int y = 0; y < dimY; y++)
                {
                    for (int z = 0; z < dimZ; z++)
                    {
                        int iData = x + y * dimX + z * (dimX * dimY);
                        cols[iData] = new Color(((float)data[iData]+0.5f)*(seg_num-2)/(seg_num-1)/(seg_num-1), 0.0f, 0.0f, 0.0f);//SegmentationID从0开始, 需要将纹理数据归一化,
                        //test
                        //cols[iData] = new Color((1.0f/15.0f), 0.0f, 0.0f, 0.0f);
                        //testend
                    }
                }
            }
            texture.SetPixels(cols);
            texture.Apply();
            return texture;
        }
    }
}
