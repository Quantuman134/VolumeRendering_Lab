using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;


namespace UnityVolumeRendering
{
    //store the volume data of drawing from controller, and some relative parameters. 
    public class DrawMask: MonoBehaviour
    {
        public float radius_Trace = 1.0f;
        public float radius_Area = 5.0f;

        private List<Vector3> kernel_Trace;
        private List<Vector3> kernel_Area;

        private float[] radiusHistory_Trace = new float[2];
        private float[] radiusHistory_Area = new float[2];

        public Color color_Trace = Color.white;
        public Color color_Area = new Color(1.0f, 1.0f, 0.0f, 0.05f);

        private int[] data_scaled;
        private Vector3 dim_scaled;

        private Vector3 scale = new Vector3(2, 2, 2);

        private Texture3D drawTexture;
        private Texture3D opTexture;

        private void Start()
        {
            radiusHistory_Trace[0] = radius_Trace;
            radiusHistory_Trace[1] = radius_Trace;
            radiusHistory_Area[0] = radius_Area;
            radiusHistory_Area[1] = radius_Area;

            kernel_Trace = kernelCal(radius_Trace);
            kernel_Area = kernelCal(radius_Area);

            color_Trace = Color.white;
            color_Area = Color.yellow;
            color_Area.a = 0.2f;
        }

        private void Update()
        {
            //update kernel
            radiusHistory_Trace[0] = radiusHistory_Trace[1];
            radiusHistory_Trace[1] = radius_Trace;
            radiusHistory_Area[0] = radiusHistory_Area[1];
            radiusHistory_Area[1] = radius_Area;

            if (radiusHistory_Trace[0] != radiusHistory_Trace[1])
            {
                kernel_Trace.Clear();
                kernel_Trace = kernelCal(radius_Trace);
            }
            if (radiusHistory_Area[0] != radiusHistory_Area[1])
            {
                kernel_Area.Clear();
                kernel_Area = kernelCal(radius_Area);
            }
        }

        public void DataInitial(Vector3 dim)
        {
            dim_scaled.x = (int)dim.x / scale.x;
            dim_scaled.y = (int)dim.y / scale.y;
            dim_scaled.z = (int)dim.z / scale.z;
            data_scaled = new int[(int)(dim_scaled.x * dim_scaled.y * dim_scaled.z)];
            for (int i = 0; i < dim_scaled.x * dim_scaled.y * dim_scaled.z; i++)
            {
                data_scaled[i] = 0;
            }
            GenerateTexture();
        }

        private List<Vector3> kernelCal(float radius)
        {
            Vector3 radius_scaled;
            radius_scaled.x = radius / scale.x;
            radius_scaled.y = radius / scale.y;
            radius_scaled.z = radius / scale.z;
            List<Vector3> kernel = new List<Vector3>();
            for (int i = (int)-radius_scaled.x; i <= radius_scaled.x; i++)
            {
                for (int j = (int)-radius_scaled.y; j <= radius_scaled.y; j++)
                {
                    for (int k = (int)-radius_scaled.z; k <= radius_scaled.z; k++)
                    {
                        float radius_sq = (float)Math.Pow(Math.Abs(i) / (Math.Abs(i) + Math.Abs(j) + Math.Abs(k) + 0.001), 2) * radius_scaled.x * radius_scaled.x + (float)Math.Pow(Math.Abs(j) / (Math.Abs(i) + Math.Abs(j) + Math.Abs(k) + 0.001), 2) * radius_scaled.y * radius_scaled.y + (float)Math.Pow(Math.Abs(k) / (Math.Abs(i) + Math.Abs(j) + Math.Abs(k) + 0.001), 2) * radius_scaled.z * radius_scaled.z;
                        if (i * i + j * j + k * k <= radius_sq)
                        {
                            kernel.Add(new Vector3(i, j, k));
                        }
                    }
                }
            }

            return kernel;
        }

        public void ClearFunction()
        {
            for (int i = 0; i < dim_scaled.x * dim_scaled.y * dim_scaled.z; i++)
            {
                data_scaled[i] = 0;
            }
            ClearTexture();
        }

        public void DrawPoint(string func, Vector3 pos)
        {
            float radius;
            List<Vector3> kernel = null;
            int maskValue = 0;
            Color color_temp = new Color(0, 0, 0, 0);

            if (func == "trace")
            {
                radius = radius_Trace;
                kernel = kernel_Trace;
                maskValue = 2;
                color_temp = color_Trace;
            }
            else if (func == "area")
            {
                radius = radius_Area;
                kernel = kernel_Area;
                maskValue = 1;
                color_temp = color_Area;
            }

            if (kernel != null && pos.x >= 0 && pos.y >= 0 && pos.z >= 0)
            {
                Vector3 pos_temp;
                pos_temp.x = (int)(pos.x / scale.x);
                pos_temp.y = (int)(pos.y / scale.y);
                pos_temp.z = (int)(pos.z / scale.z);
                foreach (Vector3 pos_kernel in kernel)
                {
                    Vector3 pos_draw = pos_temp + pos_kernel;
                    if (pos_draw.x >= 0 && pos_draw.y >= 0 && pos_draw.z >= 0)
                    {
                        if (maskValue > data_scaled[(int)(pos_draw.x + pos_draw.y * dim_scaled.x + pos_draw.z * dim_scaled.x * dim_scaled.z)])
                        {
                            data_scaled[(int)(pos_draw.x + pos_draw.y * dim_scaled.x + pos_draw.z * dim_scaled.x * dim_scaled.z)] = maskValue;
                            DateTime before = System.DateTime.Now;
                            UpdateTexture(new Vector3Int((int)pos_draw.x, (int)pos_draw.y, (int)pos_draw.z), color_temp);
                            DateTime after = System.DateTime.Now;
                            TimeSpan ts = after.Subtract(before);
                            Debug.Log("updateTexture:" + ts.TotalMilliseconds);
                        }
                    }
                }

                DateTime before1 = System.DateTime.Now;
                drawTexture.Apply();
                DateTime after1 = System.DateTime.Now;
                TimeSpan ts1 = after1.Subtract(before1);
                Debug.Log("draw_apply_new:" + ts1.TotalMilliseconds);

                before1 = after1;
                opTexture.Apply();
                after1 = System.DateTime.Now;
                ts1 = after1.Subtract(before1);
                Debug.Log("op_apply_new:" + ts1.TotalMilliseconds);
            }
        }

        public void GenerateTexture()
        {
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf) ? TextureFormat.RGBAHalf : TextureFormat.RGBAFloat;
            drawTexture = new Texture3D((int)dim_scaled.x, (int)dim_scaled.y, (int)dim_scaled.z, texformat, false);
            drawTexture.wrapMode = TextureWrapMode.Clamp;
            Color[] cols = new Color[(int)dim_scaled.x * (int)dim_scaled.y * (int)dim_scaled.z];
            for (int x = 0; x < (int)dim_scaled.x; x++)
            {
                for (int y = 0; y < (int)dim_scaled.y; y++)
                {
                    for (int z = 0; z < (int)dim_scaled.z; z++)
                    {
                        int iData = x + y * (int)dim_scaled.x + z * ((int)dim_scaled.x * (int)dim_scaled.y);

                        Color temp = new Color(0, 0, 0, 0);
                        if (data_scaled[iData] == 0)
                        {
                            temp = new Color(0, 0, 0, 0);
                        }
                        else if (data_scaled[iData] == 1)
                        {
                            temp = color_Area;
                        }
                        else if (data_scaled[iData] == 2)
                        {
                            temp = color_Trace;
                        }
                        cols[iData] = temp;
                    }
                }
            }
            drawTexture.SetPixels(cols);
            drawTexture.Apply();

            //opTexture
            TextureFormat texformat2 = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf) ? TextureFormat.RGBAHalf : TextureFormat.RGBAFloat;
            opTexture = new Texture3D((int)dim_scaled.x, (int)dim_scaled.y, (int)dim_scaled.z, texformat, false);
            opTexture.wrapMode = TextureWrapMode.Clamp;
            Color[] cols2 = new Color[(int)dim_scaled.x * (int)dim_scaled.y * (int)dim_scaled.z];
            for (int x = 0; x < (int)dim_scaled.x; x++)
            {
                for (int y = 0; y < (int)dim_scaled.y; y++)
                {
                    for (int z = 0; z < (int)dim_scaled.z; z++)
                    {
                        int iData = x + y * (int)dim_scaled.x + z * ((int)dim_scaled.x * (int)dim_scaled.y);
                        cols2[iData] = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                        cols2[iData].r = (float)data_scaled[iData] / 2.0f;
                    }
                }
            }
            opTexture.SetPixels(cols2);
            opTexture.Apply();
        }

        private void UpdateTexture(Vector3Int pos, Color color)
        {
            if (drawTexture == null)
            {
                GenerateTexture();
            }
            DateTime before = System.DateTime.Now;
            drawTexture.SetPixel(pos.x, pos.y, pos.z, color);
            DateTime after = System.DateTime.Now;
            TimeSpan ts = after.Subtract(before);
            Debug.Log("draw_setpixel:" + ts.TotalMilliseconds);
            before = after;
            //drawTexture.Apply();
            after = System.DateTime.Now;
            ts = after.Subtract(before);
            Debug.Log("draw_apply:" + ts.TotalMilliseconds);

            Color color_temp = new Color((float)data_scaled[(int)(pos.x + pos.y * dim_scaled.x + pos.z * dim_scaled.x * dim_scaled.y)] / 2.0f, 0, 0, 0);

            before = after;
            opTexture.SetPixel(pos.x, pos.y, pos.z, color_temp);
            after = System.DateTime.Now;
            ts = after.Subtract(before);
            Debug.Log("op_setpixel:" + ts.TotalMilliseconds);

            before = after;
            //opTexture.Apply();
            after = System.DateTime.Now;
            ts = after.Subtract(before);
            Debug.Log("op_apply:" + ts.TotalMilliseconds);
        }

        private void ClearTexture()
        {
            if (drawTexture == null)
            {
                GenerateTexture();
            }
            Color[] cols = new Color[(int)dim_scaled.x * (int)dim_scaled.y * (int)dim_scaled.z];
            for (int x = 0; x < (int)dim_scaled.x; x++)
            {
                for (int y = 0; y < (int)dim_scaled.y; y++)
                {
                    for (int z = 0; z < (int)dim_scaled.z; z++)
                    {
                        int iData = x + y * (int)dim_scaled.x + z * ((int)dim_scaled.x * (int)dim_scaled.y);

                        Color temp = new Color(0, 0, 0, 0);
                        cols[iData] = temp;
                    }
                }
            }
            drawTexture.SetPixels(cols);
            drawTexture.Apply();
        }

        public Texture3D GetDrawTexture()
        {
            if (drawTexture == null)
            {
                GenerateTexture();
            }
            return drawTexture;
        }

        public Texture3D GetOpTexture()
        {
            if (opTexture == null)
            {
                GenerateTexture();
            }

            return opTexture;
        }

        //for user expriment, byte: uint16 endiness: little endiness
        public void LoadData(string dir, Vector3Int dim)
        {
            // Check that the file exists
            if (!File.Exists(dir))
            {
                Debug.LogError("The file does not exist: " + dir);
                return;
            }

            FileStream fs = new FileStream(dir, FileMode.Open);
            BinaryReader reader = new BinaryReader(fs);
            long expectedFileSize = (long)(dim.x * dim.y * dim.z) * 2;
            if (fs.Length < expectedFileSize)
            {
                Debug.LogError($"The dimension({dim.x}, {dim.y}, {dim.z}) exceeds the file size. Expected file size is {expectedFileSize} bytes, while the actual file size is {fs.Length} bytes");
                reader.Close();
                fs.Close();
                return;
            }

            int uDimensionTemp = dim.x * dim.y * dim.z;
            int[] dataTemp = new int[uDimensionTemp];
            for (int i = 0; i < uDimensionTemp; i++)
            {
                dataTemp[i] = reader.ReadUInt16();
                if (dataTemp[i] == 2)
                {
                    int test = 1;
                }
            }

            data_scaled = dataTemp;
            dim_scaled = dim;
            reader.Close();
            fs.Close();

            GenerateTexture();
        }

        public void WriteData(string dir)
        {
            if (!File.Exists(dir))
            {
                Debug.LogError("The file does not exist: " + dir);
                return;
            }

            FileStream fs = new FileStream(dir, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fs);
            int uDimensionTemp = (int)dim_scaled.x * (int)dim_scaled.y * (int)dim_scaled.z;
            for (int i = 0; i < uDimensionTemp; i++)
            {
                writer.Write((UInt16)data_scaled[i]);
            }

            writer.Close();
            fs.Close();
        }
    }
}
