using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityVolumeRendering
{
    [Serializable]
    public class ViewBasedRendering : ScriptableObject
    {
        private VolumeDataset volumeData = null;
        private VolumeDataset volumeData_simp = null;

        private double gamma = 0.5378;//afile_2---0.0092,neghip---0.1345,skull---0.0264,foot---0.0397,fuel---0.0945,VisMale 0.3454, Nose_LhYang   0.0211, A024_head 0.0378
        private GDT GDT_simp = null;
        private GDT GDT_simp_Controller = null;
        private double GDT_T = 300;//原数据GDT，应用至simp时需要进行缩放变换,afile_2---120,neghip---30, a024_head 200
        private double GDT_T_Controller = 32;//手控器产生的GDT_T
        private Texture3D GDT_Texture = null;
        //坐标相关
        private Vector3 modelCoord;
        private Vector3 modelRotation;
        private Vector3 modelScale;
        private Vector3 cameraCoord;

        private Vector3 viewPos;//voxel coordinate


        public List<List<Vector3>> lineList = null;

        //simp GDT 中mask的尺寸
        private int mask_simp_size = 1;
        
        private int scale = 16;

        //property
        
        public double Gamma {
            get
            {
                return gamma;
            }
            set
            {
                gamma = value;
            }
        }

        public double GDT_Threshold
        {
            get
            {
                return GDT_T;
            }
            set
            {
                GDT_T = value;
            }
        }

        public double GDT_Threshold_Controller
        {
            get
            {
                return GDT_T_Controller;
            }
            set
            {
                GDT_T_Controller = value;
            }
        }


        public ViewBasedRendering(VolumeDataset volumeData, double view_X, double view_Y, double view_Z)
        {
            this.volumeData = volumeData;

            viewPos.x = (float)view_X;
            viewPos.y = (float)view_Y;
            viewPos.z = (float)view_Z;

            int dimX_temp = volumeData.dimX;
            int dimY_temp = volumeData.dimY;
            int dimZ_temp = volumeData.dimZ;
            if (volumeData.dimX % scale != 0)
            {
                dimX_temp = volumeData.dimX + scale - volumeData.dimX % scale;
            }
            if (volumeData.dimY % scale != 0)
            {
                dimY_temp = volumeData.dimY + scale - volumeData.dimY % scale;
            }
            if (volumeData.dimZ % scale != 0)
            {
                dimZ_temp = volumeData.dimZ + scale - volumeData.dimZ % scale;
            }

            //生成mask_simp
            
            int[] simp_data = Simp_data_Generate();
            int dimX_simp = dimX_temp / scale;
            int dimY_simp = dimY_temp / scale;
            int dimZ_simp = dimZ_temp / scale;
            volumeData_simp = new VolumeDataset();
            volumeData_simp.data = simp_data;
            volumeData_simp.dimX = dimX_simp;
            volumeData_simp.dimY = dimY_simp;
            volumeData_simp.dimZ = dimZ_simp;


            GDT_simp = new GDT(volumeData_simp, null, gamma/scale, GDT_T/scale);
            int[] mask_simp = Mask_Generate("simp");
            GDT_simp.mask = mask_simp;

            GDT_simp_Controller = new GDT(volumeData_simp, null, gamma / scale, GDT_T_Controller / scale);
            int[] mask_simp_Controller = Mask_Generate("Controller");
            GDT_simp_Controller.mask = mask_simp_Controller;

            DateTime beforDT = System.DateTime.Now;
            GDTUpdate(Vector3.zero, Vector3.zero, Vector3.one, Vector3.zero);

            DateTime afterDT = System.DateTime.Now;
            TimeSpan ts = afterDT.Subtract(beforDT);
            Debug.Log("update" + ts.TotalMilliseconds);
        }

        public void GDTUpdate(Vector3 modelCoord, Vector3 modelRotation, Vector3 modelScale, Vector3 cameraCoord)
        {
            this.modelCoord = modelCoord;
            this.modelRotation = modelRotation;
            this.modelScale = modelScale;
            this.cameraCoord = cameraCoord;

            View_Pos_Update(modelCoord, modelRotation, modelScale, cameraCoord);

            GDT_simp.mask = Mask_Generate("simp");

            GDT_simp_Controller.mask = Mask_Generate("Controller");

            GDT_simp.Raster_Scan();

            GDT_simp_Controller.Raster_Scan();

            //Normalization
            GDT_simp.Value_Normalization(0, 0.2);
            GDT_simp_Controller.Value_Normalization(0, 0.2);

            //GenerateTexture
            GenerateGDTTextrue();
        }

        //生成mask,"data"代表原数据，"simp"代表简化数据
        private int[] Mask_Generate(string str)
        {
            if (str == "simp")
            {
                int dimX = volumeData_simp.dimX;
                int dimY = volumeData_simp.dimY;
                int dimZ = volumeData_simp.dimZ;

                int view_x_temp = (int)(viewPos.x / scale);
                int view_y_temp = (int)(viewPos.y / scale);
                int view_z_temp = (int)(viewPos.z / scale);

                int[] mask = new int[dimX * dimY * dimZ];
                for (int x = 0; x < dimX; x++)
                {
                    for (int y = 0; y < dimY; y++)
                    {
                        for (int z = 0; z < dimZ; z++)
                        {
                            if (x >= view_x_temp - (mask_simp_size - 1) / 2 && x <= view_x_temp + (mask_simp_size - 1) / 2 && y >= view_y_temp - (mask_simp_size - 1) / 2 && y <= view_y_temp + (mask_simp_size - 1) / 2 && z >= view_z_temp + (mask_simp_size - 1) / 2 && z <= view_z_temp + (mask_simp_size - 1) / 2)
                            {
                                mask[x + dimX * y + dimX * dimY * z] = 0;
                            }
                            else
                            {
                                mask[x + dimX * y + dimX * dimY * z] = 1;
                            }
                        }
                    }
                }

                return mask;
            }
            else if (str == "Controller")
            {
                int dimX = volumeData_simp.dimX;
                int dimY = volumeData_simp.dimY;
                int dimZ = volumeData_simp.dimZ;

                int[] mask = new int[dimX * dimY * dimZ];
                for (int x = 0; x < dimX; x++)
                {
                    for (int y = 0; y < dimY; y++)
                    {
                        for (int z = 0; z < dimZ; z++)
                        {
                            mask[x + dimX * y + dimX * dimY * z] = 1;//初始化mask
                        }
                    }
                }
                if (lineList != null)
                {
                    //将手控器画线转化为mask
                    for (int i = 0; i < lineList.Count; i++)
                    {
                        List<Vector3> line = lineList[i];
                        for (int j = 0; j < line.Count; j++)
                        {
                            Vector3 pos = line[j];
                            pos = World2VoxelTransform(modelCoord, modelRotation, modelScale, pos);
                            if (pos.x >= 0 && pos.x < volumeData.dimX && pos.y >= 0 && pos.y < volumeData.dimY && pos.x >= 0 && pos.z < volumeData.dimZ)
                            {
                                Vector3 pos_simp = pos;
                                pos_simp.x = (int)(pos_simp.x / scale);
                                pos_simp.y = (int)(pos_simp.y / scale);
                                pos_simp.z = (int)(pos_simp.z / scale);
                                mask[(int)pos_simp.x + dimX * (int)pos_simp.y + dimX * dimY * (int)pos_simp.z] = 0;
                            }
                        }
                    }
                }

                return mask;

            }
            else
            {
                return null;
            }

        }

        //生成简化数据
        private int[] Simp_data_Generate()
        {
            int dimX_temp = volumeData.dimX;
            int dimY_temp = volumeData.dimY;
            int dimZ_temp = volumeData.dimZ;
            if (volumeData.dimX % scale != 0)
            {
                dimX_temp = volumeData.dimX + scale - volumeData.dimX % scale;
            }
            if (volumeData.dimY % scale != 0)
            {
                dimY_temp = volumeData.dimY + scale - volumeData.dimY % scale;
            }
            if (volumeData.dimZ % scale != 0)
            {
                dimZ_temp = volumeData.dimZ + scale - volumeData.dimZ % scale;
            }

            //将不足尺寸的体素补全
            int[] data_temp = new int[dimX_temp * dimY_temp * dimZ_temp];
            for (int x = 0; x < dimX_temp; x++)
            {
                for (int y = 0; y < dimY_temp; y++)
                {
                    for (int z = 0; z < dimZ_temp; z++)
                    {
                        if (x < volumeData.dimX && y < volumeData.dimY && z < volumeData.dimZ)
                        {
                            data_temp[x + y * dimX_temp + z * dimX_temp * dimY_temp] = volumeData.data[x + y * volumeData.dimX + z * volumeData.dimX * volumeData.dimY];
                        }
                        else
                        {
                            int x_temp = x;
                            int y_temp = y;
                            int z_temp = z;
                            if (x >= volumeData.dimX)
                            {
                                x_temp = volumeData.dimX - 1;
                            }
                            if (y >= volumeData.dimY)
                            {
                                y_temp = volumeData.dimY - 1;
                            }
                            if (z >= volumeData.dimZ)
                            {
                                z_temp = volumeData.dimZ - 1;
                            }
                            data_temp[x + y * dimX_temp + z * dimX_temp * dimY_temp] = volumeData.data[x_temp + y_temp * volumeData.dimX + z_temp * volumeData.dimX * volumeData.dimY];
                        }
                    }
                }
            }

            //生成mini_data
            int dimX_mini = dimX_temp / scale;
            int dimY_mini = dimY_temp / scale;
            int dimZ_mini = dimZ_temp / scale;
            int[] mini_data = new int[dimX_mini * dimY_mini * dimZ_mini];

            for (int x = 0; x < dimX_mini; x++)
            {
                for (int y = 0; y < dimY_mini; y++)
                {
                    for (int z = 0; z < dimZ_mini; z++)
                    {
                        int scalar = 0;
                        for (int i = 0; i < scale; i++)
                        {
                            for (int j = 0; j < scale; j++)
                            {
                                for (int k = 0; k < scale; k++)
                                {
                                    int x_temp = x * scale + i;
                                    int y_temp = y * scale + j;
                                    int z_temp = z * scale + k;
                                    scalar += data_temp[x_temp + y_temp * dimX_temp + z_temp * dimX_temp * dimY_temp];
                                }
                            }
                        }
                        scalar = scalar / (scale * scale * scale);
                        mini_data[x + y * dimX_mini + z * dimX_mini * dimY_mini] = scalar;

                    }
                }
            }

            return mini_data;
        }

        private void View_Pos_Update(Vector3 modelCoord, Vector3 modelRotation, Vector3 modelScale, Vector3 cameraCoord)
        {
            Vector3 relCoord = World2VoxelTransform(modelCoord, modelRotation, modelScale, cameraCoord);
            viewPos.x = relCoord.x;
            viewPos.y = relCoord.y;
            viewPos.z = relCoord.z;
        }

        //世界坐标系转相对体素原点坐标
        private Vector3 World2VoxelTransform(Vector3 modelCoord, Vector3 modelRotation, Vector3 modelScale, Vector3 worldCoord)
        {
            //与model中心相对坐标
            Vector3 relCoord = worldCoord - modelCoord;

            //旋转解算
            modelRotation.x = modelRotation.x / 180 * (float)Math.PI;
            modelRotation.y = modelRotation.y / 180 * (float)Math.PI;
            modelRotation.z = modelRotation.z / 180 * (float)Math.PI;


            //旋转矩阵
            Vector3 relCoordTemp = new Vector3();
            relCoordTemp.x = (float)(Math.Cos(modelRotation.z) * Math.Cos(modelRotation.y) * relCoord.x + Math.Sin(modelRotation.z) * Math.Cos(modelRotation.y) * relCoord.y + -Math.Sin(modelRotation.y) * relCoord.z);
            relCoordTemp.y = (float)((-Math.Sin(modelRotation.z) * Math.Cos(modelRotation.x) + Math.Cos(modelRotation.z) * Math.Sin(modelRotation.y) * Math.Sin(modelRotation.x)) * relCoord.x + (Math.Cos(modelRotation.z) * Math.Cos(modelRotation.x) + Math.Sin(modelRotation.z) * Math.Sin(modelRotation.y) * Math.Sin(modelRotation.x)) * relCoord.y + Math.Cos(modelRotation.y) * Math.Sin(modelRotation.x) * relCoord.z);
            relCoordTemp.z = (float)((Math.Sin(modelRotation.z) * Math.Sin(modelRotation.x) + Math.Cos(modelRotation.z) * Math.Sin(modelRotation.y) * Math.Cos(modelRotation.x)) * relCoord.x + (-Math.Cos(modelRotation.z) * Math.Sin(modelRotation.x) + Math.Sin(modelRotation.z) * Math.Sin(modelRotation.y) * Math.Cos(modelRotation.x)) * relCoord.y + Math.Cos(modelRotation.y) * Math.Cos(modelRotation.x) * relCoord.z);
            relCoord = relCoordTemp;
            //相对体素原点(x,y,z) = (0,0,0)的相对坐标
            relCoord.x = (relCoord.x + modelScale.x / 2) * volumeData.dimX / modelScale.x;
            relCoord.y = (relCoord.y + modelScale.y / 2) * volumeData.dimY / modelScale.y;
            relCoord.z = (relCoord.z + modelScale.z / 2) * volumeData.dimZ / modelScale.z;

            return relCoord;
        }

        public Texture3D GetGDTTexture()
        {
            if (GDT_Texture == null)
            {
                GenerateGDTTextrue();
            }

            return GDT_Texture;
        }

        //生成GDT纹理
        private Texture3D GenerateGDTTextrue()
        {
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RHalf) ? TextureFormat.RHalf : TextureFormat.RFloat;
            Texture3D texture = new Texture3D(volumeData_simp.dimX, volumeData_simp.dimY, volumeData_simp.dimZ, texformat, false);
            texture.wrapMode = TextureWrapMode.Clamp;

            Color[] cols = new Color[volumeData_simp.data.Length];
            for (int x = 0; x < volumeData_simp.dimX; x++)
            {
                for (int y = 0; y < volumeData_simp.dimY; y++)
                {
                    for (int z = 0; z < volumeData_simp.dimZ; z++)
                    {
                        //这里更新纹理像素中的颜色，之后还会更改
                        int iData = x + y * volumeData_simp.dimX + z * (volumeData_simp.dimX * volumeData_simp.dimY);
                        float min_GDT = Math.Min((float)GDT_simp.GDT_MAP[x + y * volumeData_simp.dimX + z * (volumeData_simp.dimX * volumeData_simp.dimY)], (float)GDT_simp_Controller.GDT_MAP[x + y * volumeData_simp.dimX + z * (volumeData_simp.dimX * volumeData_simp.dimY)]);
                        cols[iData] = new Color(min_GDT, 0.0f, 0.0f, 0.0f);

                    }
                }
            }

            texture.SetPixels(cols);
            texture.Apply();
            GDT_Texture = texture;

            return texture;
        }

        //about interaction
        public void UpdateGDTParameter(string str)
        {
            if (str == "simp")
            {
                GDT_simp.gamma = gamma/scale;
                GDT_simp.GDT_T = GDT_T/scale;
            }
            else if (str == "controller")
            {
                GDT_simp_Controller.gamma = gamma/scale;
                GDT_simp_Controller.GDT_T = GDT_T_Controller/scale;
            }

        }
    }
}
