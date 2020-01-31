using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cameras : MonoBehaviour 
{
    public Transform player = null;
    public Transform target = null;

    public Vector3 interp_speed = new Vector3(4.0f, 2.0f, 1.0f);
    public Vector3 nextPosition = Vector3.zero;

    // there isn't a component CameraState in Unity...we create an enum CameraState and then case and switch 
    // what to do with each value
    public enum CameraState { none, followPosition, lookAtPlayer, both };
    public CameraState cameraState = CameraState.none;

    // LateUpdate() makes the camera move after the player moves - smoothed out moving of camera - not jittery
    void LateUpdate()
    {
        switch( cameraState )
        {
            case CameraState.none: break;
            case CameraState.followPosition: FollowPosition(); break;
            case CameraState.lookAtPlayer: LookAtPlayer(); break;
            case CameraState.both: FollowPosition(); LookAtPlayer(); break;
        }
    }

    void FollowPosition()
    {
        // follow exactly - tightly:
        // this.transform.position = target.position;

        //smoother follow with Lerp - linear interpolation:
        nextPosition.x = Mathf.Lerp(this.transform.position.x, target.position.x, interp_speed.x * Time.deltaTime);
        nextPosition.y = Mathf.Lerp(this.transform.position.y, target.position.y, interp_speed.y * Time.deltaTime);
        nextPosition.z = Mathf.Lerp(this.transform.position.z, target.position.z, interp_speed.z * Time.deltaTime);

        this.transform.position = nextPosition;
    }

    void LookAtPlayer()
    {
        this.transform.LookAt(player.position);
    }
}

