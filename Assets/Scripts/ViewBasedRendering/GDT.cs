using System;
using UnityEngine;
using UnityEditor;
namespace UnityVolumeRendering
{
    [Serializable]
    public class GDT : ScriptableObject
    {
        private VolumeDataset volumeData = null;
        private double[] GDT_Map = null;
        public int[] mask;
        public double gamma = 1.0;//梯度权重参数
        private int[] kernel = new int[]
        {
            1, -1, -1,
            0, 0, -1,
            0, -1, 0,
            -1, 1, -1,
            -1, 0, 0,
            -1, -1, 1,
            1, 0, -1,
            1, -1, 0,
            0, 1, -1,
            0, 0, 0
        };
        private int kernel_size = 10;
        public double GDT_T = 0;
        public double MAX_GDT = 99999999;

        //property
        public double[] GDT_MAP
        {
            get
            {
                return GDT_Map;
            }
        }

        public GDT(VolumeDataset volumeData, int[] mask, double gamma, double GDT_T)
        {
            this.volumeData = volumeData;
            this.mask = mask;
            this.gamma = gamma;
            this.GDT_T = GDT_T;
        }

        public double GDT_cal(int x1, int y1, int z1, int x2, int y2, int z2)
        {
            int dimX = volumeData.dimX;
            int dimY = volumeData.dimY;
            int dimZ = volumeData.dimZ;
            double GDT = 0;
            double v1 = volumeData.data[x1 + y1 * dimX + z1 * dimX * dimY];
            double v2 = volumeData.data[x2 + y2 * dimX + z2 * dimX * dimY];

            //图像坐标欧式平方距离
            double term1 = (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2) + (z1 - z2) * (z1 - z2);

            //体素梯度
            double term2 = (v1 - v2) * (v1 - v2) * gamma * gamma;

            //GDT
            GDT = GDT_Map[x2 + y2 * dimX + z2 * dimX * dimY] + Math.Sqrt(term1 + term2);

            return GDT;
        }

        public double[] Raster_Scan()
        {
            //运行时间统计
            //DateTime beforDT = System.DateTime.Now;
            int dimX = volumeData.dimX;
            int dimY = volumeData.dimY;
            int dimZ = volumeData.dimZ;

            //initialization
            GDT_Map = new double[dimX * dimY * dimZ];
            for (int i = 0; i < dimX; i++)
            {
                for (int j = 0; j < dimY; j++)
                {
                    for (int k = 0; k < dimZ; k++)
                    {
                        if (mask[i + dimX * j + dimX * dimY * k] == 0)
                        {
                            GDT_Map[i + dimX * j + dimX * dimY * k] = 0;
                        }
                        else
                        {
                            GDT_Map[i + dimX * j + dimX * dimY * k] = MAX_GDT;
                        }
                    }
                }
            }
            //Raster_Scann 扫描起始点从0，0，0点开始，延X-Y-Z轴的顺序移动
            //first scan
            for (int i = 0; i < dimX + dimY + dimZ - 2; i++)
            {
                int data_x = 0;
                int data_y = 0;
                int data_z = 0;
                if (i < dimX)
                {
                    data_x = i;
                    data_y = 0;
                    data_z = 0;
                }
                else if (dimX <= i && i < (dimX + dimY - 1))
                {
                    data_x = dimX - 1;
                    data_y = i - dimX + 1;
                    data_z = 0;
                }
                else
                {
                    data_x = dimX - 1;
                    data_y = dimY - 1;
                    data_z = i - dimX - dimY + 2;
                }

                while (data_x >= 0 && data_x < dimX && data_y >= 0 && data_y < dimY && data_z >= 0 && data_z < dimZ)
                {
                    while (data_x >= 0 && data_x < dimX && data_y >= 0 && data_y < dimY && data_z >= 0 && data_z < dimZ)
                    {
                        //更新距离
                        double[] GDT_distance = new double[kernel_size];
                        for (int j = 0; j < kernel_size; j++)
                        {
                            int kernel_index = j * 3;
                            int kernel_x = kernel[kernel_index];
                            int kernel_y = kernel[kernel_index + 1];
                            int kernel_z = kernel[kernel_index + 2];

                            //基准点是否超出界限
                            if ((data_x + kernel_x) < 0 || (data_x + kernel_x) >= dimX || (data_y + kernel_y) < 0 || (data_y + kernel_y) >= dimY || (data_z + kernel_z) < 0 || (data_z + kernel_z) >= dimZ)
                            {
                                GDT_distance[j] = MAX_GDT;
                            }
                            else
                            {
                                GDT_distance[j] = GDT_cal(data_x, data_y, data_z, data_x + kernel_x, data_y + kernel_y, data_z + kernel_z);
                            }

                        }
                        double result_GDT = Find_Min(GDT_distance, kernel_size);
                        GDT_Map[data_x + data_y * dimX + data_z * dimX * dimY] = result_GDT;
                        //像素坐标更新
                        if (data_y - 1 >= 0 && data_z + 1 < dimZ)
                        {
                            data_y--;
                            data_z++;
                        }
                        else
                        {
                            int temp = data_y;
                            data_y = data_z;
                            data_z = temp;
                            break;
                        }
                    }

                    //更新起始点
                    if (data_x - 1 >= 0)
                    {
                        data_x--;
                        if (data_y + 1 < dimY)
                        {
                            data_y++;
                        }
                        else if (data_z + 1 < dimZ)
                        {
                            data_z++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            //sceond scan
            for (int i = 0; i < dimX + dimY + dimZ - 2; i++)
            {
                int data_x = dimX - 1;
                int data_y = dimY - 1;
                int data_z = dimZ - 1;
                if (i < dimX)
                {
                    data_x = dimX - 1 - i;
                    data_y = dimY - 1;
                    data_z = dimZ - 1;
                }
                else if (dimX <= i && i < (dimX + dimY - 1))
                {
                    data_x = 0;
                    data_y = dimY - 1 - (i - dimX + 1);
                    data_z = dimZ - 1;
                }
                else
                {
                    data_x = 0;
                    data_y = 0;
                    data_z = dimZ - 1 - (i - dimX - dimY + 2);
                }

                while (data_x >= 0 && data_x < dimX && data_y >= 0 && data_y < dimY && data_z >= 0 && data_z < dimZ)
                {
                    while (data_x >= 0 && data_x < dimX && data_y >= 0 && data_y < dimY && data_z >= 0 && data_z < dimZ)
                    {
                        //更新距离
                        double[] GDT_distance = new double[kernel_size];
                        for (int j = 0; j < kernel_size; j++)
                        {
                            int kernel_index = j * 3;
                            int kernel_x = -kernel[kernel_index];
                            int kernel_y = -kernel[kernel_index + 1];
                            int kernel_z = -kernel[kernel_index + 2];

                            //基准点是否超出界限
                            if ((data_x + kernel_x) < 0 || (data_x + kernel_x) >= dimX || (data_y + kernel_y) < 0 || (data_y + kernel_y) >= dimY || (data_z + kernel_z) < 0 || (data_z + kernel_z) >= dimZ)
                            {
                                GDT_distance[j] = MAX_GDT;
                            }
                            else
                            {
                                GDT_distance[j] = GDT_cal(data_x, data_y, data_z, data_x + kernel_x, data_y + kernel_y, data_z + kernel_z);
                            }

                        }
                        double result_GDT = Find_Min(GDT_distance, kernel_size);
                        GDT_Map[data_x + data_y * dimX + data_z * dimX * dimY] = result_GDT;
                        //像素坐标更新
                        if (data_y + 1 < dimY && data_z - 1 >= 0)
                        {
                            data_y++;
                            data_z--;
                        }
                        else
                        {
                            int temp = data_y;
                            data_y = data_z;
                            data_z = temp;
                            break;
                        }
                    }

                    //更新起始点
                    if (data_x + 1 < dimX)
                    {
                        data_x++;
                        if (data_y - 1 >= 0)
                        {
                            data_y--;
                        }
                        else if (data_z - 1 >= 0)
                        {
                            data_z--;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return GDT_Map;
        }

        public double Find_Min(double[] value_array, int len)
        {
            double min_value = value_array[0];
            for (int i = 1; i < len; i++)
            {
                if (min_value > value_array[i])
                {
                    min_value = value_array[i];
                }
            }
            return min_value;
        }

        public double Find_Max(double[] value_array, int len)
        {
            double max_value = value_array[0];
            for (int i = 1; i < len; i++)
            {
                if (max_value < value_array[i])
                {
                    max_value = value_array[i];
                }
            }
            return max_value;
        }

        //GDT阈值分割
        private void GDT_Threshold()
        {
            for (int i = 0; i < GDT_Map.Length; i++)
            {
                if (GDT_Map[i] > GDT_T)
                {
                    GDT_Map[i] = GDT_T + 1;//区分GDT_T+1与GDT，区分边界
                }
            }
        }

        //Threshold, 之后调整数组元素值范围
        public double[] Value_Normalization(double min, double max)
        {
            GDT_Threshold();
            int dimX = volumeData.dimX;
            int dimY = volumeData.dimY;
            int dimZ = volumeData.dimZ;

            for (int i = 0; i < dimX * dimY * dimZ; i++)
            {
                if (GDT_Map[i] == GDT_T + 1)
                {
                    GDT_Map[i] = 1;//分割外范围保持1
                }
                else
                {
                    GDT_Map[i] = GDT_Map[i] * (max - min) / (GDT_T - 0);//内部线性重分配
                }
            }
            return GDT_Map;
        }

    }
}
