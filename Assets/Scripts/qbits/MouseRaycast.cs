using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseRaycast : MonoBehaviour
{
    // init mouse-click movement
    public Camera ray_camera;
    public Vector3 moveToPosition;
    public float force = 1.0f;
    public float distThreshold = .5f;
    private Ray ray;
    // private RaycastHit hit;
    private RaycastHit[] hits;
    public Vector3 hitpoint;

    // troubleshooting

    void Start()
    {
        // send fake click somewhere arbitrarily or the "null' click will be interpretted in the 
        // QbitMovement script as a click at Vector3( 0, 0, 0 ) on load.
        hitpoint = new Vector3( 15, 15, 15 );
    }

    void Update()
    {
        if( Input.GetMouseButton( 0 ) )
        {
            ray = ray_camera.ScreenPointToRay( Input.mousePosition );
            hits = Physics.RaycastAll( ray );
            foreach( RaycastHit hit in hits )
            {
                if( hit.collider.name == "floor" )
                {
                    hitpoint = hit.point;
                    break;
                }
            }
        }
        else
        {
            // if no mouseclick, don't let the previous click remain in effect - throw the hitpoint out of the worldspace
            hitpoint = new Vector3( 15, 15, 15 );
        }

        if (Input.GetKey(KeyCode.M))
        {
            Debug.Log( "mouseSC hit " + hitpoint );
        }
    }

}
