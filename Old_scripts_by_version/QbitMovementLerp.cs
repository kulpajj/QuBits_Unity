using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QbitMovementLerp : MonoBehaviour 
{

    // id
    public int self_id = 0;

    // init mouse-click movement
    //public Camera ray_camera;
    private Vector3 hitpoint; 
    public Vector3 moveToPosition;
    public float force = 1.0f;
    public float distThreshold = .5f;
    private float movingThresh = .02f;
    private Ray ray;
    private RaycastHit hit;
    private Vector3 position;
    private Vector3 positionPrev;

    //init jittery movement
    // public GameObject randomJitterySc = null;
    public int jittering = 0;
    private string grow_or_shrink = "grow";
    public Vector3 centerPosition;
    public float distFromCenter = 0.0f;
    private float distIncrement = 0.0f;
    public float distMax = 1.0f;
    public int jitteryVector;
    private float xTranslate;
    private float zTranslate;
    // currentSign is responsible for the illusion of the particle "splitting into two"
    // every other rendering, it translates/flips the position around the center point
    private int currentSign = 1;
    private int frameCount = 0;

    // attempt at ArrayList and bundling
    // public ArrayList bundleList = new ArrayList();

    void Start()
    {
        GetSelfID();
        moveToPosition = new Vector3( this.transform.position.x, this.transform.position.y, this.transform.position.z );
        positionPrev = moveToPosition;
    }

    void FixedUpdate()
    {
        JitteryMovement();
        MouseyMovement();
    }



    // other Methods()
    void GetSelfID()
    {
        string[] splitName = new string[2];
        splitName = name.Split('_');
        string str_id = splitName[1];
        self_id = int.Parse( str_id );
    }

    void JitteryMovement()
    {
        // this is how a prefab can access a value from a non-prefab GameObject
        int jitteryQbitId = GameObject.Find("randomJitteryId").GetComponent<RandomJitteryId>().jitteryQbitId; 

        if ( jitteryQbitId == self_id )
        {
            jittering = 1;
            centerPosition = new Vector3( this.transform.position.x, this.transform.position.y, this.transform.position.z );
            grow_or_shrink = "grow";
            distMax = Random.Range( .1f, .5f );

            jitteryVector = Random.Range( 1, 5 );
        }

        if( jittering == 1 )
        {
            //only render every N frames - 60 fps is way too fast for the desired look
            if ( frameCount % 4 == 0 )
            {
                switch ( jitteryVector )
                {
                    case 1: xTranslate = currentSign * distFromCenter; zTranslate = currentSign * distFromCenter; break;
                    case 2: xTranslate = currentSign * distFromCenter; zTranslate = -1 * currentSign * distFromCenter; break;
                    case 3: xTranslate = currentSign * distFromCenter; zTranslate = 0; break;
                    case 4: xTranslate = 0; zTranslate = currentSign * distFromCenter; break;

                }

                this.transform.position = new Vector3( centerPosition.x + xTranslate, centerPosition.y, centerPosition.z + zTranslate );

                // new dist from center
                // only change dist from center every N frames - so the phrase lasts longer
                if( frameCount % 16 == 0)
                {
                    if (grow_or_shrink == "grow")
                    {
                        distIncrement = Random.Range(.005f, .08f);
                        distFromCenter += distIncrement;
                    }
                    else
                    {
                        // shrinking
                        if (distFromCenter > .03)
                        {
                            distIncrement = Random.Range(.01f, .03f);
                        }
                        else
                        {
                            distIncrement = Random.Range(.0005f, .0008f);
                        }
                        distFromCenter -= distIncrement;
                    }

                }

                // flip sign so does jittery thing
                currentSign *= -1;

                if ( distFromCenter >= distMax )
                {
                    grow_or_shrink = "shrink";
                }

                if ( grow_or_shrink == "shrink" && distFromCenter <= 0 )
                {
                   jittering = 0;
                }
            }

            frameCount++;

            // OSC
            /*
            GameObject osc = GameObject.Find( "osc" );
            OscMessage oscMsg = new OscMessage();
            OscMessage oscMsg2 = new OscMessage();
            oscMsg.address = "/" + self_id + "/jittery/dist_from_center";
            oscMsg.values.Add( distFromCenter );

            osc.GetComponent<OSC>().Send( oscMsg );
            */

            /*
            // attempt at ArrayList and bundling
            oscMsg2.address = "/" + self_id + "/something";
            oscMsg2.values.Add(5);

            bundleList.Add(oscMsg);
            bundleList.Add(oscMsg2);

            osc.GetComponent<OSC>().Send( bundleList );
            */
        }
    } 

    void MouseyMovement()
    {
        if ( jittering == 0 )
        {
            hitpoint = GameObject.Find("mouseRaycast").GetComponent<MouseRaycast>().hitpoint;

            float distTotal = Vector3.Distance(hitpoint, this.transform.position);
            float deltaX = hitpoint.x - this.transform.position.x;
            float deltaZ = hitpoint.z - this.transform.position.z;
            float normVectorX = deltaX / distTotal;
            float normVectorZ = deltaZ / distTotal;
            position = this.transform.position;
            float displacementX = positionPrev.x - position.x;
            float displacementaZ = positionPrev.z - position.z;
            // float speed = this.GetComponent<Rigidbody>().velocity.magnitude; <-----doesn't work
            float speed = Vector3.Distance( position, positionPrev ) / Time.deltaTime;


            // new position 
            if (distTotal <= distThreshold)
            {
                moveToPosition = new Vector3(this.transform.position.x - normVectorX, this.transform.position.y, this.transform.position.z - normVectorZ);
            }
            this.transform.position = Vector3.Lerp( this.transform.position, moveToPosition, force * Time.deltaTime );


            /*
            if (distTotal <= distThreshold)
            {
                this.GetComponent<Rigidbody>().AddForce(-normVectorX, 0.0f, -normVectorZ);
            }
            */

            /*
            // report OSC
            if ( speed > movingThresh )
            {
                GameObject osc = GameObject.Find( "osc" );
                OscMessage oscMsg = new OscMessage();
                oscMsg.address = "/" + self_id + "/mousey/velocity";
                oscMsg.values.Add( speed );
                osc.GetComponent<OSC>().Send( oscMsg );
            }
            */

            //prev
            positionPrev = position;
        }

    }

}
