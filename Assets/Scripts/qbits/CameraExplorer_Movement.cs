using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraExplorer_Movement : MonoBehaviour
{
    public Vector3 center = Vector3.zero;
    Vector3 initPosition;
    public float zoomDist;
    private float zoomSpeed;

    public float zoomSpeedModifier = .7f;

    GameObject cameraParentGO;

    void Start() 
    {
        zoomDist = 0;
        zoomSpeed = 0f;
        initPosition = this.transform.position;
    }

    void Update() 
    {
        //use mouse scroll wheel for zoom
        zoomSpeed += Input.GetAxis("Mouse ScrollWheel") * 500 * zoomSpeedModifier;
        //slow down all the speed
        zoomSpeed *= 1 - Time.deltaTime * 4;

        //calculate the distance from initPos based on zoomSpeed
        zoomDist += Time.deltaTime * zoomSpeed * 0.01f;
        // we always translate from the initPosition with this method
        transform.position = initPosition;
        //apply the zoom distance
        transform.position = transform.TransformPoint( new Vector3( 0, 0, zoomDist ) );
    }
}
