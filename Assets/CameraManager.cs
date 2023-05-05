using System;
using System.Collections;
using System.Collections.Generic;

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class CameraManager : MonoBehaviour
{

    public float MaxZoom = 10;
    public float MinZoom = 0.1f;
    private Camera camera;
    private Transform cameraFollow;
    [SerializeField]private Transform playerFollowPoint;
    private Vector3 mousePos;
    private Vector2 zoomLevel;

    private float t;
    
    
    private void Start()
    {
        //GetCamera();
        
    }

    private void Update()
    {
        
        if (!camera) GetCamera();

        ManageCameraRotation(Input.GetMouseButton(1));
        ManageCameraZoom(Input.mouseScrollDelta);
        
        mousePos = Input.mousePosition;
        zoomLevel = Input.mouseScrollDelta;
        
        HandleCameraFollow();
    }


    private void HandleCameraFollow()
    {
        //cameraFollow.position = transform.position;
        //cameraFollow.position = Vector3.Lerp(transform.position, camera.transform.position, 0.05f);
        var smoothSpeed = 0.05f;
        var smoothPosition = Vector3.Lerp(cameraFollow.position, playerFollowPoint.position , smoothSpeed);
        cameraFollow.position = smoothPosition;
 
        /*// Lerp toward target rotation at all times.
        var smoothRotation = Quaternion.Lerp(cameraFollow.rotation, transform.rotation, smoothSpeed);
        cameraFollow.rotation = smoothRotation;*/
        
    }
    private void GetCamera()
    {
        //camera = FindObjectOfType<Camera>();
        camera = FindObjectOfType<Camera>();
        cameraFollow = camera.transform.parent;
        playerFollowPoint = transform;
        //transform.FindRecusiveChild("Spine_03");
        //camera.GetComponentInParent<CameraFollow>().objToFollow = transform;
    }
    private void ManageCameraRotation(bool getMouseButton)
    {
        if (getMouseButton)
        {
            camera.transform.RotateAround(cameraFollow.position, Vector3.up,
                Input.mousePosition.x - mousePos.x);
            camera.transform.RotateAround(cameraFollow.position, camera.transform.right,
                mousePos.y - Input.mousePosition.y);
        }
    }

    private void ManageCameraZoom(Vector2 mouseScrollDelta)
    {
        if (mouseScrollDelta.magnitude < 0.1)
        {
            return;
        }

        //var oldDistanceToObj = camera.GetComponentInParent<CameraFollow>().DistanceToObj;
        var oldDistanceToObj = Vector3.Distance(camera.transform.position, transform.position);
        var newDistanceToObj = oldDistanceToObj;
        newDistanceToObj += mouseScrollDelta.y;
        if (newDistanceToObj < MinZoom)
        {
            newDistanceToObj = MinZoom;
        }

        if (newDistanceToObj > MaxZoom)
        {
            newDistanceToObj = MaxZoom;
        }

        Vector3 newPosition = camera.transform.localPosition;
        newPosition *= newDistanceToObj / oldDistanceToObj;
        camera.transform.localPosition = newPosition;
    }
}
