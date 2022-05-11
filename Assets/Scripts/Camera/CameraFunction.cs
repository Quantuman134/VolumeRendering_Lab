using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CameraFunction : MonoBehaviour
{
    Camera editorCamera;
    Material material;

    [SerializeField]
    public bool editorCameraTrackingEnable = true;
    // Start is called before the first frame update
    void Start()
    {
        editorCamera = SceneView.lastActiveSceneView.camera;
        //material = Resources.Load<Material>("Assets/Materials/DirectVolumeRenderingMaterial");
    }

    // Update is called once per frame
    void Update()
    {
        TrackEditorCamera();
        //material.SetFloat("_TestVal", 0.5f);
        //Debug.Log("editorCamera forward: " + editorCamera.transform.forward);
        //Debug.Log("Camera forward: " + this.gameObject.GetComponent<Transform>().forward);
        
    }

    void TrackEditorCamera()
    {
        if (editorCameraTrackingEnable)
        {
            this.gameObject.transform.position = editorCamera.transform.position;
            this.gameObject.transform.rotation = editorCamera.transform.rotation;
        }
    }
}
