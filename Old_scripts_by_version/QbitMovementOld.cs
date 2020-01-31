using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QbitMovementOld : MonoBehaviour
{
    // id
    public int self_id;
    public int ePartner_id = -1;
    private int qtype = 1;
    private float reportMovingVelocityThresh = 0.0f;

    // init mouse-click movement
    //public Camera ray_camera;
    private Vector3 hitpoint;
    public Vector3 moveToPosition;
    private bool useSelfPhysics;
    public float distToClick;
    public float distToClickThresh;
    private float ePartner_distToClick;
    private float attractive_StopForceDist = .005f;
    public float velocityMagnitude;
    private Ray ray;
    private RaycastHit hit;
    private Vector3 position;
    private Vector3 positionPrev;
    public Vector3 velocity = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 ePartner_velocity = new Vector3(0.0f, 0.0f, 0.0f);
    private float friction;
    private float forceScale;
    private float velocityToStopQtype1 = .0006f;
    private float stopQtype2StartTime;
    private float stopQtype2Timer;
    private float stopQtype2Phase;
    private Vector3 velocityZero = new Vector3(0.0f, 0.0f, 0.0f);
    // yellow 1., .8, .1, 1.  cyan .5, .9, .9, 1. purple .5, 0., .9, 1.
    private float eDurationTotal = 8.0f;
    private float eTimer;
    private Color non_eColor = new Color(.5f, 0.0f, .9f, 1.0f);
    private float non_eScale;
    private Color eColor = new Color(.5f, .9f, .9f, 1.0f);
    private float eScale = .3f;


    //init jittery movement
    // public GameObject randomJitterySc = null;
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

    //_______________________________________________________________________________________

    void Awake()
    {
        non_eScale = this.transform.localScale.x;
    }

    void Start()
    {
        GetSelfID();
        moveToPosition = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
        positionPrev = moveToPosition;
    }

    void Update()
    {
        ReportOsc();
    }

    void FixedUpdate()
    {
        JitteryMovement();
        MouseyMovement();
        //ReportOsc();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "qbit" && ePartner_id == -1)
        {
            int other_ePartner_id = collision.gameObject.GetComponent<QbitMovement>().ePartner_id;
            if (other_ePartner_id == -1 || other_ePartner_id == self_id)
            {
                ePartner_id = collision.gameObject.GetComponent<QbitMovement>().self_id;
                qtype = 2;
                eTimer = 0.0f;
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
        if (velocityMagnitude > reportMovingVelocityThresh)
        {
            GameObject osc = GameObject.Find("osc");
            osc.GetComponent<OscOut>().Send("/qbit/" + self_id + "/id", self_id);
            osc.GetComponent<OscOut>().Send("/qbit/" + self_id + "/qtype", qtype);
            osc.GetComponent<OscOut>().Send("/qbit/" + self_id + "/velocity", velocityMagnitude);
        }
    }

    void JitteryMovement()
    {
        // this is how a prefab can access a value from a non-prefab GameObject
        int jitteryQbitId = GameObject.Find("randomJitteryId").GetComponent<RandomJitteryId>().jitteryQbitId;

        if (jitteryQbitId == self_id)
        {
            qtype = 0;
            centerPosition = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
            grow_or_shrink = "grow";
            distMax = Random.Range(.1f, .5f);

            jitteryVector = Random.Range(1, 5);
        }

        if (qtype == 0)
        {
            // only render every N frames - 60 fps is way too fast for the desired look
            if (frameCount % 4 == 0)
            {
                switch (jitteryVector)
                {
                    case 1: xTranslate = currentSign * distFromCenter; zTranslate = currentSign * distFromCenter; break;
                    case 2: xTranslate = currentSign * distFromCenter; zTranslate = -1 * currentSign * distFromCenter; break;
                    case 3: xTranslate = currentSign * distFromCenter; zTranslate = 0; break;
                    case 4: xTranslate = 0; zTranslate = currentSign * distFromCenter; break;

                }

                this.transform.position = new Vector3(centerPosition.x + xTranslate, centerPosition.y, centerPosition.z + zTranslate);

                // new dist from center
                // only change dist from center every N frames - so the phrase lasts longer
                if (frameCount % 16 == 0)
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

                if (distFromCenter >= distMax)
                {
                    grow_or_shrink = "shrink";
                }

                if (grow_or_shrink == "shrink" && distFromCenter <= 0)
                {
                    qtype = 1;
                }
            }

            frameCount++;
            velocityMagnitude = distFromCenter;
        }
    }

    void MouseyMovement()
    {
        if (qtype != 0)
        {
            hitpoint = GameObject.Find("mouseRaycast").GetComponent<MouseRaycast>().hitpoint;

            //___________________________
            // forces
            //___________________________

            distToClick = Vector3.Distance(hitpoint, this.transform.position);

            if (qtype == 1)
            {
                distToClickThresh = .6f;
                forceScale = .1f;
                friction = .01f;

                useSelfPhysics = true;
            }
            else if (qtype == 2)
            {
                distToClickThresh = 1.0f;
                forceScale = .3f;
                friction = .6f;

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
            }

            if (useSelfPhysics == true)
            {
                if (distToClick <= distToClickThresh)
                {
                    float deltaX = hitpoint.x - this.transform.position.x;
                    float deltaZ = hitpoint.z - this.transform.position.z;
                    // displacement in each dimension:
                    float normVectorX = deltaX / distToClick;
                    float normVectorZ = deltaZ / distToClick;
                    float force = forceScale * (1 / Mathf.Pow(distToClick, 2));
                    // if attractive gravity, force would grow exponentially as it approaches the click pt, thus:
                    if (force >= 1.0f && distToClick > attractive_StopForceDist)
                    {
                        force = 1.0f;
                    }
                    else if (force >= 1.0f && distToClick < attractive_StopForceDist)
                    {
                        force = 0.0f;
                    }
                    float accelerationX = normVectorX * force;
                    float accelerationZ = normVectorZ * force;
                    velocity.x = (velocity.x + accelerationX) / 60;
                    velocity.z = (velocity.z + accelerationZ) / 60;
                }
            }
            else
            {
                if (ePartner_distToClick <= distToClickThresh)
                {
                    velocity.x = -ePartner_velocity.x;
                    velocity.z = -ePartner_velocity.z;
                }
            }

            // the magnitude of the velocity vector is the same as the distance between 
            // the current position and the previous position
            velocityMagnitude = Vector3.Magnitude(velocity);

            // add friction and stop completely if still moving below magnitude thresh
            if (Mathf.Abs(velocity.x) > 0.0f || Mathf.Abs(velocity.z) > 0.0f)
            {
                velocity.x = velocity.x - friction * velocity.x;
                velocity.z = velocity.z - friction * velocity.z;
                if (velocityMagnitude <= velocityToStopQtype1)
                {
                    velocity = velocityZero;
                }
            }


            //___________________________
            // stuff that varies by whether entangled
            //___________________________
            if (qtype == 1)
            {
                moveToPosition = new Vector3(this.transform.position.x - velocity.x, this.transform.position.y, this.transform.position.z - velocity.z);
            }
            else if (qtype == 2)
            {
                if (Input.GetMouseButton(0))
                {
                    moveToPosition = new Vector3(this.transform.position.x + velocity.x, this.transform.position.y, this.transform.position.z + velocity.z);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    stopQtype2StartTime = Time.time;
                    stopQtype2Phase = 0;
                }
                else
                {
                    stopQtype2Timer = Time.time - stopQtype2StartTime;
                    stopQtype2Phase = stopQtype2Timer / 1.0f;
                    if (stopQtype2Phase <= 1.0f)
                    {
                        velocity = Vector3.Lerp(velocity, velocityZero, stopQtype2Phase);
                        velocityMagnitude = Vector3.Magnitude(velocity);
                        if (self_id == 11)
                        {
                            Debug.Log("phase " + stopQtype2Phase + " velocity " + velocity + " mag " + velocityMagnitude);

                        }
                    }
                }

                if (eTimer < eDurationTotal)
                {
                    eTimer += Time.deltaTime;
                    float ePhase = eTimer / eDurationTotal;

                    float currentScale = Mathf.Lerp(eScale, non_eScale, ePhase);
                    this.transform.localScale = new Vector3(currentScale, currentScale, currentScale);
                    this.GetComponent<Renderer>().material.color = Color.Lerp(eColor, non_eColor, ePhase);
                }
                else
                {
                    ePartner_id = -1;
                    qtype = 1;
                }
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

    }
}
