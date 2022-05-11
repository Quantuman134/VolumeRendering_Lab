using System;
using UnityEngine;

namespace UnityVolumeRendering
{
    [Serializable]
    public class VolumeDataset : ScriptableObject
    {
        [SerializeField]
        public int[] data = null;
        [SerializeField]
        public int dimX, dimY, dimZ;
        [SerializeField]
        public float scaleX = 0.0f, scaleY = 0.0f, scaleZ = 0.0f;//?

        [SerializeField]
        public string datasetName;

        private int minDataValue = int.MaxValue;
        private int maxDataValue = int.MinValue;

        //Gradient & Normal
        private Vector3[] dataGrad = null;
        private Vector3[] volNorm = null;
        private float[] dataGradMold = null;

        //texture3D
        private Texture3D dataTexture = null;

        private Texture3D gradTexture = null;
        private Texture3D normTexture = null;
        private Texture3D gradMoldTexture = null;


        public Texture3D GetDataTexture()
        {
            if (dataTexture == null)
            {
                dataTexture = CreateTextureInternal();
            }
            return dataTexture;
        }

        public Texture3D GetGradTexture()
        {
            if (gradTexture == null)
            {
                Generate3DGradTexture();
            }

            return gradTexture;
        }

        public Texture3D GetNormTexture()
        {
            if (normTexture == null)
            {
                Generate3DNormTexture();
            }

            return normTexture;
        }

        public Texture3D GetGradMoldTexture()
        {
            if (gradMoldTexture == null)
            {
                Generate3DGradMoldTexture();
            }

            return gradMoldTexture;
        }

        public int GetMinDataValue()
        {
            if (minDataValue == int.MaxValue)
                CalculateValueBounds();
            return minDataValue;
        }

        public int GetMaxDataValue()
        {
            if (maxDataValue == int.MinValue)
                CalculateValueBounds();
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

        //建立内置三维纹理
        private Texture3D CreateTextureInternal()
        {
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RHalf) ? TextureFormat.RHalf : TextureFormat.RFloat;
            Texture3D texture = new Texture3D(dimX, dimY, dimZ, texformat, false);
            texture.wrapMode = TextureWrapMode.Clamp;

            int minValue = GetMinDataValue();
            int maxValue = GetMaxDataValue();
            int maxRange = maxValue - minValue;

            Color[] cols = new Color[data.Length];
            for (int x = 0; x < dimX; x++)
            {
                for (int y = 0; y < dimY; y++)
                {
                    for (int z = 0; z < dimZ; z++)
                    {
                        int iData = x + y * dimX + z * (dimX * dimY);
                        cols[iData] = new Color((float)(data[iData] - minValue) / maxRange, 0.0f, 0.0f, 0.0f);
                    }
                }
            }
            texture.SetPixels(cols);
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            return texture;
        }

        private void Generate3DGradTexture()
        {
            if (dataGrad == null)
            {
                GradCal();
            }
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf) ? TextureFormat.RGBAHalf : TextureFormat.RGBAFloat;
            gradTexture = new Texture3D(dimX, dimY, dimZ, texformat, false);
            gradTexture.wrapMode = TextureWrapMode.Clamp;
            Color[] cols = new Color[dimX * dimY * dimZ];
            for (int x = 0; x < dimX; x++)
            {
                for (int y = 0; y < dimY; y++)
                {
                    for (int z = 0; z < dimZ; z++)
                    {
                        int iData = x + y * dimX + z * (dimX * dimY);
                        Color temp = new Color();
                        temp.r = dataGrad[iData].x;
                        temp.g = dataGrad[iData].y;
                        temp.b = dataGrad[iData].z;

                        cols[iData] = temp;
                    }
                }
            }
            gradTexture.SetPixels(cols);
            gradTexture.Apply();

        }

        private void Generate3DNormTexture()
        {
            if (dataGrad == null)
            {
                GradCal();
            }
            if (volNorm == null)
            {
                NormCal();
            }

            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf) ? TextureFormat.RGBAHalf : TextureFormat.RGBAFloat;
            normTexture = new Texture3D(dimX, dimY, dimZ, texformat, false);
            normTexture.wrapMode = TextureWrapMode.Clamp;
            Color[] cols = new Color[dimX * dimY * dimZ];
            for (int x = 0; x < dimX; x++)
            {
                for (int y = 0; y < dimY; y++)
                {
                    for (int z = 0; z < dimZ; z++)
                    {
                        int iData = x + y * dimX + z * (dimX * dimY);
                        Color temp = new Color();
                        temp.r = volNorm[iData].x;
                        temp.g = volNorm[iData].y;
                        temp.b = volNorm[iData].z;

                        cols[iData] = temp;
                    }
                }
            }
            normTexture.SetPixels(cols);
            normTexture.Apply();
        }

        private void Generate3DGradMoldTexture()
        {
            if (dataGrad == null)
            {
                GradCal();
            }
            if (dataGradMold == null)
            {
                GradMoldCal();
            }

            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RHalf) ? TextureFormat.RHalf : TextureFormat.RFloat;
            gradMoldTexture = new Texture3D(dimX, dimY, dimZ, texformat, false);
            gradMoldTexture.wrapMode = TextureWrapMode.Clamp;
            Color[] cols = new Color[dimX * dimY * dimZ];
            for (int x = 0; x < dimX; x++)
            {
                for (int y = 0; y < dimY; y++)
                {
                    for (int z = 0; z < dimZ; z++)
                    {
                        int iData = x + y * dimX + z * (dimX * dimY);
                        Color temp = new Color();
                        temp.r = dataGradMold[iData];
                        cols[iData] = temp;
                    }
                }
            }
            gradMoldTexture.SetPixels(cols);
            gradMoldTexture.Apply();
        }
        //computation
        private void GradCal()
        {
            dataGrad = new Vector3[dimX * dimY * dimZ];

            for (int x = 0; x < dimX; x++)
            {
                for (int y = 0; y < dimY; y++)
                {
                    for (int z = 0; z < dimZ; z++)
                    {
                        int x1, x2, y1, y2, z1, z2;
                        x1 = x - 1;
                        x2 = x + 1;
                        y1 = y - 1;
                        y2 = y + 1;
                        z1 = z - 1;
                        z2 = z + 1;
                        if (x - 1 < 0)
                        {
                            x1 = x;
                        }
                        if (x + 1 >= dimX)
                        {
                            x2 = x;
                        }
                        if (y - 1 < 0)
                        {
                            y1 = y;
                        }
                        if (y + 1 >= dimY)
                        {
                            y2 = y;
                        }
                        if (z - 1 < 0)
                        {
                            z1 = z;
                        }
                        if (z + 1 >= dimZ)
                        {
                            z2 = z;
                        }
                        Vector3 tempGrad = new Vector3(0.0f, 0.0f, 0.0f);
                        int test1 = data[x + y * dimX + z2 * dimX * dimY];
                        int test2 = data[x + y * dimX + z1 * dimX * dimY];

                        tempGrad.x = 1.0f / 2 * (data[x2 + y * dimX + z * dimX * dimY] - data[x1 + y * dimX + z * dimX * dimY]);
                        tempGrad.y = 1.0f / 2 * (data[x + y2 * dimX + z * dimX * dimY] - data[x + y1 * dimX + z * dimX * dimY]);
                        tempGrad.z = 1.0f / 2 * (data[x + y * dimX + z2 * dimX * dimY] - data[x + y * dimX + z1 * dimX * dimY]);
                        dataGrad[x + dimX * y + dimX * dimY * z] = tempGrad;


                    }
                }
            }
        }

        private void NormCal()
        {
            volNorm = new Vector3[dimX * dimY * dimZ];
            for (int x = 0; x < dimX; x++)
            {
                for (int y = 0; y < dimY; y++)
                {
                    for (int z = 0; z < dimZ; z++)
                    {
                        volNorm[x + y * dimX + z * dimX * dimY] = dataGrad[x + y * dimX + z * dimX * dimY].normalized;
                    }
                }
            }
        }

        private void GradMoldCal()
        {
            dataGradMold = new float[dimX * dimY * dimZ];
            for (int x = 0; x < dimX; x++)
            {
                for (int y = 0; y < dimY; y++)
                {
                    for (int z = 0; z < dimZ; z++)
                    {
                        dataGradMold[x + y * dimX + z * dimX * dimY] = dataGrad[x + y * dimX + z * dimX * dimY].magnitude;
                        if (dataGrad[x + y * dimX + z * dimX * dimY].magnitude != 0)
                        {
                            float test = dataGrad[x + y * dimX + z * dimX * dimY].magnitude;
                        }
                    }
                }
            }
        }
    }
}
