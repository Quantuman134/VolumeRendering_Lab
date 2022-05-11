using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System;

namespace UnityVolumeRendering
{
    public class ControllerInteraction : MonoBehaviour
    {
        private InputDevice rightController;
        private InputDevice leftController;

        private GameObject targetController;

        //记录手控器轨迹
        private List<List<Vector3>> lineList = new List<List<Vector3>>();

        //状态标志
        private bool drawingFlag = false;
        private bool GDTLocking = false;
        private bool viewScaleEvent = false;//是否触发视觉尺寸缩放事件

        //手控器输入值
        private double rightControllerPrimaryTumbY;
        private double leftControllerPrimaryTumbY;
        private bool[] rightSecondButton = new bool[2] { false, false };

        //data
        private struct Data_VR
        {
            //camera block function
            public Vector3 camera_blockCoord_world;
            public bool[] flag_cameraBlock;

            //camera teleport function
            public bool[] flag_teleport;

            //draw trace line(mesh)
            public bool newTraceline;
            public Vector3 linePos;

            //draw trace line
            public bool[] flag_traceLine;
            public List<Vector3> traceLineList;
            public bool[] flag_clearTraceLine;

            //draw mark area
            public bool[] flag_markArea;
            public List<Vector3> markAreaList;
            public bool[] flag_clearMarkArea;
        }

        Data_VR data_VR;

        //property
        public double RightControllerPrimaryTumbY
        {
            get
            {
                return rightControllerPrimaryTumbY;
            }
        }

        public double LeftControllerPrimaryTumbY
        {
            get
            {
                return leftControllerPrimaryTumbY;
            }
        }

        public bool ViewScaleEvent
        {
            get
            {
                return viewScaleEvent;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            List<InputDevice> devices = new List<InputDevice>();
            //InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
            InputDeviceCharacteristics ControllerCharacteristics = InputDeviceCharacteristics.Controller;
            //InputDeviceCharacteristics leftControllerCharacteristics = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
            InputDevices.GetDevicesWithCharacteristics(ControllerCharacteristics, devices);
            //InputDevices.GetDevicesWithCharacteristics(leftControllerCharacteristics, devices);

            foreach (var item in devices)
            {
                Debug.Log(item.name + item.characteristics);
            }

            if (devices.Count > 1)
            {
                leftController = devices[0];
                rightController = devices[1];
            }

            targetController = GameObject.Find("RightHandAnchor");

            data_VR.flag_cameraBlock = new bool[2] { false, false };
            data_VR.flag_teleport = new bool[2] { false, false };
            data_VR.flag_traceLine = new bool[2] { false, false };
            data_VR.traceLineList = new List<Vector3>();
            data_VR.flag_clearTraceLine = new bool[2] { false, false };
            data_VR.flag_markArea = new bool[2] { false, false };
            data_VR.markAreaList = new List<Vector3>();
            data_VR.flag_clearMarkArea = new bool[2] { false, false };

        }

        // Update is called once per frame
        void Update()
        {
            rightController.TryGetFeatureValue(CommonUsages.primaryButton, out bool rightPrimaryButtonValue);
            if (rightPrimaryButtonValue)
            {
                Debug.Log("Pressing right Primary Button");
            }

            leftController.TryGetFeatureValue(CommonUsages.primaryButton, out bool leftPrimaryButtonValue);
            if (leftPrimaryButtonValue)
            {
                Debug.Log("Pressing left Primary Button");
            }

            rightController.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue);
            if (triggerValue > 0.1f)
            {
                Debug.Log("Trigger pressed" + triggerValue);
            }

            rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 right2DVec);
            if (right2DVec != Vector2.zero)
            {
                Debug.Log("right thumbstick: " + "x: " + right2DVec[0] + "  y: " + right2DVec[1]);
            }

            leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 left2DVec);
            if (left2DVec != Vector2.zero)
            {
                Debug.Log("left thumbstick: " + "x: " + left2DVec[0] + "  y: " + left2DVec[1]);
            }

            rightControllerPrimaryTumbY = right2DVec[1];
            leftControllerPrimaryTumbY = left2DVec[1];

            RightControllerUpdate();
            //FlagDetector();
            //ClearFunction();
            viewScaleEventFlagDetector();

            DrawTraceLine_mesh();
            //draw trace line
            //DrawTraceLine();
            //ClearTraceLineFunction();
            DrawMarkArea();
            ClearMarkAreaFunction();

            CameraBlock();
            Teleport();
        }

        void RightControllerUpdate()
        {
            rightSecondButton[0] = rightSecondButton[1];
            rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out rightSecondButton[1]);
        }

        private void DrawTraceLine_mesh()
        {
            rightController.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue);
            if (triggerValue > 0.5f)
            {
                GameObject cursor = GameObject.Find("RightControllerCursor");
                //drawing
                if (drawingFlag)
                {
                    Vector3 pointPos = cursor.GetComponent<Transform>().position;
                    lineList[lineList.Count - 1].Add(pointPos);

                    data_VR.newTraceline = false;
                    data_VR.linePos = pointPos;
                }
                //start to draw
                else
                {
                    drawingFlag = true;
                    List<Vector3> line = new List<Vector3>();
                    Vector3 pointPos = cursor.GetComponent<Transform>().position;
                    line.Add(pointPos);
                    lineList.Add(line);

                    data_VR.newTraceline = true;
                    data_VR.linePos = pointPos;
                }
            }
            else
            {
                //end drawing
                if (drawingFlag)
                {
                    drawingFlag = false;

                    data_VR.newTraceline = false;
                    data_VR.linePos = Vector3.negativeInfinity;
                }
                //nothing
                else
                {
                    drawingFlag = false;

                    data_VR.newTraceline = false;
                    data_VR.linePos = Vector3.negativeInfinity;
                }
            }

            bool rightBButtonRiseEdge = false;
            if (rightSecondButton[1] == true && rightSecondButton[0] == false)
            {
                rightBButtonRiseEdge = true;
            }
            if (rightBButtonRiseEdge)
            {
                GDTLocking = !GDTLocking;
                Debug.Log("GDTLocking: " + GDTLocking);
            }
        }

        private void ClearFunction()
        {
            rightController.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButtonValue);
            if (primaryButtonValue)
            {
                foreach (List<Vector3> i in lineList)
                {
                    i.Clear();
                }
                lineList.Clear();
            }
        }

        public List<List<Vector3>> GetLineList()
        {
            return lineList;
        }

        public Vector4 TraceLine_MeshAPI()
        {
            Vector4 result = new Vector4();
            result.x = data_VR.linePos.x;
            result.y = data_VR.linePos.y;
            result.z = data_VR.linePos.z;
            result.w = 0;
            if (data_VR.newTraceline)
            {
                result.w = 1;
            }
            return result;
        }

        public bool GetGDTLockingFlag()
        {
            return GDTLocking;
        }

        public void viewScaleEventFlagDetector()
        {
            if (Math.Abs(rightControllerPrimaryTumbY) >= 0.1f)
            {
                viewScaleEvent = true;
            }
            else
            {
                viewScaleEvent = false;
            }
        }

        //Draw trace line using controller (right trigger)
        private void DrawTraceLine()
        {
            rightController.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue);
            if (triggerValue > 0.5f)
            {
                Debug.Log("draw trace line");
                data_VR.flag_traceLine[0] = data_VR.flag_traceLine[1];
                data_VR.flag_traceLine[1] = true;

                GameObject cursor = GameObject.Find("RightControllerCursor");
                Vector3 cursorPos = cursor.GetComponent<Transform>().position;
                data_VR.traceLineList.Add(cursorPos);
            }
            else
            {
                data_VR.flag_traceLine[0] = data_VR.flag_traceLine[1];
                data_VR.flag_traceLine[1] = false;
            }
        }

        //Clear trace line(right )
        private void ClearTraceLineFunction()
        {
            rightController.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButtonValue);
            data_VR.flag_clearTraceLine[0] = data_VR.flag_clearTraceLine[1];
            data_VR.flag_clearTraceLine[1] = primaryButtonValue;
            if (!data_VR.flag_clearTraceLine[0] && data_VR.flag_clearTraceLine[1])
            {
                data_VR.traceLineList.Clear();
            }
        }

        //API for VolumeRenderedObject
        public Vector3 TraceLineAPI()
        {
            if (data_VR.flag_traceLine[1])
            {
                return data_VR.traceLineList[data_VR.traceLineList.Count - 1];
            }
            else
            {
                if (!data_VR.flag_clearTraceLine[0] && data_VR.flag_clearTraceLine[1])
                {
                    //negativeInfinity is flag of line clearing
                    return Vector3.negativeInfinity;
                }
                else
                {
                    //postiveInfinity is flag of none drawing
                    return Vector3.positiveInfinity;
                }
            }
        }

        //Draw mark area using controller (left trigger)
        private void DrawMarkArea()
        {
            leftController.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue);
            if (triggerValue > 0.5f)
            {
                Debug.Log("draw mark area");
                data_VR.flag_markArea[0] = data_VR.flag_markArea[1];
                data_VR.flag_markArea[1] = true;

                GameObject cursor = GameObject.Find("LeftControllerCursor");
                Vector3 cursorPos = cursor.GetComponent<Transform>().position;
                data_VR.markAreaList.Add(cursorPos);
            }
            else
            {
                data_VR.flag_markArea[0] = data_VR.flag_markArea[1];
                data_VR.flag_markArea[1] = false;
            }
        }

        //Clear trace line(right )
        private void ClearMarkAreaFunction()
        {
            leftController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryButtonValue);
            data_VR.flag_clearMarkArea[0] = data_VR.flag_clearMarkArea[1];
            data_VR.flag_clearMarkArea[1] = secondaryButtonValue;
            if (!data_VR.flag_clearMarkArea[0] && data_VR.flag_clearMarkArea[1])
            {
                data_VR.markAreaList.Clear();
            }
        }

        //API for VolumeRenderedObject
        public Vector3 MarkAreaAPI()
        {
            if (data_VR.flag_markArea[1])
            {
                return data_VR.markAreaList[data_VR.markAreaList.Count - 1];
            }
            else
            {
                if (!data_VR.flag_clearMarkArea[0] && data_VR.flag_clearMarkArea[1])
                {
                    //negativeInfinity is flag of area clearing
                    return Vector3.negativeInfinity;
                }
                else
                {
                    //postiveInfinity is flag of none drawing
                    return Vector3.positiveInfinity;
                }
            }
        }

        //保持相机位置无法跟随头运动
        public void CameraBlock()
        {
            leftController.TryGetFeatureValue(CommonUsages.primaryButton, out bool leftPrimaryButtonValue);
            if (leftPrimaryButtonValue)
            {
                data_VR.flag_cameraBlock[0] = data_VR.flag_cameraBlock[1];
                data_VR.flag_cameraBlock[1] = true;

                GameObject camera = GameObject.Find("OVRCameraRig");
                GameObject centereye = GameObject.Find("CenterEyeAnchor");

                if ((!data_VR.flag_cameraBlock[0]) && data_VR.flag_cameraBlock[1])
                {
                    data_VR.camera_blockCoord_world = centereye.GetComponent<Transform>().position;
                }

                camera.GetComponent<Transform>().position = data_VR.camera_blockCoord_world - centereye.GetComponent<Transform>().localPosition;
            }
            else
            {
                data_VR.flag_cameraBlock[0] = data_VR.flag_cameraBlock[1];
                data_VR.flag_cameraBlock[1] = false;
            }
        }

        //teleport function
        public void Teleport()
        {
            rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool rightSectondaryButtonValue);
            data_VR.flag_teleport[0] = data_VR.flag_teleport[1];
            data_VR.flag_teleport[1] = rightSectondaryButtonValue;
            if (!data_VR.flag_teleport[0] && data_VR.flag_teleport[1])
            {
                GameObject teleportCursor = GameObject.Find("RightControllerCursor");
                Vector3 teleportCoord_world = teleportCursor.GetComponent<Transform>().position;
                GameObject camera = GameObject.Find("OVRCameraRig");
                GameObject centereye = GameObject.Find("CenterEyeAnchor");
                camera.GetComponent<Transform>().position = teleportCoord_world - centereye.GetComponent<Transform>().localPosition;
            }

        }
    }

}
