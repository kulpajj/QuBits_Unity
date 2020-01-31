using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QbitMovement : MonoBehaviour
{
    // inits:

    // editing tools for multiple prefabs:
    public bool debug = false;
    private GameObject selectedGo;

    // and then PLACE ALL THIS in a method:
    /*
        selectedGo = UnityEditor.Selection.activeGameObject;
        if( selectedGo != null)
        {
            if (selectedGo.name == this.transform.name)
            { debug = true; }
            else
            { debug = false; }
            if (debug == true)
            { Debug.Log(); }
        }
    */

    [HideInInspector]
    public MouseRaycast mouseRaycast;
    public RaycastHit qbitClosestHit;

    // id / self stuff
    public int self_id;
    public int ePartner_id = -1;
    public int qtype = 1;
    private float reportMovingVelocityThresh = 0.0f;
    private Vector3 self_position;
    private Rigidbody self_rigidbody;
    private SphereCollider self_collider;
   
    // mouse-click movement
    private Vector3 hitpoint;
    public Vector3 moveToPosition;
    private bool useSelfPhysics;
    public float distToClick;
    public float distToClickThresh;
    private float ePartner_distToClick;
    public float velocityMagnitude; 
    private Ray ray;
    private RaycastHit hit;
    private Vector3 position;
    private Vector3 positionPrev;
    public Vector3 velocity = new Vector3( 0.0f, 0.0f, 0.0f );
    public Vector3 ePartner_velocity = new Vector3( 0.0f, 0.0f, 0.0f );
    private float friction;
    private float forceScale;
    private float velocityToStopQtype1 = .0006f;
    private float stopQtype2StartTime;
    private float stopQtype2Timer;
    private float stopQtype2Phase;
    private bool lerping = false;
    private Vector3 velocityZero = new Vector3( 0.0f, 0.0f, 0.0f );
    // yellow 1., .8, .1, 1.  cyan .5, .9, .9, 1. purple .5, 0., .9, 1.
    private float eDurationTotal = 8.0f;
    private float eTimer;
    private Color non_eColor = new Color( .5f, 0.0f, .9f, 1.0f );
    private float non_eScale;
    private Color eColor = new Color( .5f, .9f, .9f, 1.0f );
    private float eScale = .4f;
    private float floorLength;
    private Vector3 floorCentroid;

    // jittery movement
    private RandomJitteryId randomJitteryId;
    private string grow_or_shrink = "grow";
    public Vector3 centerPosition;
    public float distFromJitteryCenter = 0.0f;
    private float distIncrement = 0.0f;
    public float distMax = 1.0f;
    public int jitteryVector;
    private float xTranslate;
    private float zTranslate;
    // currentSign is responsible for the illusion of the particle "splitting into two"
    // every other rendering, it translates/flips the position around the center point
    private int currentSign = 1;
    private int frameCount = 0;

    // voids 
    private GameObject qbitOther;
    private int[] nearestQbitsIds = new int[2];
    private float[] nearestQbitsDists = new float[2];
    public GameObject[] qbitsAll;
    private GameObject delaunayTriangulation;
    private List<VoidAllInfo> voidsAllInfo;
    private HashSet<Vector3> voidBoundingCoords;
    private float voidArea;
    private bool iAmABoundingQbit;
    public int voidId = -1;
    Vector3 voidCentroid;
    float voidAreaMax = 4.7f;
    bool  contracting = false;
    float contractingStepSize = .001f;

    //_______________________________________________________________________________________

    void Start()
    {
        GetSelfID();
        self_position = this.transform.position;
        moveToPosition = new Vector3( self_position.x, self_position.y, self_position.z );
        positionPrev = moveToPosition;
        qbitsAll = GameObject.FindGameObjectsWithTag( "qbit" );
        mouseRaycast = GameObject.Find( "mouseRaycast" ).GetComponent<MouseRaycast>();
        delaunayTriangulation = GameObject.Find("delaunayTriangulation");
        randomJitteryId = GameObject.Find("randomJitteryId").GetComponent<RandomJitteryId>();
        non_eScale = this.transform.localScale.x;
        self_rigidbody = this.GetComponent<Rigidbody>();
        self_collider  = this.GetComponent<SphereCollider>();
        floorLength = GameObject.Find("floor").GetComponent<Renderer>().bounds.size.x;
        floorCentroid = GameObject.Find("floor").GetComponent<Renderer>().bounds.center;
    }

    void Update() 
    {
        voidsAllInfo = delaunayTriangulation.GetComponent<DelaunayTriangulation>().voidsAllInfo;
            
        if( voidsAllInfo != null)
        {
            CheckIfIBoundAVoid();
        }

        ReportOsc();
    }

    void FixedUpdate()
    {
        self_position = this.transform.position;
        if(Mathf.Abs(self_position.x) > 6 || Mathf.Abs(self_position.z) > 6)
        {
            Debug.Log(self_id);
        }
        QbitsCheckJittery();

        if( iAmABoundingQbit == true )
        {
            QbitsBoundingAVoidHandling();
        }

        if( qtype == 0 )
        {
            QbitsJittery();
        }
        if( qtype == 1 )
        {
            if( contracting == false )
            {
                QbitsRepulsion();
            }
        }
        else if( qtype == 2 )
        {
            QbitsAttraction();
        }
    }

    private void OnCollisionEnter( Collision collision )
    {
        // qtype = 0: random jittery
        // qtype = 1: unentangled
        // qtype = 2: entangled
        if( collision.gameObject.tag == "qbit" && ePartner_id == -1 )
        {
            int other_ePartner_id = collision.gameObject.GetComponent<QbitMovement>().ePartner_id;
            bool other_iAmABoundingQbit = collision.gameObject.GetComponent<QbitMovement>().iAmABoundingQbit;
            if ( ( other_ePartner_id == -1 || other_ePartner_id == self_id ) && iAmABoundingQbit == false && other_iAmABoundingQbit == false )
            {
                ePartner_id = collision.gameObject.GetComponent<QbitMovement>().self_id;
                qtype = 2;
                eTimer = 0.0f;
                self_rigidbody.isKinematic = false;
                self_collider.enabled = true;
            }
        }
        if( qtype == 2 )
        {
            if( collision.gameObject.tag == "qbit")
            {
                Physics.IgnoreCollision(collision.gameObject.GetComponent<Collider>(), self_collider);
            }
        }
    }


    //_______________________________________________________________________________________


    void GetSelfID()
    {
        string[] splitName = new string[2];
        splitName = name.Split('_');
        string str_id = splitName[1];
        self_id = int.Parse(str_id);
    }

    void ReportOsc()
    {
        if ( velocityMagnitude > reportMovingVelocityThresh )
        {
            GameObject osc = GameObject.Find("osc");
            osc.GetComponent<OscOut>().Send( "/qbit/" + self_id + "/id", self_id );
            osc.GetComponent<OscOut>().Send( "/qbit/" + self_id + "/qtype", qtype );
            osc.GetComponent<OscOut>().Send( "/qbit/" + self_id + "/velocity", velocityMagnitude );
        }
    }

    void QbitsCheckJittery()
    {
        // this is how a prefab can access a value from a non-prefab GameObject
        int jitteryQbitId = randomJitteryId.jitteryQbitId;

        if( jitteryQbitId == self_id )
        {
            qtype = 0;
            centerPosition = new Vector3(self_position.x, self_position.y, self_position.z);
            grow_or_shrink = "grow";
            distMax = Random.Range(.1f, .5f);
            self_rigidbody.isKinematic = true;
            self_collider.enabled = false;

            jitteryVector = Random.Range(1, 5);
        }
    }

    void QbitsJittery()
    {
       // only render every N frames - 60 fps is way too fast for the desired look
       if ( frameCount % 4 == 0 )
       {
           switch ( jitteryVector )
           {
               case 1: xTranslate = currentSign * distFromJitteryCenter; zTranslate = currentSign * distFromJitteryCenter; break;
               case 2: xTranslate = currentSign * distFromJitteryCenter; zTranslate = -1 * currentSign * distFromJitteryCenter; break;
               case 3: xTranslate = currentSign * distFromJitteryCenter; zTranslate = 0; break;
               case 4: xTranslate = 0; zTranslate = currentSign * distFromJitteryCenter; break;
           }

           this.transform.position = new Vector3( centerPosition.x + xTranslate, centerPosition.y, centerPosition.z + zTranslate );

           // new dist from center
           // only change dist from center every N frames - so the phrase lasts longer
           if ( frameCount % 16 == 0 )
           {
               if ( grow_or_shrink == "grow" )
               {
                   distIncrement = Random.Range( .005f, .08f );
                   distFromJitteryCenter += distIncrement;
               }
               else
               {
                   // shrinking
                   if ( distFromJitteryCenter > .03 )
                   {
                       distIncrement = Random.Range( .01f, .03f );
                   }
                   else
                   {
                       distIncrement = Random.Range( .0005f, .0008f );
                   }
                   distFromJitteryCenter -= distIncrement;
               }
           }

           // flip sign so does jittery thing
           currentSign *= -1;

           if ( distFromJitteryCenter >= distMax )
           {
               grow_or_shrink = "shrink";
           }

            if ( grow_or_shrink == "shrink" && distFromJitteryCenter <= 0 )
            {
                qtype = 1;
                self_rigidbody.isKinematic = false;
                self_collider.enabled = true;
            }
       }

       frameCount++;
       velocityMagnitude = distFromJitteryCenter;
    }

    void QbitsRepulsion()
    {
        hitpoint = mouseRaycast.hitpoint;

        //___________________________
        // force
        //___________________________

        distToClick = Vector3.Distance(hitpoint, self_position);

        distToClickThresh = .6f;
        forceScale = .1f;
        friction = .01f;

        if (distToClick <= distToClickThresh)
        {
            float deltaX = hitpoint.x - self_position.x;
            float deltaZ = hitpoint.z - self_position.z;
            // displacement in each dimension:
            float normVectorX = deltaX / distToClick;
            float normVectorZ = deltaZ / distToClick;
            float force = forceScale * (1 / Mathf.Pow(distToClick, 2));
            if (force >= 1.0f)
            {
                force = 1.0f;
            }
            float accelerationX = normVectorX * force;
            float accelerationZ = normVectorZ * force;
            velocity.x = (velocity.x + accelerationX) / 60;
            velocity.z = (velocity.z + accelerationZ) / 60;
        }

        // the magnitude of the velocity vector is the same as the distance between 
        // the current position and the previous position
        velocityMagnitude = Vector3.Magnitude(velocity);

        // add friction and stop completely if still moving below magnitude thresh
        if( Mathf.Abs(velocity.x) > 0.0f || Mathf.Abs(velocity.z) > 0.0f )
        {
            velocity.x = velocity.x - friction * velocity.x;
            velocity.z = velocity.z - friction * velocity.z;
            if (velocityMagnitude <= velocityToStopQtype1)
            {
                velocity = velocityZero;
            }
        }

        if( Mathf.Abs( self_position.x - velocity.x ) >= floorCentroid.x + floorLength / 2 || Mathf.Abs( self_position.z - velocity.z ) >= floorCentroid.z + floorLength / 2 )
        {
            if(Mathf.Abs(self_position.x - velocity.x) >= floorCentroid.x + floorLength / 2)
            {
                self_position.x += velocity.x;
            }
            if (Mathf.Abs(self_position.z - velocity.z) >= floorCentroid.z + floorLength / 2)
            {
                self_position.z += velocity.z;
            }
            moveToPosition = new Vector3( self_position.x, self_position.y, self_position.z );
        }
        else
        {
            moveToPosition = new Vector3( self_position.x - velocity.x, self_position.y, self_position.z - velocity.z );
        }

        this.transform.position = moveToPosition;

        //why doesn't AddForce work?
        /*
        if (distTotal <= distThreshold)
        {
            this.GetComponent<Rigidbody>().AddForce(-accelerationX, 0.0f, -accelerationZ);
            Debug.Log(normVectorX + " " + normVectorZ);
        }
        */
    }

    void QbitsAttraction()
    {
        //___________________________
        // force
        //___________________________
        if( Input.GetMouseButton( 0 ) )
        {
            distToClickThresh = 1.0f;
            forceScale = .3f;
            friction = .6f;

            hitpoint = mouseRaycast.hitpoint;
            distToClick = Vector3.Distance(hitpoint, self_position);

            GameObject ePartnerObject = GameObject.Find("qbit_" + ePartner_id);
            ePartner_distToClick = ePartnerObject.GetComponent<QbitMovement>().distToClick;
            if (distToClick <= ePartner_distToClick)
            {
                useSelfPhysics = true;
            }
            else
            {
                useSelfPhysics = false;
                ePartner_velocity = ePartnerObject.GetComponent<QbitMovement>().velocity;
            }
            if (useSelfPhysics == true)
            {
                if (distToClick <= distToClickThresh)
                {
                    float deltaX = hitpoint.x - self_position.x;
                    float deltaZ = hitpoint.z - self_position.z;
                    // displacement in each dimension:
                    float normVectorX = deltaX / distToClick;
                    float normVectorZ = deltaZ / distToClick;
                    float force = forceScale * (1 / Mathf.Pow(distToClick, 2));
                    // with attractive gravity, force grows exponentially as it approaches the click pt, thus:
                    if( force >= 1.0f )
                    {
                        force = 1.0f;
                    }
                    float accelerationX = normVectorX * force;
                    float accelerationZ = normVectorZ * force;
                    velocity.x = (velocity.x + accelerationX) / 60;
                    velocity.z = (velocity.z + accelerationZ) / 60;
                }
            }
            else if (useSelfPhysics == false)
            {
                if (ePartner_distToClick <= distToClickThresh)
                {
                    velocity.x = -ePartner_velocity.x;
                    velocity.z = -ePartner_velocity.z;
                }
            }
        }
        // if no click, no force, just friction:
        else
        {
            friction = .01f;
        }

        // the magnitude of the velocity vector is the same as the distance between 
        // the current position and the previous position
        velocityMagnitude = Vector3.Magnitude(velocity);

        // add friction and stop completely if still moving below magnitude thresh
        if ( Mathf.Abs( velocityMagnitude ) > 0.0f )
        {
            velocity.x = velocity.x - friction * velocity.x;
            velocity.z = velocity.z - friction * velocity.z;
            if ( velocityMagnitude <= velocityToStopQtype1 )
            {
                velocity = velocityZero;
            }
        }

        moveToPosition = new Vector3(self_position.x + velocity.x, self_position.y, self_position.z + velocity.z );


        if ( eTimer < eDurationTotal )
        {
            eTimer += Time.deltaTime;
            float ePhase = eTimer / eDurationTotal;

            float currentScale = Mathf.Lerp( eScale, non_eScale, ePhase );
            this.transform.localScale = new Vector3( currentScale, currentScale, currentScale );
            this.GetComponent<Renderer>().material.color = Color.Lerp( eColor, non_eColor, ePhase );
        }
        else
        {
            ePartner_id = -1;
            qtype = 1;
            self_rigidbody.isKinematic = false;
            self_collider.enabled = true;
        }

        this.transform.position = moveToPosition;
    }

    void CheckIfIBoundAVoid()
    {
        iAmABoundingQbit = false;
        foreach( VoidAllInfo voidEntry in voidsAllInfo )
        {
            if( iAmABoundingQbit == true ){break;}
            foreach( Vector3 coord in voidEntry.boundingCoords )
            {
                iAmABoundingQbit = self_position == coord;
                
                if( iAmABoundingQbit == true )
                {
                    voidId = voidEntry.id;
                    voidCentroid = voidEntry.centroid;
                    voidArea = voidEntry.area;
                    break;
                }
            }
        }
    }

    void QbitsBoundingAVoidHandling()
    {
        if( voidArea >= voidAreaMax )
        {
            this.transform.position = Vector3.MoveTowards(self_position, voidCentroid, contractingStepSize);
            contracting = true;
        }
        else
        {
            contracting = false;
        }
    }

}