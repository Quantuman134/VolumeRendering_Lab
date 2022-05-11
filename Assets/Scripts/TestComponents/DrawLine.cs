using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace UnityVolumeRendering
{
    public class DrawLine : MonoBehaviour
    {
        private ControllerInteraction CI;
        private GameObject line_curr;
        private int pointNum_curr = 0;
        private int lineNum = 0;

        private Transform volumeTransform;
        public GameObject trackObject;
        Vector3Int dim;
        Vector3 modelCoord;
        Vector3 modelRotation;
        Vector3 modelScale;
        bool test = false;
        int loadLineFlag = 0;

        void Start()
        {
            CI = GameObject.Find("OVRCameraRig").GetComponent<ControllerInteraction>();
        }

        private void Update()
        {
            
            if (trackObject != null)
            {
                
                test = true;
                VolumeDataset dataset = trackObject.GetComponent<VolumeRenderedObject>().dataset;
                dim = new Vector3Int(dataset.dimX, dataset.dimY, dataset.dimZ);
                modelCoord = trackObject.GetComponent<Transform>().position;
                modelRotation = trackObject.GetComponent<Transform>().localRotation.eulerAngles;
                modelScale = trackObject.GetComponent<Transform>().localScale;
                
            }

            if (trackObject != null)
            {
                if (loadLineFlag == 0)
                {
                    loadLineFlag++;
                    //LoadData("C:/Users/admin/Desktop/Project/User experiment/ground truth/Nose_LhYang_line.raw", dim, modelCoord, modelRotation, modelScale);
                }
            }

            DrawLine_Mesh();
        }

        private void OnDestroy()
        {
            if (test)
            {
                //WriteData("C:/Users/admin/Desktop/Project/User experiment/user result/user12/Nose_LhYang_line_GDT.raw", dim, modelCoord, modelRotation, modelScale);
                //WriteData("C:/Users/admin/Desktop/Project/User experiment/user result/user12/Nose_LhYang_line_nGDT.raw", dim, modelCoord, modelRotation, modelScale);
            }
        }

        private void DrawLine_Mesh()
        {
            Vector4 newFrame = CI.TraceLine_MeshAPI();
            Vector3 newPos = newFrame;

            if (!newPos.Equals(Vector3.negativeInfinity))
            {
                if (newFrame.w == 1)
                {
                    lineNum += 1;
                    pointNum_curr = 1;
                    GameObject line = new GameObject("line" + lineNum);
                    line.transform.parent = gameObject.transform;
                    line_curr = line;

                    LineRenderer renderer = line_curr.AddComponent<LineRenderer>();
                    renderer.startWidth = 0.009f;
                    renderer.endWidth = 0.009f;
                    renderer.material = Resources.Load<Material>("Line");
                    renderer.positionCount = pointNum_curr;
                    renderer.SetPosition(pointNum_curr - 1, newPos);
                }
                else
                {
                    pointNum_curr += 1;
                    LineRenderer renderer = line_curr.GetComponent<LineRenderer>();
                    renderer.positionCount = pointNum_curr;
                    renderer.SetPosition(pointNum_curr - 1, newPos);
                }
            }
        }

        /*
        public void WriteData(string dir, Vector3Int dim, Vector3 modelCoord, Vector3 modelRotation, Vector3 modelScale)
        {
            List<List<Vector3>> lineList = CI.GetLineList();
            int[] data = new int[dim.x * dim.y * dim.z];
            for (int i = 0; i < dim.x * dim.y * dim.z; i++)
            {
                data[i] = 0;
            }

            foreach (List<Vector3> line in lineList)
            {
                foreach (Vector3 point in line)
                {
                    Vector3 temp  = World2VoxelTransform(dim, modelCoord, modelRotation, modelScale, point);
                    Vector3Int tempInt = new Vector3Int((int)temp.x, (int)temp.y, (int)temp.z);
                    if (tempInt.x + tempInt.y * dim.x + tempInt.z * dim.x * dim.y >= 0 && tempInt.x + tempInt.y * dim.x + tempInt.z * dim.x * dim.y < dim.x * dim.y * dim.z)
                    {
                        data[tempInt.x + tempInt.y * dim.x + tempInt.z * dim.x * dim.y] = 2;
                    }
                }
            }

            if (!File.Exists(dir))
            {
                Debug.LogError("The file does not exist: " + dir);
                return;
            }

            FileStream fs = new FileStream(dir, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fs);
            int uDimensionTemp = (int)dim.x * (int)dim.y * (int)dim.z;
            for (int i = 0; i < uDimensionTemp; i++)
            {
                writer.Write((UInt16)data[i]);
            }

            writer.Close();
            fs.Close();
        }
        */

        public void WriteData(string dir, Vector3Int dim, Vector3 modelCoord, Vector3 modelRotation, Vector3 modelScale)
        {
            List<List<Vector3>> lineList = CI.GetLineList();
            FileStream fs = new FileStream(dir, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fs);
            if (!File.Exists(dir))
            {
                Debug.LogError("The file does not exist: " + dir);
                return;
            }
            foreach (List<Vector3> line in lineList)
            {

                foreach (Vector3 point in line)
                {
                    Vector3 temp = World2VoxelTransform(dim, modelCoord, modelRotation, modelScale, point);
                    writer.Write((float)temp.x);
                    writer.Write((float)temp.y);
                    writer.Write((float)temp.z);
                }
            }
            writer.Close();
            fs.Close();
        }

        public void LoadData(string dir, Vector3Int dim, Vector3 modelCoord, Vector3 modelRotation, Vector3 modelScale)
        {
            List<Vector3> pointList = new List<Vector3>();
            // Check that the file exists
            if (!File.Exists(dir))
            {
                Debug.LogError("The file does not exist: " + dir);
                return;
            }
            FileStream fs = new FileStream(dir, FileMode.Open);
            BinaryReader reader = new BinaryReader(fs);
            long pointNum = fs.Length / (3 * 4);

            lineNum += 1;
            pointNum_curr = (int)pointNum;
            GameObject line = new GameObject("line" + lineNum);
            line.transform.parent = gameObject.transform;
            line_curr = line;

            LineRenderer renderer = line_curr.AddComponent<LineRenderer>();
            renderer.startWidth = 0.009f;
            renderer.endWidth = 0.009f;
            renderer.material = Resources.Load<Material>("Line");
            renderer.positionCount = pointNum_curr;
            for (int i = 0; i < pointNum_curr; i++)
            {
                Vector3 pos = new Vector3();
                //pos = reader.ReadVector3();
                pos = Voxel2WorldTransform(dim, modelCoord, modelRotation, modelScale, pos);
                renderer.SetPosition(i, pos);
            }

            reader.Close();
            fs.Close();
        }


        private Vector3 World2VoxelTransform(Vector3Int dim, Vector3 modelCoord, Vector3 modelRotation, Vector3 modelScale, Vector3 worldCoord)
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
            relCoord.x = (relCoord.x + modelScale.x / 2) * dim.x / modelScale.x;
            relCoord.y = (relCoord.y + modelScale.y / 2) * dim.y / modelScale.y;
            relCoord.z = (relCoord.z + modelScale.z / 2) * dim.z / modelScale.z;

            return relCoord;
        }

        private Vector3 Voxel2WorldTransform(Vector3Int dim, Vector3 modelCoord, Vector3 modelRotation, Vector3 modelScale, Vector3 voxCoord)
        {
            Vector3 worldCoord = voxCoord;

            worldCoord.x = worldCoord.x * modelScale.x / dim.x - modelScale.x / 2;
            worldCoord.y = worldCoord.y * modelScale.y / dim.y - modelScale.y / 2;
            worldCoord.z = worldCoord.z * modelScale.z / dim.z - modelScale.z / 2;

            modelRotation.x = modelRotation.x / 180 * (float)Math.PI;
            modelRotation.y = modelRotation.y / 180 * (float)Math.PI;
            modelRotation.z = modelRotation.z / 180 * (float)Math.PI;

            Vector3 worldCoordTemp = new Vector3();
            worldCoordTemp.x = (float)(Math.Cos(modelRotation.z) * Math.Cos(modelRotation.y) * worldCoord.x + ((-Math.Sin(modelRotation.z) * Math.Cos(modelRotation.x) + Math.Cos(modelRotation.z) * Math.Sin(modelRotation.y) * Math.Sin(modelRotation.x)) * worldCoord.y + ((Math.Sin(modelRotation.z) * Math.Sin(modelRotation.x) + Math.Cos(modelRotation.z) * Math.Sin(modelRotation.y) * Math.Cos(modelRotation.x)) * worldCoord.z)));
            worldCoordTemp.y = (float)(Math.Sin(modelRotation.z) * Math.Cos(modelRotation.y) * worldCoord.x + (Math.Cos(modelRotation.z) * Math.Cos(modelRotation.x) + Math.Sin(modelRotation.z) * Math.Sin(modelRotation.y) * Math.Sin(modelRotation.x)) * worldCoord.y + (-Math.Cos(modelRotation.z) * Math.Sin(modelRotation.x) + Math.Sin(modelRotation.z) * Math.Sin(modelRotation.y) * Math.Cos(modelRotation.x)) * worldCoord.z);
            worldCoordTemp.z = (float)(-Math.Sin(modelRotation.y) * worldCoord.x + Math.Cos(modelRotation.y) * Math.Sin(modelRotation.x) * worldCoord.y + Math.Cos(modelRotation.y) * Math.Cos(modelRotation.x) * worldCoord.z);
            worldCoord = worldCoordTemp;

            worldCoord = worldCoord + modelCoord;

            return worldCoord;
        }

    }
}


