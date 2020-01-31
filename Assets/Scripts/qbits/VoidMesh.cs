using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Delaunay;
using Delaunay.Geo;

public class VoidMesh : MonoBehaviour 
{
    // debugging tool for multiple prefabs:
    private GameObject selectedGO;
    private GameObject[] selectedGOs;
    // keep this in the Update():
    /* selectedGO = UnityEditor.Selection.activeGameObject; */

    // and then use this anywhere to debug:
    /*
        if (selectedGO != null)
        {
            if (selectedGO.name == this.transform.name)
            { Debug.Log(""); }
        }
    */

    // OSC
    OscOut oscOutScript;
    OscIn  oscInScript;

    // Delaunay
    public DelaunayTriangulation delaunayScript;
    public List<BoundingQbit_AllInfo> boundingQbits_allInfo;
    public BoundingQbit_ConvexHullOrder_AllInfo[] boundingQbits_convexHullOrder_allInfo;
    public Vector2[] boundingQbits_convexHullOrder_positions;

    // mixer
    Mixer mixer;

    // ****
    // mixer values
    float   mx_static_oneClick_trailProb;
    Vector2 mx_static_oneClick_ampRange_trailDelay;
    Vector2 mx_static_oneClick_ampRange_sparse;
    Vector2 mx_static_oneClick_ampRange_tinyFlurry;
    float   mx_static_tinyFlurry_probability;
    float   mx_static_igniterEvent_ampGlobal;
    float   mx_edgeMoving_ampGlobal; //<-- todo delete
    float   mx_shakeIt_ampGlobal;

    // ****
    // additional values to audio/Max
    float         aud_static_ampGlobal;
    float         aud_static_delayAmp;
    bool          aud_static_oneClick_trail;
    public string aud_static_state = "sparse";
    private bool  aud_static_oneClick = true;
    public bool   aud_hitby_igniter;
    private bool  aud_voidOpen_click;
    private bool  aud_voidClose_click;
    bool          aud_shakeIt_displacingBegin;
    bool          aud_shakeIt_springingBegin; //<-- cue to begin orbiting
    bool          aud_shakeIt_springingEnd;
    bool          aud_edgeMoving_moving; //<-- todo delete

    // swarmParams
    SwarmParams swarmParams_script;
    bool    void_static_on;
    bool    upsideDownReveal_on;
    float   upsideDownReveal_amp;
    float   upsideDownReveal_meshY_maxHeight = .03f;
    Vector2 upsideDownReveal_meshY_randRange;

    // mouse input 
    // ShakeIt - mainCamera to mouse hitpoint
    bool         mouse_hitMe;
    bool         mouse_hitMePrev;
    MouseRaycast mouseRaycast_script;
    Vector3      hitpoint_camToMouse;
    Vector3      hitpoint_camToMouse_initialClickPt;
    Vector3      hitpoint_camToMouse_null = new Vector3( 15, 15, 15 );
    bool         shakeIt_displacing;
    bool         shakeIt_springing;
    Vector3      shakeIt_initialLocalPos;
    Vector3      localPosPrev;
    float        shakeIt_deltaPos_reportStopMoving = .01f;
    Vector2      hitpoint_camToMouse_deltaXZFromPrev;
    Vector3      hitpoint_camToMouse_prev;
    Vector3      shakeIt_displacing_newLocation;
    float        shakeIt_distanceFromInitialPos;
    float        shakeIt_distanceFromInitialPos_max = .3f;
    float        shakeIt_displacing_stepSize = .01f; //<--- for Vector3.movetowards() - slows it down so the mesh doesn't follow the mouse so immediately
    SpringJoint  springJoint;
    float        springjoint_spring = 10f;
        // doubleClick
    bool firstClick = false;
    bool timer_running;
    float timer_secondClick;
    float secondClick_maxTime = .4f;

    private List<Vector2> selectedGOs_XZ;
    int goPointTest;

    GameObject[] otherVoids;
    int otherVoidsSelfIdCounter;

    GameObject delaunayTriangulation;
    string parentName;
    public int self_id;

    GameObject[] qbitsAll;
    bool foundABoundingQbit;
    int foundBoundingQbitsCounter;
    bool firstMeshFrame;
    bool destroyMe;

    // ******************************************************************************
    // self_voidAllInfo.triangles and id are first set by the DelaunayTriangulation script on spawning this mesh;
    // this mesh then contributes self_VoidAllInfo.area and centroid;
    // the VoidsAllInfo script aggragates all the self_voidAllInfo methods from all meshes;
    // the Qbit movement scripts check in with the VoidAllInfo script to find out if they are a bounding qbit;
    // ******************************************************************************
    public Void_Cn self_voidAllInfo;
    public Void_Cn self_voidAllInfoPrev;
    bool self_still_exists;

    Mesh voidMesh;
    Rigidbody voidRigidbody;
    Material voidMaterial;
    public  float meshArea;
    private List<Vector2> newDelaunay_Vertices2D;
    private Vector3[]     voidMesh_Vertices3D;
    private int[] voidMesh_Triangles;
    private float sumX;
    private float sumZ;
    public Vector3 meshCentroid;
    public Vector3 meshScaledLocalPos;
    private float voidScale = .8f;

    public float floorWidth;
    public float floorHeight;
    private List<uint> colorsForDelaunay;
    private List<Triangle> trianglesFromNewDelaunay;
    private List<Vector2> convexHullPts_FromNewDelaunay;

    Color colorStart;
    Color colorEnd;
    Color colorClosed = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
    Color colorOpen   = new Color( 0.0f, 0.0f, 0.0f, 1.0f );

    private float trackingAreaThresh;
    private float xfadeAreaThresh;
    private float xfadeMaxAreaThresh;
    private float xfadeTime;
    private float xfadeStartTime;
    private float xfadeTotalTime = 2.5f;
    private float voidXfadePhase = -1;
    private bool startOpening = true;
    private bool isOpening;
    public bool  iAmOpen;
    private bool startClosing;
    private bool isClosing;

    // static
    public GameObject voidStaticPrefab;
    float static_oneClick_timeUntilNext;
    float static_oneClick_startTime;
    float static_oneClick_deltaTime;
    float static_oneClick_trailRandomValue;
    float static_oneClick_trailDelayOnTime;
    bool  static_tinyFlurry_selectRandom = true;
    float static_tinyFlurry_randomValue;
    float static_tinyFlurry_startTime;
    float static_tinyFlurry_duration;
    float static_tinyFlurry_deltaTime;
    float static_scaleOneClick = .065f;
    float static_scaleIgniterEvent = .02f;
    public Void_Cn.HitByVoidIgniterType hitby_voidIgniterType;

    // edge moving

    // debug
    int testNumLargest;
    int testNumSmaller;

    void Awake()
    {
        voidMesh = this.GetComponent<MeshFilter>().mesh;
        voidMaterial = this.GetComponent<Renderer>().material;
        voidMaterial.color = colorClosed;
    }

    void Start()
    {
        // with a spawned GO... 
        // instantiate frame = the GO does nothing
        // frame 2 = Start(), not Update() yet
        // frame 3 = the first Update()
        floorWidth  = GameObject.Find( "floor" ).GetComponent<Renderer>().bounds.size.x;
        floorHeight = GameObject.Find( "floor" ).GetComponent<Renderer>().bounds.size.z;

        delaunayScript = GameObject.Find( "delaunayTriangulation" ).GetComponent<DelaunayTriangulation>();
        boundingQbits_allInfo = self_voidAllInfo.boundingQbits_allInfo;
        self_id = self_voidAllInfo.id;

        mouseRaycast_script = GameObject.Find( "mouseRaycast" ).GetComponent<MouseRaycast>();

        trackingAreaThresh = delaunayScript.voidTrackingAreaThresh;
        xfadeAreaThresh = delaunayScript.voidXfadeAreaThresh;

        // these interestingly have to be here in the Start() or the DelaunayTriangulation script
        // won't be able to check the centroid of this mesh until the 3rd frame after instantiation, 
        // and you'd get two GOs spawned per void centroid
        meshCentroid = self_voidAllInfo.centroid;
        meshArea = self_voidAllInfo.area;

        firstMeshFrame = true;

        oscOutScript = GameObject.Find( "osc" ).GetComponent<OscOut>();
        oscInScript  = GameObject.Find( "osc" ).GetComponent<OscIn>();
        oscInScript.MapInt( "/void/" + ( self_id + 1 ) + "/igniterEvent/click", OscIn_IgniterClick );
        oscInScript.MapInt( "/void/" + ( self_id + 1 ) + "/igniterEvent/end", OscIn_IgniterEnd );

        swarmParams_script = GameObject.Find( "swarmParams" ).GetComponent<SwarmParams>();

        springJoint = GetComponent<SpringJoint>();
        voidRigidbody = GetComponent<Rigidbody>();

        mixer = new Mixer();
        MixerValues_Init();
        localPosPrev = transform.position;
    }

    void Update() 
    {
        if( aud_shakeIt_springingBegin == true) { aud_shakeIt_springingBegin = false; }
        selectedGO = UnityEditor.Selection.activeGameObject;
        hitpoint_camToMouse = mouseRaycast_script.hitpoint;

        Input_DoubleClick();

        // *****************************
        // new Delaunay of only the void points...because our mesh needs to be made out of triangles anyway
        newDelaunay_Vertices2D = new List<Vector2>();
        colorsForDelaunay = new List<uint>();
        sumX = 0;
        sumZ = 0;

        for( int i = 0; i < boundingQbits_allInfo.Count; i++ )
        {
            // stuff for new delaunay...
            Vector3 positionForDelaunay;

            if( boundingQbits_allInfo[i].qbitMovementScript.qtype == 1 )
            {
                positionForDelaunay = boundingQbits_allInfo[i].transform.position;
            }
            else
            {
                positionForDelaunay = boundingQbits_allInfo[i].qbitMovementScript.jittery_centerPosition;
            }

            newDelaunay_Vertices2D.Add(new Vector2(positionForDelaunay.x, positionForDelaunay.z));
            sumX += positionForDelaunay.x;
            sumZ += positionForDelaunay.z;

            colorsForDelaunay.Add( 0 );
        }

        Delaunay.Voronoi voronoi = new Delaunay.Voronoi( newDelaunay_Vertices2D, colorsForDelaunay, new Rect( 0, 0, floorWidth, floorHeight ) );
        trianglesFromNewDelaunay = voronoi.Triangles();
        convexHullPts_FromNewDelaunay = voronoi.HullPointsInOrder();
        meshArea = 0;

        voidMesh_Vertices3D = new Vector3[ trianglesFromNewDelaunay.Count * 3 ];
        voidMesh_Triangles = new int[ trianglesFromNewDelaunay.Count * 3 ];

        if( swarmParams_script.void_upsideDownReveal_on == true && iAmOpen == true )
        {
            // prepare randRange for Y values for voidMesh_Vertices3D generated below
            upsideDownReveal_amp = swarmParams_script.void_upsideDownReveal_ampLocal;
                // the max allowed Y height is scaled from the current upsideDownReveal_amp
            upsideDownReveal_meshY_randRange = new Vector2( 0f, Scale( upsideDownReveal_amp, 0f, 1f, 0f, upsideDownReveal_meshY_maxHeight ) );
        }
        else
        {
                // 0 Y height, cuz not upsideDownReveal 
            upsideDownReveal_meshY_randRange = Vector2.one * 0f;
        }


        for( int t = 0; t < trianglesFromNewDelaunay.Count; t++ )
        {
            int index0 =  t * 3;
            int index1 = (t * 3) + 1;
            int index2 = (t * 3) + 2;

            voidMesh_Triangles[index0] = index0;
            voidMesh_Triangles[index1] = index1;
            voidMesh_Triangles[index2] = index2;

            float[] y_values = new float[3];
            int indexRand = Random.Range(0, 3);
            float randValue = Random.Range(upsideDownReveal_meshY_randRange[0], upsideDownReveal_meshY_randRange[1]);
            for (int i = 0; i < y_values.Length; i++)
            {
                if (i == indexRand)
                {
                    y_values[i] = randValue;
                }
                else
                {
                    y_values[i] = 0;
                }
            }

            voidMesh_Vertices3D[index0] = new Vector3( trianglesFromNewDelaunay[t].sites[0].Coord.x, y_values[0], trianglesFromNewDelaunay[t].sites[0].Coord.y );
            voidMesh_Vertices3D[index1] = new Vector3( trianglesFromNewDelaunay[t].sites[1].Coord.x, y_values[1], trianglesFromNewDelaunay[t].sites[1].Coord.y );
            voidMesh_Vertices3D[index2] = new Vector3( trianglesFromNewDelaunay[t].sites[2].Coord.x, y_values[2], trianglesFromNewDelaunay[t].sites[2].Coord.y );

            meshArea += delaunayScript.TriangleArea( trianglesFromNewDelaunay[t] );
        }

        // *****************************
        // opening and closing the void...

        // destroy me if below area thresh
        if( meshArea < trackingAreaThresh )
        {
            AllPolysOff();
            Destroy( gameObject );
            // Destroy() finishes excecuting the Update() loop, which leads to the polys turning back on - boo
            // so we create a bool here telling the ReportOSC() to not execute this frame
            destroyMe = true;
        }

        // opening...
        if( meshArea >= xfadeAreaThresh && startOpening == true )
        {
            startOpening = false;
            isClosing = false;
            isOpening = true;
            voidXfadePhase = 0;
            xfadeStartTime = Time.time;

            colorStart = colorClosed;
            colorEnd = colorOpen;
        }

        // closing...
        if( meshArea < xfadeAreaThresh && isOpening == false && startOpening == false )
        {
            startClosing = true;
        }

        if( startClosing == true )
        {
            isClosing = true;
            startClosing = false;
            startOpening = true;
            voidXfadePhase = 0;
            xfadeStartTime = Time.time;

            colorStart = colorOpen;
            colorEnd = colorClosed;
        }

        // xfading for opening and closing...
        if( voidXfadePhase != -1 )
        {
            if( isOpening == true )
            {
                voidXfadePhase = (Time.time - xfadeStartTime) / xfadeTotalTime;
                voidMaterial.color = Color.Lerp(colorStart, colorEnd, voidXfadePhase);
                if( voidXfadePhase >= 1.0f )
                {
                    iAmOpen = true;
                    aud_voidOpen_click = true;
                    isOpening = false;
                    voidXfadePhase = -1;
                }
            }
            else if( isClosing == true )
            {
                voidXfadePhase = ( Time.time - xfadeStartTime ) / xfadeTotalTime;
                voidMaterial.color = Color.Lerp(colorStart, colorEnd, voidXfadePhase);
                if( voidXfadePhase >= 1.0f )
                {
                    iAmOpen = false;
                    isClosing = false;
                    aud_voidClose_click = true;
                    // EdgeMovingPolyOff(); <-- todo delete
                    ShakeItPolyOff();
                }
            }
        }

        // *****************************
        // draw mesh

        // draw the mesh...
        voidMesh.Clear();

        voidMesh.vertices = voidMesh_Vertices3D;
        voidMesh.triangles = voidMesh_Triangles;
        voidMesh.RecalculateNormals();

        meshCentroid = new Vector3( sumX / boundingQbits_allInfo.Count, 0, sumZ / boundingQbits_allInfo.Count);

        // ScaleAround( this.gameObject, meshCentroid, new Vector3( voidScale, voidScale, voidScale) );

        // *****************************
        // static, shakeIt
        if( ( isOpening == true || iAmOpen == true ) && isClosing == false )
        {
            Input_ShakeIt();
            // EdgeMoving();
        }

        if( iAmOpen == true && isClosing == false )
        {
            VoidStatic();
        }

        // *****************************
        // prepare data for Qbits, etc

        // this method includes concave detection:
        Build_ConvexHullOrder_AllInfo();
        // we also separately maintain an array of all the positions because the Method in QbitMovement.cs that checks for
        // qbits inside voids needs a Vector2[] - and I did not write that method:
        boundingQbits_convexHullOrder_positions = new Vector2[ convexHullPts_FromNewDelaunay.Count ];
        convexHullPts_FromNewDelaunay.CopyTo( boundingQbits_convexHullOrder_positions );

        // to self_voidAllInfo contribute info...
        // area, centroid, and convexHullOrder_allInfo, igniterEvent, shakeIt etc
        self_voidAllInfo.area = meshArea;
        self_voidAllInfo.centroid = meshCentroid;
        self_voidAllInfo.isOpening = isOpening;
        self_voidAllInfo.isOpen = iAmOpen;
        self_voidAllInfo.boundingQbits_convexHullOrder_positions = boundingQbits_convexHullOrder_positions;
        self_voidAllInfo.boundingQbits_convexHullOrder_allInfo = boundingQbits_convexHullOrder_allInfo;
        self_voidAllInfo.hitby_igniter = aud_hitby_igniter;
        self_voidAllInfo.hitby_voidIgniterType = hitby_voidIgniterType;
        self_voidAllInfo.shakeIt_displacingBegin = aud_shakeIt_displacingBegin;
        self_voidAllInfo.shakeIt_springingBegin = aud_shakeIt_springingBegin;

        // *****************************
        // osc
        if( destroyMe == false )
        {
            ReportOsc();
        }

        /*
        if (selectedGO != null)
        {
            if (selectedGO.name == this.transform.name)
            { Debug.Log("allInfo " + ( self_voidAllInfo == null ) + " prev " + ( self_voidAllInfoPrev == null)); }
        }*/

        // *****************************
        // prev, setbacks
        self_voidAllInfoPrev = self_voidAllInfo.DeepCopy();
        mouse_hitMePrev = mouse_hitMe;
        localPosPrev = transform.position;
        aud_static_oneClick = false;
    }

    void Build_ConvexHullOrder_AllInfo()
    {
        boundingQbits_convexHullOrder_allInfo = new BoundingQbit_ConvexHullOrder_AllInfo[ convexHullPts_FromNewDelaunay.Count ];

        // positions and ids
        for( int c = 0; c < convexHullPts_FromNewDelaunay.Count; c++ )
        {
            foreach( BoundingQbit_AllInfo boundingQbit_allInfo in boundingQbits_allInfo )
            {
                int qtype = boundingQbit_allInfo.qbitMovementScript.qtype;
                Vector2 boundingQbit_allInfo_position = new Vector2();

                if( qtype == 1 )
                {
                    boundingQbit_allInfo_position = new Vector2( boundingQbit_allInfo.transform.position.x, boundingQbit_allInfo.transform.position.z );
                }
                else if( qtype == 0 )
                {
                    boundingQbit_allInfo_position = new Vector2( boundingQbit_allInfo.qbitMovementScript.jittery_centerPosition.x, boundingQbit_allInfo.qbitMovementScript.jittery_centerPosition.z );
                }

                if( convexHullPts_FromNewDelaunay[ c ] == boundingQbit_allInfo_position )
                {
                    BoundingQbit_ConvexHullOrder_AllInfo newEntry = new BoundingQbit_ConvexHullOrder_AllInfo { position = boundingQbit_allInfo_position, id = boundingQbit_allInfo.id };

                    boundingQbits_convexHullOrder_allInfo[c] = newEntry;
                    break;
                }
            }
        }

        /*
        if( boundingQbits_convexHullOrder_allInfo.Length > 2 )
        {*/
            // detect bounding Qbits approaching concave
            for( int q = 0; q < boundingQbits_convexHullOrder_allInfo.Length; q++ )
            {
                try
                {
                    Vector2 ptA;
                    Vector2 ptB;
                    Vector2 ptC;
                    if (q == 0)
                    {
                        ptA = boundingQbits_convexHullOrder_allInfo[boundingQbits_convexHullOrder_allInfo.Length - 1].position;
                        ptB = boundingQbits_convexHullOrder_allInfo[q].position;
                        ptC = boundingQbits_convexHullOrder_allInfo[q + 1].position;
                    }
                    else if (q == boundingQbits_convexHullOrder_allInfo.Length - 1)
                    {
                        ptA = boundingQbits_convexHullOrder_allInfo[q - 1].position;
                        ptB = boundingQbits_convexHullOrder_allInfo[q].position;
                        ptC = boundingQbits_convexHullOrder_allInfo[0].position;
                    }
                    else
                    {
                        ptA = boundingQbits_convexHullOrder_allInfo[q - 1].position;
                        ptB = boundingQbits_convexHullOrder_allInfo[q].position;
                        ptC = boundingQbits_convexHullOrder_allInfo[q + 1].position;
                    }
                    Vector2 vectorAB = new Vector2(ptB.x - ptA.x, ptB.y - ptA.y);
                    Vector2 vectorBC = new Vector2(ptC.x - ptB.x, ptC.y - ptB.y);
                    float dotProductAB_BC = vectorAB.x * vectorBC.x + vectorAB.y * vectorBC.y;
                    float distAB = Vector2.Distance(ptA, ptB);
                    float distBC = Vector2.Distance(ptB, ptC);

                    float angleRadians = Mathf.Acos(dotProductAB_BC / (distAB * distBC));

                    float angleDegrees = angleRadians * (180 / Mathf.PI);

                    boundingQbits_convexHullOrder_allInfo[q].iAmFormingThisVertexAngle = angleDegrees;

                    if (angleDegrees <= 10)
                    {
                        boundingQbits_convexHullOrder_allInfo[q].expandToMakeConvex = true;
                    }
                    else
                    {
                        boundingQbits_convexHullOrder_allInfo[q].expandToMakeConvex = false;
                    }
                }
                catch( System.NullReferenceException exception )
                {
                    Debug.Log( "convexHullOrder_allInfo.length " + boundingQbits_convexHullOrder_allInfo.Length );
                }
            }
        //}
         
    }

    //******************************
    // OSC out
    void ReportOsc()
    {
        // in the voids DSP, /id comes after the specific /void/polyname/id; this is because there are independent 
        // polys per independent void sound, and wanted to mute the processing on each of these independently
        // and that's how the routing works then
        if( aud_voidOpen_click == true )
        {
            oscOutScript.Send( "/void/static/" + ( self_id + 1 ) + "/on",                     aud_voidOpen_click );
            aud_voidOpen_click = false;
        }
        else if( aud_voidClose_click == true )
        {
            // mute all of this void's polys; also, this same method is called in Destroy() because user may destroy GO before it is faded out 
            // and before we'd get the voidClose_click
            AllPolysOff();
            aud_voidClose_click = false;
        }
        // else open still and doing shit
        else
        {
            /*
            oscOutScript.Send( "/void/edgeMoving/" + ( self_id + 1 ) + "/moving",             aud_edgeMoving_moving );
            if( aud_edgeMoving_moving == true )
            {
                oscOutScript.Send( "/void/edgeMoving/" + ( self_id + 1 ) + "/amp/global",     mx_edgeMoving_ampGlobal );
            }*/

            if( aud_shakeIt_displacingBegin == true )
            {
                oscOutScript.Send( "/void/shakeIt/" + ( self_id + 1 ) + "/on",                1 );
                oscOutScript.Send( "/void/shakeIt/" + ( self_id + 1 ) + "/amp/global",        mx_shakeIt_ampGlobal );
                aud_shakeIt_displacingBegin = false;
            }
            else if( aud_shakeIt_springingEnd == true )
            {
                oscOutScript.Send( "/void/shakeIt/" + ( self_id + 1 ) + "/off",               1 );
                aud_shakeIt_springingEnd = false;
            }

            if ( aud_static_oneClick == true )
            {
                oscOutScript.Send( "/void/static/" + ( self_id + 1 ) + "/state",              aud_static_state );
                oscOutScript.Send( "/void/static/" + ( self_id + 1 ) + "/oneClickEvent",      aud_static_oneClick );
                oscOutScript.Send( "/void/static/" + ( self_id + 1 ) + "/amp/global",         aud_static_ampGlobal );
                oscOutScript.Send( "/void/static/" + ( self_id + 1 ) + "/trail",              aud_static_oneClick_trail );
                if( aud_static_oneClick_trail == true )
                {
                    oscOutScript.Send( "/void/static/" + ( self_id + 1 ) + "/amp/delay",      aud_static_delayAmp );
                }

                if( aud_static_oneClick_trail == true ) { aud_static_oneClick_trail = false; }
            }

            if( aud_hitby_igniter == true )
            {
                // the voidIgniter GO tells me, the mesh, if i was hit by it - that is the beginning of the voidStatic phrase in max; 
                // the end of that event end comes from max reporting, when its done performing its phrase - see OscIn method below
                oscOutScript.Send( "/void/static/" + ( self_id + 1 ) + "/igniterEvent/begin", aud_hitby_igniter );
                oscOutScript.Send( "/void/static/" + ( self_id + 1 ) + "/amp/global",         aud_static_ampGlobal );
                // Debug.Log( "amp " + aud_static_ampGlobal + " state " + aud_static_state );
                aud_hitby_igniter = false;
            }
        }
    }

    void ShakeItPolyOff()
    {
        // turn the associated shakeIt poly off so not stuck playing when the void is closed but GO still there
        oscOutScript.Send( "/void/shakeIt/" + ( self_id + 1 ) + "/off", 1 );
    }

    // delete:
    void EdgeMovingPolyOff()
    {
        // turn the associated edgeMoving poly off so not stuck playing when the void is closed but GO still there
        oscOutScript.Send( "/void/edgeMoving/" + ( self_id + 1 ) + "/moving", 0 );
    }

    void AllPolysOff()
    {
        // turn all the associated polys off so they aren't stuck on playing when the void is destroyed!
        oscOutScript.Send( "/void/static/"  + ( self_id + 1 ) + "/off", aud_voidClose_click );
        // oscOutScript.Send( "/void/edgeMoving/" + ( self_id + 1 ) + "/moving", 0 );
        oscOutScript.Send( "/void/shakeIt/" + ( self_id + 1 ) + "/off", 1);
        oscOutScript.Send( "/void/geyser/"  + ( self_id + 1 ) + "/off", aud_voidClose_click );
    }

    //******************************
    // OscIn
    void OscIn_IgniterClick( int bang )
    {
        // instantiate i - 1 number of statics per click from max
        for( int i = 0; i < 4; i++ )
        {
            InstantiateVoidStatic( aud_static_state );
        }
    }

    void OscIn_IgniterEnd( int bang )
    {
        aud_static_state = "sparse";
    }

    //******************************
    // static
    void VoidStatic()
    {
        void_static_on = swarmParams_script.void_static_on;

        // Debug.Log(voidStatic_on);

        if( void_static_on == true )
        {
            if( aud_static_state != "igniter event" )
            {
                static_oneClick_deltaTime = Time.time - static_oneClick_startTime;
                if( static_oneClick_deltaTime >= static_oneClick_timeUntilNext )
                {
                    aud_static_oneClick = true;
                }

                if( aud_static_oneClick == true ) // <--- set back to false after ReportOSC()
                {
                    if( aud_static_state == "sparse" )
                    {
                        static_oneClick_timeUntilNext = Random.Range( .008f, 1.5f );
                        static_oneClick_startTime = Time.time;
                        aud_static_ampGlobal = Random.Range( mx_static_oneClick_ampRange_sparse[0], mx_static_oneClick_ampRange_sparse[1] );
                    }
                    else if( aud_static_state == "tiny flurry" )
                    {
                        static_oneClick_timeUntilNext = Random.Range( .005f, .11f );
                        static_oneClick_startTime = Time.time;
                        aud_static_ampGlobal = Random.Range( mx_static_oneClick_ampRange_tinyFlurry[0], mx_static_oneClick_ampRange_tinyFlurry[1] );
                    }

                    InstantiateVoidStatic( aud_static_state );
                }

                if( static_tinyFlurry_selectRandom == true )
                {
                    static_tinyFlurry_randomValue = Random.Range( 0f, 1f );

                    if( static_tinyFlurry_randomValue <= mx_static_tinyFlurry_probability )
                    {
                        aud_static_state = "tiny flurry";
                        static_tinyFlurry_startTime = Time.time;
                        static_tinyFlurry_duration = Random.Range( .4f, 1.3f );
                        static_tinyFlurry_selectRandom = false;
                    }
                }

                if( aud_static_state == "tiny flurry" )
                {
                    static_tinyFlurry_deltaTime = Time.time - static_tinyFlurry_startTime;
                    if( static_tinyFlurry_deltaTime >= static_tinyFlurry_duration )
                    {
                        aud_static_state = "sparse";
                        aud_static_oneClick = true;
                        static_tinyFlurry_selectRandom = true;
                    }
                }
            }
        }

        // the voidIgniter gameobject script that hits me is what sets my voidStatic_state as "igniter event" and igniterEvent_begin = true
        // begin comes from the voidIgniter GO; end comes from max when its done performing its phrase
        if( aud_static_state == "igniter event" )
        {
            // during this state, Max sends data to the OscIn_ methods below concerning the Igniter Event
            // also:
            aud_static_ampGlobal = mx_static_igniterEvent_ampGlobal;
            // Debug.Log( "igniter " + aud_static_ampGlobal );
        }
    }

    void InstantiateVoidStatic( string voidStatic_state )
    {
        float scale;

        // select a random vertex, get equation of line between it and void centroid; 
        // then get a random pt on that line segment
        int randomVertex_index = Random.Range( 0, boundingQbits_convexHullOrder_allInfo.Length - 1 );

        Vector2 randomVertex_position = boundingQbits_convexHullOrder_allInfo[ randomVertex_index ].position;

        float m = ( randomVertex_position.y - meshCentroid.z ) / ( randomVertex_position.x - meshCentroid.x );
        float b = meshCentroid.z - m * meshCentroid.x;

        float randomXOnLineSegment = Random.Range( randomVertex_position.x, meshCentroid.x );
        Vector3 randomPtOnLineSegment = new Vector3( randomXOnLineSegment, 0, m * randomXOnLineSegment + b );
        randomPtOnLineSegment = Vector3.MoveTowards( randomPtOnLineSegment, meshCentroid, 1 - voidScale );

        GameObject staticGO = Instantiate( voidStaticPrefab, randomPtOnLineSegment, Quaternion.identity );
        staticGO.transform.SetParent( this.transform );

        if( voidStatic_state == "igniter event" )
        {
            scale = .02f;
        }
        else
        {
            scale = .07f;

            static_oneClick_trailRandomValue = Random.Range( 0.0f, 1.0f );
            if( static_oneClick_trailRandomValue < mx_static_oneClick_trailProb )
            {
                aud_static_oneClick_trail = true;
                staticGO.GetComponent<TrailRenderer>().enabled = true;
                static_oneClick_trailDelayOnTime = Random.Range( 1.0f, 2.5f );
                aud_static_delayAmp = Random.Range( mx_static_oneClick_ampRange_trailDelay[0], mx_static_oneClick_ampRange_trailDelay[1] );
            }
        }

        staticGO.transform.localScale = new Vector3( scale, scale, scale );
    }

    //******************************
    // edge moving
    void EdgeMoving()
    {
        if( self_voidAllInfoPrev != null )
        {
            aud_edgeMoving_moving = false;
            float moving_thresh = .0001f;
            foreach( BoundingQbit_ConvexHullOrder_AllInfo qbit in self_voidAllInfo.boundingQbits_convexHullOrder_allInfo )
            {
                if( aud_edgeMoving_moving == true ){ break; }
                foreach(BoundingQbit_ConvexHullOrder_AllInfo qbitPrev in self_voidAllInfoPrev.boundingQbits_convexHullOrder_allInfo )
                {
                    if( qbit.id == qbitPrev.id )
                    {
                        // TODO ||, not && ?
                        if( Mathf.Abs( qbit.position.x - qbitPrev.position.x ) >= moving_thresh && Mathf.Abs( qbit.position.y - qbitPrev.position.y ) >= moving_thresh )
                        {
                            aud_edgeMoving_moving = true;
                            break;
                        }
                    }
                }
            }
        }
    }

    //******************************
    // etc
    public void ScaleAround( GameObject target, Vector3 pivot, Vector3 newScale )
    {
        Vector3 A = target.transform.localPosition;
        Vector3 B = pivot;

        Vector3 C = A - B; // diff from object pivot to desired pivot/origin

        float RS = newScale.x / target.transform.localScale.x; // relataive scale factor

        // calc final position post-scale
        meshScaledLocalPos = B + C * RS;

        // finally, actually perform the scale/translation
        target.transform.localScale = newScale;
        target.transform.localPosition = meshScaledLocalPos;
    }

    //*****************************
    // mixer
    void MixerValues_Init()
    {
        mx_static_oneClick_trailProb           = mixer.void_static_oneClick_trailProb;
        mx_static_oneClick_ampRange_trailDelay = mixer.void_static_oneClick_ampRange_trailDelay;
        mx_static_oneClick_ampRange_sparse     = mixer.void_static_oneClick_ampRange_sparse;
        mx_static_oneClick_ampRange_tinyFlurry = mixer.void_static_oneClick_ampRange_tinyFlurry;
        mx_static_tinyFlurry_probability       = mixer.void_static_tinyFlurry_probability;
        mx_static_igniterEvent_ampGlobal       = mixer.void_static_igniterEvent_ampGlobal;
        mx_shakeIt_ampGlobal                   = mixer.void_shakeIt_ampGlobal;
        mx_edgeMoving_ampGlobal                = mixer.void_edgeMoving_ampGlobal; //<-- todo delete
    }

    //*****************************
    // input

    void Input_ShakeIt()
    {
        // for "shaking" the void - moving the entire mesh and setting off its spring joint

        mouse_hitMe = CheckIfMouseHitInsideMe( boundingQbits_convexHullOrder_positions, new Vector2( hitpoint_camToMouse.x, hitpoint_camToMouse.z ) );

        // initial click inside void:
        if( mouse_hitMePrev == false && mouse_hitMe == true )
        {
            hitpoint_camToMouse_initialClickPt = hitpoint_camToMouse;
            shakeIt_displacing = true;
            aud_shakeIt_displacingBegin = true; // <--- turn ShakeIt poly on
            shakeIt_initialLocalPos = transform.position;
            hitpoint_camToMouse_prev = hitpoint_camToMouse;
        }
        else if( shakeIt_displacing == true && hitpoint_camToMouse != hitpoint_camToMouse_null )
        {
            // displacement
            // 1) once displacement started, voidMesh just follows the mouse delta from prev frame
            // 2) BUT we restrict its displacement to a circle with radius distanceMax centered around transform.position
            // https://answers.unity.com/questions/1309521/how-to-keep-an-object-within-a-circlesphere-radius.html
            hitpoint_camToMouse_deltaXZFromPrev = new Vector2( hitpoint_camToMouse.x - hitpoint_camToMouse_prev.x, hitpoint_camToMouse.z - hitpoint_camToMouse_prev.z );
            shakeIt_displacing_newLocation = new Vector3( transform.position.x + hitpoint_camToMouse_deltaXZFromPrev.x, transform.position.y, transform.position.z + hitpoint_camToMouse_deltaXZFromPrev.y );
            shakeIt_distanceFromInitialPos = Vector3.Distance( shakeIt_displacing_newLocation, shakeIt_initialLocalPos ); //distance from ~green object~ to *black circle*

            if( shakeIt_distanceFromInitialPos > shakeIt_distanceFromInitialPos_max ) //If the distance is less than the radius, it is already within the circle.
            {
                Vector3 fromOriginToObject = shakeIt_displacing_newLocation - shakeIt_initialLocalPos; // newPos - voidLocalPos
                fromOriginToObject *= shakeIt_distanceFromInitialPos_max / shakeIt_distanceFromInitialPos; // Multiply by radius // Divide by Distance
                shakeIt_displacing_newLocation = shakeIt_initialLocalPos + fromOriginToObject; // voidLocalPos + all that Math
            }

            transform.position = Vector3.MoveTowards( transform.position, shakeIt_displacing_newLocation, shakeIt_displacing_stepSize );
            // transform.position = shakeIt_displacing_newLocation;

            hitpoint_camToMouse_prev = hitpoint_camToMouse;
            springJoint.spring = 0;
        }

        if( shakeIt_springing == true )
        {
            springJoint.spring = springjoint_spring;
            if( ( transform.position.x - localPosPrev.x <= .001f ) && ( transform.position.z - localPosPrev.z <= .001f ) )
            {
                aud_shakeIt_springingEnd = true;
                shakeIt_springing = false;
            }
        }

        // springing back - handled by SpringJoint
        if( Input.GetMouseButtonUp( 0 ) )
        {
            springJoint.spring = springjoint_spring;
            // without giving it a lil shove on mouse up, it sometimes gets stuck and doesn't spring back
            voidRigidbody.AddForce( .0001f, 0, .0001f );
            shakeIt_displacing = false;
            shakeIt_springing = true;
            aud_shakeIt_springingBegin = true; // <-- orbiting begin
        }
        if( aud_shakeIt_springingBegin == true )
        {
            // Debug.Log("orbit");
        }
    }

    void Input_DoubleClick()
    {
        //this is how long in seconds to allow for a double click
        if( Input.GetMouseButtonDown( 0 ) )
        {
            if( !firstClick ) // first click no previous clicks
            {
                firstClick = true;
                timer_secondClick = Time.time;
            }
            else
            {
                firstClick = false; // found a double click, now reset
                // Debug.Log( "double clicky" );
            }
        }
        if( firstClick )
        {
            // if the time now is gredater than secondClick_maxTime,
            if( ( Time.time - timer_secondClick ) > secondClick_maxTime )
            {
                // ...its been too long and we want to reset so the next click is simply a single click and not a double click
                firstClick = false;
            }
        }

    }

    public static bool CheckIfMouseHitInsideMe( Vector2[] voidPts, Vector2 qbitPosition )
    {
        /// http://wiki.unity3d.com/index.php/PolyContainsPoint
        /// returns true if the point is inside the polygon; otherwise, false
        bool inside = false;
        int j = voidPts.Length - 1;
        for (int i = 0; i < voidPts.Length; j = i++)
        {
            if (((voidPts[i].y <= qbitPosition.y && qbitPosition.y < voidPts[j].y) || (voidPts[j].y <= qbitPosition.y && qbitPosition.y < voidPts[i].y)) &&
              (qbitPosition.x < (voidPts[j].x - voidPts[i].x) * (qbitPosition.y - voidPts[i].y) / (voidPts[j].y - voidPts[i].y) + voidPts[i].x))
            {
                inside = !inside;
            }
        }
        return inside;
    }

    public float Scale( float oldValue, float oldMin, float oldMax, float newMin, float newMax )
    {

        float oldRange = oldMax - oldMin;
        float newRange = newMax - newMin;
        float newValue = (((oldValue - oldMin) * newRange) / oldRange) + newMin;

        return newValue;
    }

    void OnDrawGizmos()
    { 
        // to visualize voids, uncomment this, not in the delaunay script, as once a void is born from the delaunay, it is independent from the delaunay
       
        /*
        if( boundingQbits_convexHullOrder_allInfo != null )
        {
            Gizmos.color = Color.cyan;
            List<Vector3> convexToDraw = new List<Vector3>();

            foreach(BoundingQbit_ConvexHullOrder_AllInfo qbit in boundingQbits_convexHullOrder_allInfo)
            {
                convexToDraw.Add( new Vector3( qbit.position.x, 0, qbit.position.y ) );
            }
            int otherIndex;

            for (int i = 0; i < convexToDraw.Count; i++)
            {
                if(i < convexToDraw.Count - 1)
                {
                    otherIndex = i + 1;
                }
                else
                {
                    otherIndex = 0;
                }
                Gizmos.DrawLine(convexToDraw[i], convexToDraw[otherIndex]);
            }
        }*/

        if(trianglesFromNewDelaunay != null)
        {
            Gizmos.color = Color.magenta;
            for (int i = 0; i < trianglesFromNewDelaunay.Count; i++)
            {
                Vector2 point0 = trianglesFromNewDelaunay[i].sites[0].Coord;
                Vector2 point1 = trianglesFromNewDelaunay[i].sites[1].Coord;
                Vector2 point2 = trianglesFromNewDelaunay[i].sites[2].Coord;

                //rotate so the analysis is now in the X and Z dimensions:
                Vector3 pt0 = new Vector3(point0.x, 0.1f, point0.y);
                Vector3 pt1 = new Vector3(point1.x, 0.1f, point1.y);
                Vector3 pt2 = new Vector3(point2.x, 0.1f, point2.y);
                Gizmos.DrawLine(pt0, pt1);
                Gizmos.DrawLine(pt1, pt2);
                Gizmos.DrawLine(pt2, pt0);
            }
        }
    }
}
