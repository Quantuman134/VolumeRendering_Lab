using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace UnityVolumeRendering
{
    [ExecuteInEditMode]//可以在编辑模式下执行
    public class VolumeRenderedObject : MonoBehaviour//继承自monobehaviour可以用作组件
    {
        public int i = 0;
        public DateTime before;
        public DateTime after;

        [HideInInspector]//不在inspector显示该控件
        public TransferFunction transferFunction;

        [HideInInspector]
        public TransferFunction2D transferFunction2D;

        [HideInInspector]
        public VolumeDataset dataset;

        [HideInInspector]
        public MeshRenderer meshRenderer;

        private RenderMode renderMode;
        private TFRenderMode tfRenderMode;
        private bool lightingEnabled;

        private Vector2 visibilityWindow = new Vector2(0.0f, 1.0f);

        private Light[] lights;

        private Camera editorCamera;

        public SlicingPlane CreateSlicingPlane()
        {
            GameObject sliceRenderingPlane = GameObject.Instantiate(Resources.Load<GameObject>("SlicingPlane"));
            sliceRenderingPlane.transform.parent = transform;
            sliceRenderingPlane.transform.localPosition = Vector3.zero;
            sliceRenderingPlane.transform.localRotation = Quaternion.identity;
            sliceRenderingPlane.transform.localScale = Vector3.one * 0.1f; // TODO: Change the plane mesh instead and use Vector3.one
            MeshRenderer sliceMeshRend = sliceRenderingPlane.GetComponent<MeshRenderer>();
            sliceMeshRend.material = new Material(sliceMeshRend.sharedMaterial);
            Material sliceMat = sliceRenderingPlane.GetComponent<MeshRenderer>().sharedMaterial;
            sliceMat.SetTexture("_DataTex", dataset.GetDataTexture());
            sliceMat.SetTexture("_TFTex", transferFunction.GetTexture());
            sliceMat.SetMatrix("_parentInverseMat", transform.worldToLocalMatrix);
            sliceMat.SetMatrix("_planeMat", Matrix4x4.TRS(sliceRenderingPlane.transform.position, sliceRenderingPlane.transform.rotation, transform.lossyScale)); // TODO: allow changing scale

            return sliceRenderingPlane.GetComponent<SlicingPlane>();
        }

        public void SetRenderMode(RenderMode mode)
        {
            if(renderMode != mode)
            {
                renderMode = mode;
                SetVisibilityWindow(0.0f, 1.0f); // reset visibility window
            }
            UpdateMaterialProperties();
        }

        public void SetTransferFunctionMode(TFRenderMode mode)
        {
            tfRenderMode = mode;
            if (tfRenderMode == TFRenderMode.TF1D && transferFunction != null)
                transferFunction.GenerateTexture();
            else if(transferFunction2D != null)
                transferFunction2D.GenerateTexture();
            UpdateMaterialProperties();
        }

        public TFRenderMode GetTransferFunctionMode()
        {
            return tfRenderMode;
        }

        public RenderMode GetRenderMode()
        {
            return renderMode;
        }

        public bool GetLightingEnabled()
        {
            return lightingEnabled;
        }

        public void SetLightingEnabled(bool enable)
        {
            lightingEnabled = enable;
        }

        public void SetVisibilityWindow(float min, float max)
        {
            SetVisibilityWindow(new Vector2(min, max));
        }

        public void SetVisibilityWindow(Vector2 window)
        {
            visibilityWindow = window;
            UpdateMaterialProperties();
        }

        public Vector2 GetVisibilityWindow()
        {
            return visibilityWindow;
        }

        private void UpdateMaterialProperties()
        {

            if(tfRenderMode == TFRenderMode.TF2D)
            {
                meshRenderer.sharedMaterial.SetTexture("_TFTex", transferFunction2D.GetTexture());
                meshRenderer.sharedMaterial.EnableKeyword("TF2D_ON");
            }
            else
            {
                meshRenderer.sharedMaterial.SetTexture("_TFTex", transferFunction.GetTexture());
                meshRenderer.sharedMaterial.DisableKeyword("TF2D_ON");

            }

            if(lightingEnabled)
                meshRenderer.sharedMaterial.EnableKeyword("LIGHTING_ON");
            else
                meshRenderer.sharedMaterial.DisableKeyword("LIGHTING_ON");

            switch (renderMode)
            {
                case RenderMode.DirectVolumeRendering:
                    {
                        meshRenderer.sharedMaterial.EnableKeyword("MODE_DVR");
                        meshRenderer.sharedMaterial.DisableKeyword("MODE_MIP");
                        meshRenderer.sharedMaterial.DisableKeyword("MODE_SURF");
                        break;
                    }
                case RenderMode.MaximumIntensityProjectipon:
                    {
                        meshRenderer.sharedMaterial.DisableKeyword("MODE_DVR");
                        meshRenderer.sharedMaterial.EnableKeyword("MODE_MIP");
                        meshRenderer.sharedMaterial.DisableKeyword("MODE_SURF");
                        break;
                    }
                case RenderMode.IsosurfaceRendering:
                    {
                        meshRenderer.sharedMaterial.DisableKeyword("MODE_DVR");
                        meshRenderer.sharedMaterial.DisableKeyword("MODE_MIP");
                        meshRenderer.sharedMaterial.EnableKeyword("MODE_SURF");
                        break;
                    }
            }

            meshRenderer.sharedMaterial.SetFloat("_MinVal", visibilityWindow.x);
            meshRenderer.sharedMaterial.SetFloat("_MaxVal", visibilityWindow.y);
            meshRenderer.sharedMaterial.SetColor("_LightColor", lights[0].color.linear * lights[0].intensity);
        }

        //保证相机与缩放后的数据相对位置不变
        private Vector3 ViewScaleCameraTransform(Vector3 scale, Vector3 scaleNew, Vector3 cameraWorld, Vector3 dataWorld)
        {
            Vector3 cameraWorldNew = new Vector3();
            cameraWorldNew.x = (cameraWorld.x - dataWorld.x) / scale.x * scaleNew.x + dataWorld.x;
            cameraWorldNew.y = (cameraWorld.y - dataWorld.y) / scale.y * scaleNew.y + dataWorld.y;
            cameraWorldNew.z = (cameraWorld.z - dataWorld.z) / scale.z * scaleNew.z + dataWorld.z;
            return cameraWorldNew;
        }

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
            relCoord.x = (relCoord.x + modelScale.x / 2) * dataset.dimX / modelScale.x;
            relCoord.y = (relCoord.y + modelScale.y / 2) * dataset.dimY / modelScale.y;
            relCoord.z = (relCoord.z + modelScale.z / 2) * dataset.dimZ / modelScale.z;

            return relCoord;
        }

        //开始时会被调用
        private void Start()
        {
            Application.targetFrameRate = 60;

            lights = FindObjectsOfType<Light>();
            editorCamera = SceneView.lastActiveSceneView.camera;
            UpdateMaterialProperties();
        }

        private void Update()
        {
            UpdateMaterialProperties();
        }

        private void OnDestroy()
        {
        }
    }
}
