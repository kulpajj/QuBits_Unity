using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class QbitMovement : MonoBehaviour
{
    // inits:

    // debugging tool for multiple prefabs:
    private GameObject   selectedGO;
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

    // JSON
    JSONNode sampleInfo_JSONParsed;

    // global evolution
    GlobalEvolution globalEvolution_script;
    GlobalEvolution.GlobalEvolutionState globalEvolutionState;
    GlobalEvolution.GlobalEvolutionState globalEvolutionStatePrev;
    SwarmOrbitingEvents swarmOrbitingEvents_script;

    // mixer
    Mixer mixer;

    // global params script
    SwarmParams swarmParams_script;

    // ****
    // mixer values
    float     mx_velocitymagAsAmplitude_multiplier;
    string    mx_qtype0_bufferNamePrefix;
    float     mx_qtype0_bufferPlayPos;
    float     mx_qtype0_ampGlobal;
    float     mx_qtype0_transposition;
    string    mx_qtype1Repel_bufferName;
    float     mx_qtype1Repel_ampGlobal;
    float     mx_qtype1Brownian_ampGlobal_begin;
    float     mx_qtype1Brownian_ampGlobal_lowerAndRougher;
    string    mx_qtype2_bufferNamePrefix;
    float     mx_qtype2_bufferPlayPos;
    float     mx_qtype2_ampGlobal;
    float     mx_qtype2_transposition;
    int       mx_frozen_sameBufferCountMax;
    string    mx_frozen_bufferPrefix_woodDark;
    string    mx_frozen_bufferPrefix_tubesLow;
    Vector2   mx_frozen_bufferTranspositionRange_woodDark;
    Vector2   mx_frozen_bufferTranspositionRange_tubesLow;
    float[]   mx_frozen_localAmpLouds_begin;
    float[]   mx_frozen_localAmpQuiets;
    List<int> mx_frozen_localAmpLouds_weightedProbs_beginSpin;
    float     mx_frozen_ampGlobal_begin;
    float     mx_frozen_ampGlobal_lowerAndRougher;
    float     mx_filament_YscaleMax_begin;
    float     mx_filament_YscaleMax_tallerTubes;
    float     mx_filament_YscaleMax_beginSpin;
    string    mx_orbit_transition_bufferPrefix;
    float     mx_orbit_transToOrbiting_ampGlobal_begin;
    float     mx_orbit_transToOrbiting_ampGlobal_lowerAndRougher;
    List<int> mx_orbit_transToOrbiting_probShortMidLong;
    List<int> mx_orbit_transToOrbiting_sameTubeCountMaxRange_begin;
    List<int> mx_orbit_transToOrbiting_sameTubeCountMaxRange_beginCeiling;
    List<int> mx_orbit_transToOrbiting_sameTubeCountMaxRange_tallerTubes;
    int       mx_orbit_frozen_and_transToOrbiting_xfadeDur;
    Vector2   mx_orbit_userLetGoTooSoon_xfadeToFrozen_phaseRange;
    float     mx_orbit_orbiting_ampGlobal_begin;
    float     mx_orbit_orbiting_ampGlobal_lowerAndRougher;
    string    mx_orbit_orbiting_bufferPrefix;
    List<int> mx_orbit_orbiting_probShortLong;
    Vector2   mx_orbit_orbiting_durRangeShort;
    Vector2   mx_orbit_orbiting_durRangeLong;
    Vector2   mx_orbit_orbiting_transpLocal_destRange;
    Vector2   mx_orbit_orbiting_transpLocal_durRange;
    float     mx_orbit_transToFrozen_ampGlobal;
    Vector2   mx_orbit_transitionXfadeOut_proportionOfTotalOrbitingDurRange;
    Vector2   mx_orbit_orbitingXfadeOut_proportionOfTotalOrbitingDurRange;

    // ****
    // delegates - variable globalEvolution Methods()

    // ****
    // additional values to audio/Max
    // we want code that 1) sets these generic vars somewhere in the methods; and 2) in the oscReport() sends the values of these vars
    bool          aud_startedMoving;
    bool          aud_stoppedMoving;
    float         aud_ampGlobal;
    float         aud_qtype1Brownian_ampGlobal;
    float         aud_frozen_ampGlobal;
    float         aud_orbit_orbiting_ampGlobal;
    float         aud_orbit_transToOrbiting_ampGlobal;
    // this script just alternates the val between 0 and 1 when need a new sound: 
    // this val along with an xfadeDur are sent to the 2 xfading groove~s: go to 0. in xfadeDur, or go to 1. in xfadeDur
    int           aud_samplePlayer_xfade = 0;
    float         aud_samplePlayer_xfadeDur;
    bool          aud_beginQtype3; //<-- for audio xfade
    bool              beginQtype3; //<-- for physics and coding here, including the coroutine needed
        // these shoulda been used for everything, but only started making generic vars with the frozen behaviors:
    public float  aud_ampLocal;
    public string aud_bufferName;
    public float  aud_transpGlobal;
    float         aud_transpLocal;
    string        aud_collName;
    float         aud_bufferPlayPos;
    float             bufferDuration;

    [HideInInspector]
    public MouseRaycast mouseRaycast;

    // id / self stuff
    public int self_id;
    public int ePartner_id = -1;
    public int qtype = 1;
    private int qtypePrev = 1;
    private Vector3 self_position;
    private Vector3 self_positionPrev;
    private Rigidbody self_rigidbody;
    private SphereCollider self_collider;
    private Renderer self_renderer;
    GameObject   filament_current_go;
    MeshRenderer filament_current_renderer;
    GameObject   filament1_go;
    MeshRenderer filament1_renderer;
    GameObject   filament2_go;
    MeshRenderer filament2_renderer;
    GameObject   filament3_go;
    MeshRenderer filament3_renderer;
    GameObject   filament4_go;
    MeshRenderer filament4_renderer;
    GameObject   filament5_go;
    MeshRenderer filament5_renderer;
    TrailRenderer orbiting_trail;

    // mouse-click movement
    private Vector3 hitpoint;
    public Vector3 moveToPosition;
    public bool useSelfPhysics;
    public float distToClick;
    public float distToClickThresh;
    private float ePartner_distToClick;
    bool repelling;
        // allows us to distinguish between brownian just stopped and mousclick movement just stopped
    bool repellingMovementJustStopped;
    private Ray ray;
    private RaycastHit hit;
    public float velocityMagnitude;
    float        velocityMagnitudePrev;
    public Vector2 velocity2D;
    public Vector3 velocity = new Vector3( 0.0f, 0.0f, 0.0f );
    public Vector2 acceleration2D;
    GameObject ePartnerObject;
    public Vector2 ePartner_velocity2D = new Vector2( 0.0f, 0.0f );
    private float friction;
    private float forceScale;
    private float velocityToStopQtype1 = .0006f;
    private float velocityToStopQtype2 = .0006f;
    private float stopQtype2StartTime;
    private float stopQtype2Timer;
    private float stopQtype2Phase;
    private bool  lerping;
    private float floorLength;
    private Vector3 floorCentroid;
    private Vector3 velocityZero = new Vector3( 0.0f, 0.0f, 0.0f );
        // yellow 1., .8, .1, 1.  cyan .5, .9, .9, 1. purple .5, 0., .9, 1.
    private float eDurationTotal = 8.0f;
    private float eTimer;
    private float qtype0_scaleMax = .2f;
    private float qtype1_alphaFaded = .65f;
    private Color qtype1_color = new Color( .5f, 0.0f, .9f, 1.0f );
    private float qtype1_scale;
    private Color qtype2_color = new Color( .5f, .9f, .9f, 1.0f );
    private float qtype2_scale = .4f;
    private Color qtype3_colorFrozen = new Color( .2f, .2f, .2f, 1.0f );
    private Color qtype3_colorOrbiting = new Color( 1.0f, .8f, .1f, 1.0f );
    private float qtype3_beginningScale = .2f;

    // brownian movement
    public bool  brownian_moving;
        // what the Delaunay uses so that brownian doesn't effect voids:
    public Vector3 brownian_centerPosition;
    float   brownian_prob_moveIfStopped;
    float   brownian_prob_stopIfMoving;
    bool    brownian_newPoint;
    float   brownian_distAllowedFromCenter = .1f;
    Vector2 brownian_pointInsideCircle;
    Vector2 brownian_stepSizeRange = new Vector2( .001f, .002f );
    float   brownian_stepSize;
        // velocity is reported as the stepSize scaled to this range ( and corresponds to localAmp in max )
    Vector2 brownian_reportedVelocityRange;

    // jittery movement
    private RandomJitteryId randomJitteryId;
    private string grow_or_shrink = "grow";
    // what the Delaunay uses so that jittery doesn't effect voids:
    public Vector3 jittery_centerPosition;
    public float distFromJitteryCenter = 0.0f;
    private float distIncrement = 0.0f;
    public float distMax = 1.0f;
    private float jitteryStartingScale;
    private float jitteryCurrentScale;
    public int jitteryVector;
    private float xTranslate;
    private float zTranslate;
        // currentSign is responsible for the illusion of the particle "splitting into two"
        // every other rendering, it translates/flips the position around the center point
    private int currentSign = 1;
    private int frameCount = 0;

    // go back to initial position
    Vector3 initialPosition;
    bool  moveBackToInitialPos_begin;
    bool  moveBackToInitialPos_moving;
    float moveBackToInitialPos_stepSize = .001f;
    float time_whenStoppedMoving = -1f;
    float time_deltaSinceStoppedMoving;
    float time_durationUntilBeginMovingBackToInitialPos = 6.0f;

    // voids
        // void structures
    public DelaunayTriangulation delaunayScript;
    private VoidsAllInfo voidsAllInfo_script;
    private List<Void_Cn> voidsAllInfo;
    private List<Void_Cn> voidsAllInfoPrev;
    private GameObject qbitOther;
    public GameObject[] qbitsAll;
    private GameObject delaunayTriangulation;
    bool stopRepulsiveVelocity;

        // bounding
    private bool checkIfIAmABoundingQbit_enabled = true;
    private bool iAmABoundingQbit;
    Void_Cn iAmBoundingThisVoid_voidCn;
    public int iAmBoundingThisVoid_id = -1;
    private Vector3 iAmBoundingThisVoid_centroid;
    private float iAmBoundingThisVoid_area;
    private bool iAmBoundingThisVoid_isOpening;
    private bool iAmBoundingThisVoid_isOpen;
    private bool iAmBoundingThisVoid_isOpenPrev;
    private BoundingQbit_ConvexHullOrder_AllInfo[] iAmBoundingThisVoid_qbits_hullOrder_allInfo;
    private BoundingQbit_ConvexHullOrder_AllInfo thisIsMy_qbit_hullOrder_allInfo;
    bool iAmConvexWhileOpeningTest;
    private HashSet<Vector3> iAmBoundingThisVoid_boundingCoords;
    float voidAreaMax = 6.0f;
    bool iBoundAVoid_contracting;
    bool iBoundAVoid_expanding;
    float contractingStepSize = .0005f;
    float contractingTimeTotal = 15.0f;
    float contractingTimeStart;
    float contractingTimeDelta;
    RaycastHit[] moveAwayFromVoid_rayHits;
    Vector3 moveAwayFromVoid_hitPtOnWall;
    float expandingStepSize;

        // these handle both qbits bouncing off the edge of the void ( they first are detected as being inside for one frame )
        // and responsible when voids open for booting out any qbits that don't belong ( arent bounding )
        // ** stepSize is a constant while still inside the void; once outside, start the timer and apply friction to stepsize
    bool  kickedOutOfAVoid_underway;
    bool  kickedOutOfAVoid_stillInside;
    float kickedOutOfAVoid_stepSize;
    bool  kickedOutOfAVoid_frictionBegin;
    float kickedOutOfAVoid_frictionTimeTotal = .8f;
    float kickedOutOfAVoid_frictionTimeStart;
    float kickedOutOfAVoid_frictionTimeDelta;
    bool iAmInsideThisVoid_test;
    bool iAmInsideThisVoid_testPrev;
    bool myInitialPosIsInsideVoid_test;
    Void_Cn iAmInsideThisVoid_allInfo;
    Void_Cn iAmInsideThisVoid_allInfoPrev;
    int iAmInsideThisVoid_id = -1;
    bool theVoidIAmInsideStillExists;
    float frameCountPrev;
    bool beginQtype3_moveTowardsCentroid;
    float beginQtype3_initialDistToCentroid;
    float beginQtype3_currentDistToCentroid;

        // qtype3 - frozen
    int         frozen_ampIndex;
    float       frozen_ampLerpStart;
    float       frozen_ampLerpDestination;
    string      frozen_direction;
    int         frozen_changeDirection;
    float       frozen_phraseDuration;
    float       frozen_phraseStartTime;
    float       frozen_phraseCurrTime;
    float       frozen_phase;
    int         frozen_folderNum;
    int         frozen_folderNumPrev = -1;
    int         frozen_numFolders;
    int         frozen_numSamplesInFolder;
    bool        frozen_theFirstDecrescendo; // <-- for switching from sphere to filament 
    int         frozen_bufferNum;
    int         frozen_bufferNumPrev = -1;
    int         frozen_sameBufferCounter;
    int         frozen_sameFolderCounter;
    int         frozen_bufferCountMax;
    int         frozen_folderCountMax;
    string      frozen_bufferPrefix;
    Vector2     frozen_bufferTranspositionRange;
    public bool frozen_sendNewBuffer;
    float[]     frozen_localAmpLouds;
    int         filament_treeSeed;
    float       filament_currGlobalEvolution_YscaleMax;
    float       filament_currGlobalEvolution_XZscale;
    float       filament_XZscale_thicker = 1f;
    float       filament_XZscale_thinner = .5f;
    float       ceilingHeight;
    delegate    void Frozen_NewLoudAmp();
    Frozen_NewLoudAmp del_Frozen_NewLoudAmp;
    Vector2     lightDisc_spinForceRange = new Vector2( 300f, 2000f );
    // for higher metal: some folders are sounds that are too high, so type the acceptable ones into a list here
    // List<int> frozen_folderChoices = new List<int> { 2, 3, 4, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 21, 22, 23 };
    //int       frozen_folderIndex;
    //int       frozen_folderIndexPrev = -1;

    // qtype3 - orbiting
    // 3 main sections for every orbiting event - transToOrbiting, orbiting, transToFrozen ( and during transToOrbiting the user might let go, which transitions back to frozen )
    enum Qtype3State { frozen, transToOrbiting, userLetGoTooSoon, orbiting, transToFrozen };
    Qtype3State qtype3_state;
        // these "section" variables are re-used in each section to mark time
        // orbit_section_begin and orbit_section_beginReportOsc are decoupled by about a frame - has to do with how could not use BEGINNING as if qtype3state != qtype3statePrev - because both were always the same 
        // script had to change itself to section_begin on the previous state AND the next frame needs to see section_begin == true; whatever; this works so live with it
    bool  orbit_section_begin;
    bool  orbit_section_beginReportOsc;
    float orbit_section_duration;
    float orbit_section_startTime;
    float orbit_section_deltaTime;
    float orbit_section_deltaTimePrev;
        // again, all short, all mid, and all long samples were made with line so that they reach the brightest sound
        // ( the beginning of the frozen sample at the strike of the pin ) at the same time; that dur is stored in this var 
        // so that when we transToFrozen, we begin at the brightest part of the file and timestretch -1 back to the beginning of the file
    float orbit_trans_durInBuffer_brightest;
        // DESIGN
            // a time val that can increase or decrease so we can run the transition phase forward or reverse
            // in transToOrbiting we add orbit_section_deltaTime to the clock; in userLetGoTooSoon we subtract orbit_section_deltaTime
    int   orbit_transition_folderIndex;
    float orbit_transition_transpGlobal; // needed in addition to aud_transpGlobal for when go from frozen back to sameTube
    float orbit_transition_clock;
    float orbit_section_phase;
        // so that we only have one transition buffer throughout transToOrbiting and userLetGo until frozen again
    bool      orbit_transition_newBufferReportOsc;
    bool      orbit_transToOrbiting_beginFromFrozen;
    List<int> orbit_transToOrbiting_sameTubeCountMaxRange;
    int       orbit_transToOrbiting_sameTubeCountMax;
    float     orbit_orbiting_grainRate; // from max
    List<int> orbit_orbiting_grainRateRange = new List<int> { 700, 45 };
        // physics / visual params that are determined by the grainRate from max...
    Vector2   orbit_speedRange = new Vector2( .04f, 1.7f );
    Vector2   orbit_rotationSpeedRange = new Vector2( 20f, 40f ); // basically in degrees per second
    Vector2   orbit_trailWidthRange = new Vector2( .005f, .065f );
    float     orbit_trailTime = 1.25f; // how many secs the trail lasts til disappears, and thus deterimes the trail length
    Vector2   orbit_scaleRange = new Vector2( .07f, .2f );

    float     orbit_orbitingXfadeOut_atDeltaTime;
    bool      orbit_orbitingXfadeOut_begin;
    bool      orbit_transToFrozenFadeOut;
    int       sameTubeCounter;

    enum Orbit_Transition_ShortMidLong { shortPhrase, midPhrase, longPhrase };
    Orbit_Transition_ShortMidLong orbit_transition_shortMidLong;
    string orbit_transition_shortMidLong_string;
    bool   orbit_userLetGoTooSoon_sendFadeOut;

        // transpLocal
    float transpLocal_startPt;
    float transpLocal_dest;
    float transpLocal_duration;
    float transpLocal_startTime;
    float transpLocal_deltaTime;
    float transpLocal_phase;

        // transitioning tree
    float   orbit_transition_filament_YscaleBegin;
    float   orbit_transition_filament_YscaleMax;
    Vector2 orbit_transition_filament_reachYscaleMaxAtPhaseVal_range = new Vector2( .07f, .75f );
    float   orbit_transition_filament_reachYscaleMaxAtPhaseVal;
        // needed to properly scale the filament yscale from the reverse running phase, which could be e.g. .35 to 0. when user let's go too soon
    float   orbit_userLetGoTooSoon_phaseValWhenLetGo; 
    float   orbit_transition_filament_YscaleCurrent;
    float   orbit_transition_filament_alpha;
    float   orbit_transition_filament_alphaBegin;

        // elipse movement params
    float orbit_width;
    float orbit_height;
    Vector2 orbit_widthRange  = new Vector2( .6f, 1.1f );
    Vector2 orbit_heightRange = new Vector2( .1f, .3f );
    float orbit_phase;
    float orbit_speed;
    float orbit_angle;
    float orbit_positionX;
    float orbit_positionZ;
    float orbit_rotationInitial;
    float orbit_rotationSpeed;
    public GameObject qbitsContainerGO;
    public GameObject orbitingParentPrefab;
    GameObject orbitingParentGO;
    InsideVoidQbit_Cn self_insideVoidQbit_cn;

    // debugging temp vars
    bool catchNullException;
    int frameNumPrev;
    bool penis;

    //_______________________________________________________________________________________

    void Start()
    {
        GetSelfID();
        self_position = this.transform.position;
        initialPosition = this.transform.position;
        moveToPosition = new Vector3( self_position.x, self_position.y, self_position.z );
        qbitsAll = GameObject.FindGameObjectsWithTag( "qbit" );
        mouseRaycast = GameObject.Find( "mouseRaycast" ).GetComponent<MouseRaycast>();
        delaunayTriangulation = GameObject.Find( "delaunayTriangulation" );
        randomJitteryId = GameObject.Find( "randomJitteryId" ).GetComponent<RandomJitteryId>();
        qtype1_scale = this.transform.localScale.x;
        self_rigidbody = GetComponent<Rigidbody>();
        self_collider  = GetComponent<SphereCollider>();
        self_renderer = GetComponent<Renderer>();
        self_renderer.material.color = new Color( qtype1_color.r, qtype1_color.g, qtype1_color.b, qtype1_alphaFaded );
        floorLength = GameObject.Find( "floor" ).GetComponent<Renderer>().bounds.size.x;
        floorCentroid = GameObject.Find( "floor" ).GetComponent<Renderer>().bounds.center;
        delaunayScript = GameObject.Find( "delaunayTriangulation" ).GetComponent<DelaunayTriangulation>();
        voidsAllInfo_script = GameObject.Find( "voidsAllInfo" ).GetComponent<VoidsAllInfo>();
        oscOutScript = GameObject.Find( "osc" ).GetComponent<OscOut>();
        oscInScript  = GameObject.Find( "osc" ).GetComponent<OscIn>();
        oscInScript.MapDouble( "/qbit/" + self_id + "/orbiting/grainRate", OscIn_OrbitingGrainRate );
        sampleInfo_JSONParsed = GameObject.Find( "loadJSON_fromMax" ).GetComponent<LoadJSON_FromMax>().sampleInfo_JSONParsed;
        swarmOrbitingEvents_script = GameObject.Find( "swarmOrbitingEvents" ).GetComponent<SwarmOrbitingEvents>();
        swarmParams_script = GameObject.Find( "swarmParams" ).GetComponent<SwarmParams>();
        globalEvolution_script = GameObject.Find( "globalEvolution" ).GetComponent<GlobalEvolution>();
        filament1_go = transform.Find( "filament1" ).gameObject;
        filament1_renderer = filament1_go.GetComponent<MeshRenderer>();
        filament2_go = transform.Find( "filament2" ).gameObject;
        filament2_renderer = filament2_go.GetComponent<MeshRenderer>();
        filament3_go = transform.Find( "filament3" ).gameObject;
        filament3_renderer = filament3_go.GetComponent<MeshRenderer>();
        filament4_go = transform.Find( "filament4" ).gameObject;
        filament4_renderer = filament4_go.GetComponent<MeshRenderer>();
        filament5_go = transform.Find( "filament5" ).gameObject;
        filament5_renderer = filament5_go.GetComponent<MeshRenderer>();
        ceilingHeight = GameObject.Find( "ceiling" ).transform.position.y;
        orbiting_trail = GetComponent<TrailRenderer>();
        orbiting_trail.time = orbit_trailTime;
        orbiting_trail.enabled = false;
        brownian_centerPosition = this.transform.position;

        mixer = new Mixer();
        MixerValues_Init();

        // neat random weighted test, especially since have 122 qbits to roll the dice
        /*
        List<int> weights = new List<int>() { 7, 5, 2 };
        int randIndex = RandomWeighted( weights );
        Debug.Log(randIndex);*/
    }

    void Update() 
    {
        selectedGO = UnityEditor.Selection.activeGameObject;
        self_position = this.transform.position;
        voidsAllInfo = voidsAllInfo_script.voidsAllInfo;

        globalEvolutionState = globalEvolution_script.globalEvolutionState;
        EvolutionParams();

        // get the voidsAllInfo, i.e. what is now called the Void_Cn, that I am inside of, if any
        if( voidsAllInfo != null )
        {
            if( qtype != 3 && qtype != 0 )
            {
                CheckIfIBoundAVoid();

                if( iAmABoundingQbit == false )
                {
                    CheckIfBrownian();
                }
            }

            CheckIfIAmInsideAnyVoid();
        }

        // checks
        CheckIfJittery();
            // on and off for polys
        CheckStartAndStopMoving( sampleInfo_JSONParsed );

            // needs to come after the CheckStartAndStopMoving() method:
        if( qtype != 3 && repelling == false )
        {
            // check if move back if bounding or if bounding a not-yet open void
            if( iAmABoundingQbit == false || ( iAmBoundingThisVoid_voidCn != null && iAmBoundingThisVoid_voidCn.isOpen == false ) )
            {
                CheckIfBeginMovingBackToInitialPos();
            }
        }

        // qtype3 possibilities ( trapped inside void )
        if( qtype == 3 )
        {
            // THESE CANNOT BE IF/ELSE IFs - when changing state, don't want to wait til the next frame to execute that state
            // would fuck up what gets sent in the osc and make it a pain in the ass to reason about
            if( qtype3_state == Qtype3State.frozen )
            {
                Frozen_Phrases();
            }
            // see orbiting_design patch for schema of how all this works and with max
            if( qtype3_state == Qtype3State.transToOrbiting )
            {
                Orbit_TransToOrbiting();
            }
            if( qtype3_state == Qtype3State.userLetGoTooSoon )
            {
                Orbit_UserLetGoTooSoon();
            }
            if( qtype3_state == Qtype3State.orbiting )
            {
                Orbit_Orbiting();
                // color alpha handled in the TrappedInsideVoidMovement method
            }
            if( qtype3_state == Qtype3State.transToFrozen )
            {
                Orbit_TransToFrozen();
            }
        }

        // send osc
        ReportOsc();

        // deep copy classes
        voidsAllInfoPrev = new List<Void_Cn>();
        foreach( Void_Cn voidEntry in voidsAllInfo )
        {
            Void_Cn deepCopy = voidEntry.DeepCopy();
            voidsAllInfoPrev.Add( deepCopy );
        }

        // prev and setbacks
        velocityMagnitudePrev = velocityMagnitude;
        qtypePrev = qtype;
        globalEvolutionStatePrev = globalEvolutionState;
        if( aud_beginQtype3 == true ) { aud_beginQtype3 = false; }
    }

    void FixedUpdate()
    {
        self_position = this.transform.position;
        hitpoint = mouseRaycast.hitpoint;

        // bounding bits can still fail to be kicked out of void to be convex, so...press M
        if(Time.frameCount != frameNumPrev)
        {
            if (selectedGO != null)
            {
                if (selectedGO.name == this.transform.name)
                {
                    if (Input.GetKey(KeyCode.M))
                    {
                        Debug.Log( "qbitSC hit " + hitpoint + " checkBounding " + checkIfIAmABoundingQbit_enabled + " bounding " + iAmABoundingQbit + " voidId " +iAmBoundingThisVoid_id + " voidCentr " + iAmBoundingThisVoid_centroid + " expanding " + iBoundAVoid_expanding+ " contracting " + iBoundAVoid_contracting+ " stopVelocity " + stopRepulsiveVelocity );
                    }
                }
            }
        }

        // if a qbit gets stuck bouncing back and forth trying to move back to init position
        if( Time.frameCount != frameNumPrev )
        {
            if( selectedGO != null )
            {
                if( selectedGO.name == this.transform.name )
                {
                    if( Input.GetKey(KeyCode.I))
                    {
                        Debug.Log("moveBackToInit " + moveBackToInitialPos_moving + " hitpoint " + hitpoint);
                    }
                }
            }
        }

        // physics
        if ( qtype == 0 )
        {
            QbitsJittery();
        }
        else if( qtype == 1 )
        {
            // the hierarchy of various movements as qtype 1s, and what can override what...
            if( iAmABoundingQbit == true )
            {
                IBoundAVoidMovement();
                if( iBoundAVoid_contracting == true || iBoundAVoid_expanding == true )
                {
                    // otherwise, a void keeps pulsing in and out til it settles
                    stopRepulsiveVelocity = true;
                }
            }
            if( kickedOutOfAVoid_underway == true )
            {
                stopRepulsiveVelocity = true;
                if( iAmInsideThisVoid_allInfo != null )
                {
                    KickedOutOfAVoidMovement(); // DEBUGGING <-- this isn't making the qbit drift cuz allInfo is indeed null
                }
            }
            if( moveBackToInitialPos_moving == true )
            {
                QbitsMovingBackToInitialPos();
            }
            if( brownian_moving == true )
            {
                if( iAmABoundingQbit == true ) { brownian_moving = false; }
                BrownianMovement();
            }

            if( iBoundAVoid_contracting == false && iBoundAVoid_expanding == false && kickedOutOfAVoid_underway == false )
            {
                QbitsRepulsion();
            }
        }
        else if( qtype == 2 )
        {
            QbitsAttraction();
        }
        else if( qtype == 3 )
        {
            TrappedInsideVoidMovement();
        }

        self_positionPrev = self_position;
        iAmInsideThisVoid_testPrev = iAmInsideThisVoid_test;
        orbit_section_deltaTimePrev = orbit_section_deltaTime;
        frameNumPrev = Time.frameCount;
    }

    private void OnCollisionEnter( Collision collision )
    {
        // qtype = 0: random jittery
        // qtype = 1: unentangled
        // qtype = 2: entangled
        // qtype = 3: when qtype2 passes in void and gets trapped

        if( qtype == 1 && collision.gameObject.tag == "qbit" && ePartner_id == -1 && iAmABoundingQbit == false )
        {
            int other_ePartner_id = collision.gameObject.GetComponent<QbitMovement>().ePartner_id;
            bool other_iAmABoundingQbit = collision.gameObject.GetComponent<QbitMovement>().iAmABoundingQbit;
            if( ( other_ePartner_id == -1 || other_ePartner_id == self_id ) && iAmABoundingQbit == false && other_iAmABoundingQbit == false )
            {
                ePartner_id = collision.gameObject.GetComponent<QbitMovement>().self_id;
                ePartnerObject = GameObject.Find( "qbit_" + ePartner_id );
                qtype = 2;
                eTimer = 0.0f;
                self_rigidbody.isKinematic = false;
                self_collider.enabled = true;
            }
        }
        if( qtype == 2 )
        {
            if( collision.gameObject.tag == "qbit" )
            {
                Physics.IgnoreCollision( collision.gameObject.GetComponent<Collider>(), self_collider );
            }
        }

        if( collision.gameObject.tag == "voidIgniter" )
        {
            Physics.IgnoreCollision( collision.gameObject.GetComponent<Collider>(), self_collider );
        }
    }

    void OnTriggerEnter( Collider other )
    {
        if( other.CompareTag( "disc" ) )
        {
            // DESIGN: the qbit has the is trigger collider, not the light disc 
            //         the radius of the qbit tree collision influence is determined by the radius of the capsule collider on the tree ( each filament on the qbit prefab )
            float randSpinForce = Random.Range( lightDisc_spinForceRange[0], lightDisc_spinForceRange[1] );
            other.GetComponent<Rigidbody>().AddTorque( 0, randSpinForce, 0 );
            other.GetComponent<LightDiscMovement>().collisionQbit = true;
        }
    }


    //_______________________________________________________________________________________

    void GetSelfID()
    {
        string[] splitName = new string[2];
        splitName = name.Split( '_' );
        string str_id = splitName[1];
        self_id = int.Parse( str_id );
    }

    //****************************************
    // oscOut
    void ReportOsc()
    {
        // DESIGN : most of these params need to drill down in the poly to EITHER /sample_player/0 or /sample_player/1, e.g. /sample_player/0/filename vs /sample_player/1/filename
        //          the poly does the assigning to this specific address space - which /sample_player
        //          we do not want to have to send /sample_player/id with every possible message so the poly can assign it properly...
        //          so instead, here, we send /sample_player/id every time there is an xfade of a new sound, i.e. every time there is a new /filename
        //          the poly stores and unions the latest /sample_player/id to every bundle, so it is present even when we aren't sending /sample_player/id
        if( qtype != 3 )
        {
            if( velocityMagnitude > 0 ) // <--- but threshold startedMoving + stoppedMoving handled separately below
            {
                oscOutScript.Send( "/qbit/" + self_id + "/amp/local",                      mx_velocitymagAsAmplitude_multiplier * velocityMagnitude );

                if( qtype == 1 && brownian_moving == true )
                {
                    oscOutScript.Send( "/qbit/" + self_id + "/transposition",              aud_transpGlobal );
                }
            }

            if( qtype != qtypePrev )
            {
                if( qtype == 0 )
                {
                    oscOutScript.Send( "/qbit/" + self_id + "/filename",                   mx_qtype0_bufferNamePrefix + self_id );
                    oscOutScript.Send( "/qbit/" + self_id + "/playPos",                    mx_qtype0_bufferPlayPos );
                    oscOutScript.Send( "/qbit/" + self_id + "/amp/global",                 mx_qtype0_ampGlobal );
                    oscOutScript.Send( "/qbit/" + self_id + "/transp/global",              mx_qtype0_transposition );
                    oscOutScript.Send( "/qbit/" + self_id + "/timestretch",                1 );
                    oscOutScript.Send( "/qbit/" + self_id + "/filter",                     1 ); // gate to which filter ( 1 = directionalLight, 2 = orbiting filter )
                }
                else if( qtype == 2 )
                {
                    oscOutScript.Send( "/qbit/" + self_id + "/filename",                   mx_qtype2_bufferNamePrefix + self_id );
                    oscOutScript.Send( "/qbit/" + self_id + "/playPos",                    mx_qtype2_bufferPlayPos );
                    oscOutScript.Send( "/qbit/" + self_id + "/amp/global",                 mx_qtype2_ampGlobal );
                    oscOutScript.Send( "/qbit/" + self_id + "/transp/global",              mx_qtype2_transposition );
                    oscOutScript.Send( "/qbit/" + self_id + "/timestretch",                1 );
                    oscOutScript.Send( "/qbit/" + self_id + "/filter",                     1 ); // gate to which filter

                    if( qtypePrev != 2 )
                    {
                        // this tells the poly to not interpret velocity 0 as stop playing - we want to hear the collision!
                        oscOutScript.Send( "/qbit/" + self_id + "/collision",              true );
                    }
                }

                aud_samplePlayer_xfade = 1 - aud_samplePlayer_xfade;
                oscOutScript.Send( "/qbit/" + self_id + "/sample_player/id",               aud_samplePlayer_xfade );
                oscOutScript.Send( "/qbit/" + self_id + "/sample_player/xfadeLineMsg",     aud_samplePlayer_xfade, 100 ); // <-- line msg, 0. or 1. in 100 ms
            }

            if( aud_startedMoving == true )
            {
                oscOutScript.Send( "/qbit/" + self_id + "/startedMoving",                  aud_startedMoving );

                if( qtype == 1 )
                {
                    oscOutScript.Send( "/qbit/" + self_id + "/sample_player/id",           aud_samplePlayer_xfade );
                    oscOutScript.Send( "/qbit/" + self_id + "/sample_player/xfadeLineMsg", aud_samplePlayer_xfade, 100 );
                    oscOutScript.Send( "/qbit/" + self_id + "/filename",                   aud_bufferName );
                    oscOutScript.Send( "/qbit/" + self_id + "/playPos",                    aud_bufferPlayPos );
                    oscOutScript.Send( "/qbit/" + self_id + "/transp/global",              aud_transpGlobal );
                    oscOutScript.Send( "/qbit/" + self_id + "/timestretch",                1 );
                    oscOutScript.Send( "/qbit/" + self_id + "/filter",                     1 ); // gate to which filter
                    if( repelling == true )
                    {
                        oscOutScript.Send( "/qbit/" + self_id + "/amp/global",             mx_qtype1Repel_ampGlobal );
                        penis = false;
                    }
                    else
                    {
                        oscOutScript.Send( "/qbit/" + self_id + "/amp/global",             aud_qtype1Brownian_ampGlobal );
                        penis = true;
                    }
                }

                aud_startedMoving = false;
            }

            if( aud_stoppedMoving == true )
            {
                oscOutScript.Send( "/qbit/" + self_id + "/stoppedMoving",                  aud_stoppedMoving );
                aud_stoppedMoving = false;
                repellingMovementJustStopped = false;
            }
        }
        else if( qtype == 3 )
        {
            if( qtype3_state == Qtype3State.frozen )
            {
                if( aud_beginQtype3 == true )
                {
                    aud_samplePlayer_xfade = 1 - aud_samplePlayer_xfade;
                    // poly on:
                    oscOutScript.Send( "/qbit/" + self_id + "/beginQtype3",                aud_beginQtype3 );
                    oscOutScript.Send( "/qbit/" + self_id + "/filter",                     1 );
                }

                if( frozen_sendNewBuffer == true )
                {
                    oscOutScript.Send( "/qbit/" + self_id + "/sample_player/id",           aud_samplePlayer_xfade );
                    oscOutScript.Send( "/qbit/" + self_id + "/sample_player/xfadeLineMsg", aud_samplePlayer_xfade, 100 );
                    oscOutScript.Send( "/qbit/" + self_id + "/playPos",                    aud_bufferPlayPos );
                    oscOutScript.Send( "/qbit/" + self_id + "/amp/global",                 aud_frozen_ampGlobal );
                    oscOutScript.Send( "/qbit/" + self_id + "/filename",                   aud_bufferName );
                    oscOutScript.Send( "/qbit/" + self_id + "/transp/global",              aud_transpGlobal );
                    oscOutScript.Send( "/qbit/" + self_id + "/timestretch",                1 );
                    oscOutScript.Send( "/qbit/" + self_id + "/filter",                     1 ); // gate to which filter
                    frozen_sendNewBuffer = false;
                }
                oscOutScript.Send( "/qbit/" + self_id + "/qtype",                          qtype );
                oscOutScript.Send( "/qbit/" + self_id + "/amp/local",                      aud_ampLocal );
            }
            else if( qtype3_state == Qtype3State.transToOrbiting )
            {
                if( orbit_transition_newBufferReportOsc == true )
                {
                    // sent only when going from frozen to transition: once per transition
                    // but not when user goes from userLetGoTooSoon back to trasToOrbiting
                    oscOutScript.Send( "/qbit/" + self_id + "/playPos",                    0 );
                    oscOutScript.Send( "/qbit/" + self_id + "/amp/global",                 aud_orbit_transToOrbiting_ampGlobal );
                    oscOutScript.Send( "/qbit/" + self_id + "/amp/local",                  1 );
                    oscOutScript.Send( "/qbit/" + self_id + "/filename",                   aud_bufferName );
                    oscOutScript.Send( "/qbit/" + self_id + "/transp/global",              aud_transpGlobal );
                    oscOutScript.Send( "/qbit/" + self_id + "/filter",                     2 ); // gate to which filter; 2 = orbiting filter
                    orbit_transition_newBufferReportOsc = false;
                }

                if( orbit_section_beginReportOsc == true )
                {
                    // sent every time user goes from userLetGoTooSoon back to trasToOrbiting
                    oscOutScript.Send( "/qbit/" + self_id + "/timestretch",                1 );
                    orbit_section_beginReportOsc = false;
                }
            }
            else if( qtype3_state == Qtype3State.userLetGoTooSoon )
            {
                if( orbit_section_beginReportOsc == true )
                {
                    // the sample player doesn't actually change or xfade here, we just need the current ID so odot can assign it to correct engine
                    // oscOutScript.Send( "/qbit/" + self_id + "/sample_player/xfadeLineMsg", aud_samplePlayer_xfade, 5 );
                    oscOutScript.Send( "/qbit/" + self_id + "/timestretch",                -1 );
                    orbit_section_beginReportOsc = false;
                }
                else if( orbit_userLetGoTooSoon_sendFadeOut == true )
                {
                    oscOutScript.Send( "/qbit/" + self_id + "/amp/local", aud_ampLocal );
                }
            }
            else if( qtype3_state == Qtype3State.orbiting )
            {
                // TODO TODO TODO swarm global amp?
                if( orbit_section_beginReportOsc == true )
                {
                    oscOutScript.Send( "/qbit/" + self_id + "/sample_player/id",           aud_samplePlayer_xfade );
                    oscOutScript.Send( "/qbit/" + self_id + "/sample_player/xfadeLineMsg", aud_samplePlayer_xfade, aud_samplePlayer_xfadeDur );
                    oscOutScript.Send( "/qbit/" + self_id + "/playPos",                    0 );
                    oscOutScript.Send( "/qbit/" + self_id + "/amp/global",                 aud_orbit_orbiting_ampGlobal );
                    oscOutScript.Send( "/qbit/" + self_id + "/amp/local",                  1 );
                    oscOutScript.Send( "/qbit/" + self_id + "/filename",                   aud_bufferName );
                    oscOutScript.Send( "/qbit/" + self_id + "/transp/global",              aud_transpGlobal );
                    oscOutScript.Send( "/qbit/" + self_id + "/timestretch",                1 );
                    oscOutScript.Send( "/qbit/" + self_id + "/getOrbitingData/collName",   aud_collName );
                    orbit_section_beginReportOsc = false;
                }

                oscOutScript.Send( "/qbit/" + self_id + "/transp/local",                   aud_transpLocal );

                if( orbit_orbitingXfadeOut_begin == true )
                {
                    oscOutScript.Send( "/qbit/" + self_id + "/sample_player/id",           aud_samplePlayer_xfade );
                    oscOutScript.Send( "/qbit/" + self_id + "/sample_player/xfadeLineMsg", aud_samplePlayer_xfade, aud_samplePlayer_xfadeDur );
                    oscOutScript.Send( "/qbit/" + self_id + "/playPos",                    aud_bufferPlayPos );
                    oscOutScript.Send( "/qbit/" + self_id + "/amp/global",                 mx_orbit_transToFrozen_ampGlobal );
                    oscOutScript.Send( "/qbit/" + self_id + "/amp/local",                  1 );
                    oscOutScript.Send( "/qbit/" + self_id + "/filename",                   aud_bufferName );
                    oscOutScript.Send( "/qbit/" + self_id + "/transp/global",              aud_transpGlobal );
                    oscOutScript.Send( "/qbit/" + self_id + "/timestretch",                -1 );
                    orbit_orbitingXfadeOut_begin = false;
                }
            }
            else if( qtype3_state == Qtype3State.transToFrozen )
            {
                if( orbit_transToFrozenFadeOut == true )
                {
                    // just fade out the transition file
                    oscOutScript.Send( "/qbit/" + self_id + "/amp/local",                  aud_ampLocal );
                }
            }
        }
    }

    //****************************************
    // oscIn
    void OscIn_OrbitingGrainRate( double grainRate )
    {
        // runs in reverse from 1. - 0. because in qtype3State.userLetGoTooSoon, timestretch is -1
        orbit_orbiting_grainRate = ( float )grainRate;
        // Debug.Log(orbit_orbiting_grainRate);
    }

    //_______________________________________________________________________________________
    // movement physics
    void QbitsJittery()
    {
       // only render every N frames - 60 fps is way too fast for the desired look
       if( frameCount % 2 == 0 )
       {
           switch( jitteryVector )
           {
               case 1: xTranslate = currentSign * distFromJitteryCenter; zTranslate = currentSign * distFromJitteryCenter; break;
               case 2: xTranslate = currentSign * distFromJitteryCenter; zTranslate = -1 * currentSign * distFromJitteryCenter; break;
               case 3: xTranslate = currentSign * distFromJitteryCenter; zTranslate = 0; break;
               case 4: xTranslate = 0; zTranslate = currentSign * distFromJitteryCenter; break;
           }

           this.transform.position = new Vector3( jittery_centerPosition.x + xTranslate, jittery_centerPosition.y, jittery_centerPosition.z + zTranslate );

           // new dist from center
           // only change dist from center every N frames - so the phrase lasts longer
           if( frameCount % 12 == 0 )
           {
               if( grow_or_shrink == "grow" )
               {
                   distIncrement = Random.Range( .005f, .08f );
                   distFromJitteryCenter += distIncrement;
               }
               else
               {
                   // shrinking
                   if( distFromJitteryCenter > .03 )
                   {
                       distIncrement = Random.Range( .01f, .03f );
                   }
                   else
                   {
                       distIncrement = Random.Range( .0005f, .0008f );
                   }
                   distFromJitteryCenter -= distIncrement;
               }

               // scale from 0 to .5 input because distMax has a range with a max .5, generated from CheckIfJittery()
               jitteryCurrentScale = Scale( distFromJitteryCenter, 0, .5f, qtype1_scale, qtype0_scaleMax );
               this.transform.localScale = new Vector3( jitteryCurrentScale, jitteryCurrentScale, jitteryCurrentScale );
               float alpha = Scale( distFromJitteryCenter, 0, .5f, .9f, 1.1f );
               self_renderer.material.color = new Color( qtype1_color.r, qtype1_color.g, qtype1_color.b, alpha );
            }

           // flip sign so does jittery thing
           currentSign *= -1;

           if( distFromJitteryCenter >= distMax )
           {
               grow_or_shrink = "shrink";
           }

            if( grow_or_shrink == "shrink" && distFromJitteryCenter <= 0 )
            {
                qtype = 1;
                self_rigidbody.isKinematic = false;
                self_collider.enabled = true;
                self_renderer.material.color = new Color( qtype1_color.r, qtype1_color.g, qtype1_color.b, qtype1_alphaFaded );
            }
       }

       frameCount++;
       velocityMagnitude = distFromJitteryCenter;
    }

    void BrownianMovement()
    {
        if( brownian_newPoint == true )
        {
            Vector2 brownian_pointInsideUnitCircle = Random.insideUnitCircle;
            brownian_pointInsideCircle = new Vector2( brownian_pointInsideUnitCircle.x * brownian_distAllowedFromCenter + brownian_centerPosition.x, brownian_pointInsideUnitCircle.y * brownian_distAllowedFromCenter + brownian_centerPosition.z );
            brownian_stepSize = Random.Range( brownian_stepSizeRange[0], brownian_stepSizeRange[1] );
            // stepSize determines speed, but velMagnitude always determines amplitude in max, so, must set it
            // here comes from the GlobalParams script as it evolves over time per density
            brownian_reportedVelocityRange = swarmParams_script.qbit_brownianReportedVelocityRange;
            velocityMagnitude = Scale( brownian_stepSize, brownian_stepSizeRange[0], brownian_stepSizeRange[1], brownian_reportedVelocityRange[0], brownian_reportedVelocityRange[1] );
            Vector2 alphaRange = swarmParams_script.qbit_brownianAlphaRange_current;
            float alpha = Scale( brownian_stepSize, brownian_stepSizeRange[0], brownian_stepSizeRange[1], alphaRange[0], alphaRange[1] );
            self_renderer.material.color = new Color( qtype1_color.r, qtype1_color.g, qtype1_color.b, alpha );

            // coroutine introduces a random delay so not all qbits switch to new transposition at same time: 
            if( swarmParams_script.qbit_qtype1_getNewTranspositionRange == true )
            {
                StartCoroutine( NewBrownianRollyPollyTransposition() );
            }

            brownian_newPoint = false;
        }
        else
        {
            this.transform.position = Vector3.MoveTowards( self_position, new Vector3( brownian_pointInsideCircle.x, self_position.y, brownian_pointInsideCircle.y ), brownian_stepSize );
            if( new Vector2( self_position.x, self_position.z ) == brownian_pointInsideCircle )
            {
                brownian_newPoint = true;
            }
        }
    }

    void QbitsMovingBackToInitialPos()
    {
        if( self_position == initialPosition || repelling == true )
        {
            brownian_centerPosition = initialPosition;
            moveBackToInitialPos_moving = false;
            velocityMagnitude = 0f;
            self_renderer.material.color = new Color( qtype1_color.r, qtype1_color.g, qtype1_color.g, qtype1_alphaFaded );
        }
        else
        {
            this.transform.position = Vector3.MoveTowards( this.transform.position, initialPosition, moveBackToInitialPos_stepSize );
            // just set as a constant value to determine amplitude; doesn't in any way reflect or effect their speed - that's just stepSize
            velocityMagnitude = .0005f;
            self_renderer.material.color = new Color( qtype1_color.r, qtype1_color.g, qtype1_color.b, .91f );
        }
    }

    void QbitsRepulsion()
    {
        //___________________________
        // force
        //___________________________

        if( stopRepulsiveVelocity == true )
        {
            velocity = velocityZero;
            stopRepulsiveVelocity = false;
        }

        distToClick = Vector3.Distance( hitpoint, self_position );

        distToClickThresh = .6f;
        forceScale = .1f;
        friction = .01f;

        if( distToClick <= distToClickThresh )
        {
            float deltaX = hitpoint.x - self_position.x;
            float deltaZ = hitpoint.z - self_position.z;
            // displacement in each dimension:
            float normVectorX = deltaX / distToClick;
            float normVectorZ = deltaZ / distToClick;
            float force = forceScale * ( 1 / Mathf.Pow( distToClick, 2 ) );
            if( force >= 1.0f )
            {
                force = 1.0f;
            }
            float accelerationX = normVectorX * force;
            float accelerationZ = normVectorZ * force;

            int frameSpeed; 

            if( iAmABoundingQbit == false )
            {
                frameSpeed = 60;
            }
            else
            {
                frameSpeed = 250;
            }

            velocity.x = ( velocity.x + accelerationX ) / frameSpeed;
            velocity.z = ( velocity.z + accelerationZ ) / frameSpeed;
            repelling = true;
            brownian_moving = false;
        }

        if( repelling == true )
        {
            // the magnitude of the velocity vector is the same as the distance between 
            // the current position and the previous position
            velocityMagnitude = Vector3.Magnitude( velocity );
            // velocity is aactually about .0166 as a max, but this max .002 helps it stay brighter longer
            float alpha = Scale( velocityMagnitude, 0f, .002f, qtype1_alphaFaded, 1f );
            self_renderer.material.color = new Color( qtype1_color.r, qtype1_color.g, qtype1_color.b, alpha );
            float currentScale = Scale( velocityMagnitude, 0f, .0166f, qtype1_scale, .12f );
            this.transform.localScale = new Vector3( currentScale, currentScale, currentScale );

            if( Mathf.Abs( velocity.x ) == 0.0f && Mathf.Abs( velocity.z ) == 0.0f )
            {
                StopRepelling();
            }

            // add friction and stop completely if still moving below magnitude thresh
            if( Mathf.Abs( velocity.x ) > 0.0f || Mathf.Abs( velocity.z ) > 0.0f )
            {
                velocity.x = velocity.x - friction * velocity.x;
                velocity.z = velocity.z - friction * velocity.z;
                if( velocityMagnitude <= velocityToStopQtype1 )
                {
                    StopRepelling();
                }
            }

            if( Mathf.Abs( self_position.x - velocity.x ) >= floorCentroid.x + floorLength / 2 || Mathf.Abs( self_position.z - velocity.z ) >= floorCentroid.z + floorLength / 2 )
            {
                // this safeguards a qbit from passing through a wall
                if( Mathf.Abs( self_position.x - velocity.x ) >= floorCentroid.x + floorLength / 2 )
                {
                    self_position.x += velocity.x;
                }
                if( Mathf.Abs( self_position.z - velocity.z ) >= floorCentroid.z + floorLength / 2 )
                {
                    self_position.z += velocity.z;
                }
                moveToPosition = new Vector3( self_position.x, self_position.y, self_position.z );
            }
            else
            {
                // else the repulsive force
                moveToPosition = new Vector3( self_position.x - velocity.x, self_position.y, self_position.z - velocity.z );
            }

            this.transform.position = moveToPosition;

            self_position = moveToPosition;
        }
    }

    void StopRepelling()
    {
        velocity = velocityZero;
        velocityMagnitude = 0.0f;
        repelling = false;
        repellingMovementJustStopped = true;
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

            hitpoint = mouseRaycast.hitpoint;
            distToClick = Vector3.Distance( hitpoint, self_position );

            ePartner_distToClick = ePartnerObject.GetComponent<QbitMovement>().distToClick;

            if( ePartner_id == -1 )
            {
                useSelfPhysics = true;
            }
            else
            {
                if (distToClick <= ePartner_distToClick )
                {
                    useSelfPhysics = true;
                }
                else
                {
                    useSelfPhysics = false;
                    ePartner_velocity2D = ePartnerObject.GetComponent<QbitMovement>().velocity2D;
                }
            }

            if( useSelfPhysics == true )
            {
                if( distToClick <= distToClickThresh )
                {
                    // from the Nature of Code
                    float deltaX = hitpoint.x - self_position.x;
                    float deltaZ = hitpoint.z - self_position.z;
                    Vector2 dirVector = new Vector2( deltaX, deltaZ );
                    dirVector.Normalize();
                    dirVector *= .0001f;
                    acceleration2D = dirVector;
                    velocity2D += acceleration2D;
                }
            }
            else if( useSelfPhysics == false )
            {
                if( ePartner_distToClick <= distToClickThresh )
                {
                    velocity2D.x = -ePartner_velocity2D.x;
                    velocity2D.y = -ePartner_velocity2D.y;
                }
            }
        }

        if( Mathf.Abs( velocity2D.x ) > 0.0f || Mathf.Abs( velocity2D.y ) > 0.0f )
        {
            velocity2D.x -= friction * velocity2D.x;
            velocity2D.y -= friction * velocity2D.y;
        }

        velocityMagnitude = Mathf.Sqrt( velocity2D.x * velocity2D.x + velocity2D.y * velocity2D.y );

        moveToPosition = new Vector3( self_position.x + velocity2D.x, self_position.y, self_position.z + velocity2D.y );
        this.transform.position = moveToPosition;

        if( eTimer < eDurationTotal )
        {
            eTimer += Time.deltaTime;
            float ePhase = eTimer / eDurationTotal;

            float currentScale = Mathf.Lerp( qtype2_scale, qtype1_scale, ePhase );
            this.transform.localScale = new Vector3( currentScale, currentScale, currentScale );
            this.GetComponent<Renderer>().material.color = Color.Lerp( qtype2_color, qtype1_color, ePhase );
        }
        else
        {
            ePartner_id = -1;
            ePartnerObject = null;
            qtype = 1;
            self_rigidbody.isKinematic = false;
            self_collider.enabled = true;
        }
    }

    void IBoundAVoidMovement()
    {
        // expanding
        if( iBoundAVoid_expanding == true )
        {
            if (iAmBoundingThisVoid_isOpening == true)
            {
                expandingStepSize = .005f;
            }
            else
            {
                expandingStepSize = .003f;
            }
            MoveAwayFromVoidCentroid( iAmBoundingThisVoid_centroid, expandingStepSize );
            if (selectedGO != null)
            {
                if (selectedGO.name == this.transform.name)
                {
                    if (Input.GetKey(KeyCode.M))
                    {
                        Debug.Log("iBoundAVoid_expanding");
                    }
                }
            }
            NullifyMouseHitPoint();
        }

        // contracting
        if( iBoundAVoid_contracting == false )
        {
            if( iAmBoundingThisVoid_area >= voidAreaMax )
            {
                iBoundAVoid_contracting = true;
                contractingTimeStart = Time.time;
                contractingTimeDelta = 0;
            }
        }

        if( iBoundAVoid_contracting == true )
        {
            // make it so contractingStepSize isn't a constant, but its exponential
            // qbits that travel further out from void have a larger contractingStepSize,
            // so they get pulled in more aggressively, so they can't make the radius of the void go too far out
            float distToCentroid = Vector3.Distance(self_position, iAmBoundingThisVoid_centroid);
            contractingStepSize = .000015f * Mathf.Pow(distToCentroid, 10);
            this.transform.position = Vector3.MoveTowards(self_position, iAmBoundingThisVoid_centroid, contractingStepSize);
            contractingTimeDelta += Time.time - contractingTimeStart;
            if (selectedGO != null)
            {
                if (selectedGO.name == this.transform.name)
                {
                    if (Input.GetKey(KeyCode.M))
                    {
                        Debug.Log("iBoundAVoid_contracting");
                    }
                }
            }
            NullifyMouseHitPoint();
        }

        if( contractingTimeDelta >= contractingTimeTotal && iBoundAVoid_contracting == true )
        {
            iBoundAVoid_contracting = false;
        }
    }

    void TrappedInsideVoidMovement()
    {
        if( qtype3_state == Qtype3State.frozen )
        {
            // if( iAmInsideThisVoid_allInfo.shakeIt_springingBegin == true ) { Debug.Log("orbiting"); }
            // only do once per frame, despite being in the fixed update
            if( Time.frameCount - frameCountPrev > 0 )
            {
                // voidsInfoPrev
                if( voidsAllInfoPrev != null )
                {
                    foreach( Void_Cn voidEntryPrev in voidsAllInfoPrev )
                    {
                        if( voidEntryPrev.id == iAmInsideThisVoid_allInfo.id )
                        {
                            iAmInsideThisVoid_allInfoPrev = voidEntryPrev;
                            break;
                        }
                    }
                }

                // ***
                // beginQtype3 move towards center
                if( aud_beginQtype3 == true )
                {
                    beginQtype3_initialDistToCentroid = Vector3.Distance( self_position, iAmInsideThisVoid_allInfo.centroid );
                    beginQtype3_moveTowardsCentroid = true;
                    // sets back to beginQtype3 = false at end of fixedUpdate() - at end of frame
                }

                if( beginQtype3_moveTowardsCentroid == true )
                {
                    beginQtype3_currentDistToCentroid = Vector3.Distance( self_position, iAmInsideThisVoid_allInfo.centroid );
                    if( beginQtype3_currentDistToCentroid > beginQtype3_initialDistToCentroid * .5f )
                    {
                        this.transform.position = Vector3.MoveTowards( self_position, iAmInsideThisVoid_allInfo.centroid, .03f );
                    }
                    else
                    {
                        beginQtype3_moveTowardsCentroid = false;
                    }
                }

                // ***
                // follow centroid around as void moves
                if( beginQtype3_moveTowardsCentroid == false )
                {
                    if( iAmInsideThisVoid_allInfoPrev != null )
                    {
                        float centroidDeltaXDistPrevFrame = iAmInsideThisVoid_allInfo.centroid.x - iAmInsideThisVoid_allInfoPrev.centroid.x;
                        float centroidDeltaZDistPrevFrame = iAmInsideThisVoid_allInfo.centroid.z - iAmInsideThisVoid_allInfoPrev.centroid.z;
                        this.transform.position = new Vector3( this.transform.position.x + centroidDeltaXDistPrevFrame, this.transform.position.y, this.transform.position.z + centroidDeltaZDistPrevFrame );
                    }
                }
            }
        }
        else if( qtype3_state == Qtype3State.orbiting )
        {
            if( orbitingParentGO != null )
            {
                // actual orbit and its speed:
                orbit_speed = Scale( orbit_orbiting_grainRate, orbit_orbiting_grainRateRange[0], orbit_orbiting_grainRateRange[1], orbit_speedRange[0], orbit_speedRange[1] );
                orbit_phase += Time.deltaTime * orbit_speed;
                orbit_phase %= 1f;

                orbit_angle = Mathf.Deg2Rad * 360 * orbit_phase;
                orbit_positionX = Mathf.Sin( orbit_angle ) * orbit_width;
                orbit_positionZ = Mathf.Cos( orbit_angle ) * orbit_height;

                orbitingParentGO.transform.position = iAmInsideThisVoid_allInfo.centroid;
                this.transform.localPosition = new Vector3( orbit_positionX, this.transform.position.y, orbit_positionZ );

                // rotation of that ellipse around the void centroid:
                orbit_rotationSpeed = Scale( orbit_orbiting_grainRate, orbit_orbiting_grainRateRange[0], orbit_orbiting_grainRateRange[1], orbit_rotationSpeedRange[0], orbit_rotationSpeedRange[1] );
                orbitingParentGO.transform.RotateAround( orbitingParentGO.transform.position, Vector3.up, orbit_rotationSpeed * Time.deltaTime );

                // trail:
                orbiting_trail.widthMultiplier = Scale( orbit_orbiting_grainRate, orbit_orbiting_grainRateRange[0], orbit_orbiting_grainRateRange[1], orbit_trailWidthRange[0], orbit_trailWidthRange[1] );

                // alpha
                float alpha = Scale( orbit_orbiting_grainRate, orbit_orbiting_grainRateRange[0], orbit_orbiting_grainRateRange[1], .05f, 1f );
                self_renderer.material.color = new Color( self_renderer.material.color.r, self_renderer.material.color.g, self_renderer.material.color.b, alpha );

                // scale 
                transform.localScale = Vector3.one * Scale( orbit_orbiting_grainRate, orbit_orbiting_grainRateRange[0], orbit_orbiting_grainRateRange[1], orbit_scaleRange[0], orbit_scaleRange[1] );
            }
        }

        frameCountPrev = Time.frameCount;
    }

    // handles both qbits bouncing off the edge of the void ( they first are detected as being inside for one frame )
    // and responsible when voids open for booting out any qbits that don't belong ( arent bounding )
    // ** stepSize is a constant while still inside the void; once outside, start the timer and apply friction to stepsize
    void KickedOutOfAVoidMovement()
    {
        // this movement is also responsible for kicking the qbits out that don't belong when the void is first opening
        if( iAmInsideThisVoid_test == true )
        {
            kickedOutOfAVoid_stepSize = .005f;
            Vector3 iAmInsideThisVoidCentroid = iAmInsideThisVoid_allInfo.centroid;
            MoveAwayFromVoidCentroid(iAmInsideThisVoidCentroid, kickedOutOfAVoid_stepSize);
            // for amplitude only
            velocityMagnitude = .004f;
            if (selectedGO != null)
            {
                if (selectedGO.name == this.transform.name)
                {
                    if (Input.GetKey(KeyCode.M))
                    {
                        Debug.Log("kickedOutOfVoid_movement_inside");
                    }
                }
            }
            NullifyMouseHitPoint();
        }
        else
        {
            if( iAmInsideThisVoid_testPrev == true )
            {
                kickedOutOfAVoid_stepSize = .005f;
                kickedOutOfAVoid_frictionTimeStart = Time.time;
                kickedOutOfAVoid_frictionBegin = false;
            }

            kickedOutOfAVoid_frictionTimeDelta = Time.time - kickedOutOfAVoid_frictionTimeStart;
            if( kickedOutOfAVoid_frictionTimeDelta <= kickedOutOfAVoid_frictionTimeTotal )
            {
                Vector3 iAmInsideThisVoidCentroid = iAmInsideThisVoid_allInfo.centroid;
                // friction:
                kickedOutOfAVoid_stepSize *= .98f;
                MoveAwayFromVoidCentroid( iAmInsideThisVoidCentroid, kickedOutOfAVoid_stepSize );
                // for amplitude only
                velocityMagnitude = Scale( kickedOutOfAVoid_stepSize, .005f, 0f, .002f, .0005f );
                if (selectedGO != null)
                {
                    if (selectedGO.name == this.transform.name)
                    {
                        if (Input.GetKey(KeyCode.M))
                        {
                            Debug.Log("kickedOutOfVoid_movement_outside");
                        }
                    }
                }
                NullifyMouseHitPoint();
            }
            else
            {
                kickedOutOfAVoid_underway = false;
                iAmInsideThisVoid_id = -1;
                iAmInsideThisVoid_allInfo = null;
            }
        }
    }

    // embedded in above ^
    void MoveAwayFromVoidCentroid( Vector3 thisVoidCentroid, float stepSize )
    {
        float rayCastMaxDistance = Mathf.Sqrt( Mathf.Pow( floorLength, 4 ) ); // the diagonal length of the floor

        // raycast direction = target - origin, if you want it to go from the origin through the target
        moveAwayFromVoid_rayHits = Physics.RaycastAll( thisVoidCentroid, self_position - thisVoidCentroid, rayCastMaxDistance );

        foreach( RaycastHit hit in moveAwayFromVoid_rayHits )
        {
            if( hit.transform.tag == "wall" )
            {
                moveAwayFromVoid_hitPtOnWall = hit.point;
                break;
            }
        }

        this.transform.position = Vector3.MoveTowards( self_position, moveAwayFromVoid_hitPtOnWall, stepSize );
    }

    //_______________________________________________________________________________________
    // checks
    void CheckIfJittery()
    {
        int jitteryQbitId = randomJitteryId.jitteryQbitId;

        if( jitteryQbitId == self_id && qtype != 2 && qtype != 3 )
        {
            qtype = 0;
            jittery_centerPosition = new Vector3( self_position.x, self_position.y, self_position.z );
            grow_or_shrink = "grow";
            distMax = Random.Range( .1f, .5f );
            self_rigidbody.isKinematic = true;
            self_collider.enabled = false;
            self_renderer.material.color = new Color( qtype1_color.r, qtype1_color.g, qtype1_color.b, 1f );

            jitteryVector = Random.Range( 1, 5 );
        }
    }

    void CheckIfBeginMovingBackToInitialPos()
    {
        if( repellingMovementJustStopped == true )
        {
            time_whenStoppedMoving = Time.time;
        }
        time_deltaSinceStoppedMoving = Time.time - time_whenStoppedMoving;

        if( brownian_centerPosition != initialPosition )
        {
            if( time_deltaSinceStoppedMoving >= time_durationUntilBeginMovingBackToInitialPos && moveBackToInitialPos_moving == false )
            {
                // don't move back to initialPos if its inside a void - otherwise would just bang head on side of void over and over
                CheckIfMyInitialPosIsInsideVoid();
                if( myInitialPosIsInsideVoid_test == false )
                {
                    moveBackToInitialPos_begin = true;
                    moveBackToInitialPos_moving = true;
                    moveBackToInitialPos_stepSize = Random.Range( .001f, .003f );
                    if (selectedGO != null)
                    {
                        if (selectedGO.name == this.transform.name)
                        {
                            if (Input.GetKey(KeyCode.M))
                            {
                                Debug.Log("movingBackToInit");
                            }
                        }
                    }
                    NullifyMouseHitPoint();
                }
            }
        }
    }

    void CheckIfIBoundAVoid()
    {
        // initial check - just get the void id; no longer look in this way if found to be bounding once
        if( checkIfIAmABoundingQbit_enabled == true ) // DEBUGGING <-- the problem...this is left false but we aint bounding no mo
        {

        }

        iAmABoundingQbit = false;
        foreach (Void_Cn voidEntry in voidsAllInfo)
        {
            if (iAmABoundingQbit == true) { break; }
            if (voidEntry.boundingQbits_allInfo != null)
            {
                foreach (BoundingQbit_AllInfo qbitEntry in voidEntry.boundingQbits_allInfo)
                {
                    if (self_id == qbitEntry.id )
                    {
                        iAmABoundingQbit = true;
                        iAmBoundingThisVoid_id = voidEntry.id;
                        checkIfIAmABoundingQbit_enabled = false;
                        self_collider.enabled = false;
                        break;
                    }
                }
            }
        }

        // thereafter, check every frame if still bounding that void id and if so, get its info
        if( iAmABoundingQbit == true )
        {
            foreach( Void_Cn voidEntry in voidsAllInfo )
            {
                if( iAmBoundingThisVoid_id == voidEntry.id )
                {
                    iAmBoundingThisVoid_voidCn = voidEntry;
                    iAmBoundingThisVoid_isOpening = voidEntry.isOpening;
                    iAmBoundingThisVoid_isOpen = voidEntry.isOpen;
                    iAmBoundingThisVoid_centroid = voidEntry.centroid;
                    iAmBoundingThisVoid_area = voidEntry.area;
                    iAmBoundingThisVoid_qbits_hullOrder_allInfo = voidEntry.boundingQbits_convexHullOrder_allInfo;

                    // convex test - while opening
                    if( voidEntry.isOpening == true )
                    {
                        iAmConvexWhileOpeningTest = false;
                        foreach( Vector2 pt in voidEntry.boundingQbits_convexHullOrder_positions )
                        {
                            // couldn't indicate self_postion == pt because it always fails when it moves; I think this 
                            // is because of when the voidMesh is rendered vs this Qbit script.
                            // self_positionPrev == pt also failed so I use the following distance calculation thresh instead
                            iAmConvexWhileOpeningTest = Vector2.Distance( new Vector2( self_position.x, self_position.z ), pt ) <= .1;
                            if( iAmConvexWhileOpeningTest == true ) { break; }
                        }

                        if( iAmConvexWhileOpeningTest == false )
                        {
                            iBoundAVoid_expanding = true;
                        }
                        else
                        {
                            iBoundAVoid_expanding = false;
                        }
                    }

                    // force bounding qbits ( id-ed by the DelaunayTriangulation ) to become qtype1, 
                    // or its all effed
                    if( qtype != 1 /*&& voidEntry.isOpen == true*/ )
                    {
                        qtype = 1;
                        self_rigidbody.isKinematic = false;
                        self_collider.enabled = true;
                        this.transform.localScale = new Vector3(qtype1_scale, qtype1_scale, qtype1_scale);
                        this.GetComponent<Renderer>().material.color = qtype1_color;
                        if( ePartnerObject != null )
                        {
                            ePartnerObject.GetComponent<QbitMovement>().ePartner_id = -1;
                        }

                        ePartner_id = -1;
                        ePartnerObject = null;
                    }

                    // convex test - once opened and all already convex;
                    // the voidMesh figures out if I am approaching concave, so just find out what it has to tell me
                    if( iAmBoundingThisVoid_qbits_hullOrder_allInfo != null )
                    {
                        foreach( BoundingQbit_ConvexHullOrder_AllInfo qbitEntry in iAmBoundingThisVoid_qbits_hullOrder_allInfo )
                        {
                            if( self_id == qbitEntry.id )
                            {
                                thisIsMy_qbit_hullOrder_allInfo = qbitEntry;
                                iBoundAVoid_expanding = qbitEntry.expandToMakeConvex;
                                break;
                            }
                        }

                        break;
                    }
                }
            }

            /*
            if (selectedGO != null)
            {
                if (selectedGO.name == this.transform.name)
                { Debug.Log(thisIsMy_qbit_hullOrder_allInfo.iAmFormingThisVertexAngle + " " + thisIsMy_qbit_hullOrder_allInfo.expandToMakeConvex); }
            }*/
        }

        if( ( iAmBoundingThisVoid_isOpen == false && iAmBoundingThisVoid_isOpenPrev == true ) || iAmBoundingThisVoid_centroid == Vector3.one * 0f || ( voidsAllInfo.Count == 0 && iAmBoundingThisVoid_isOpenPrev == true ) )
        {
            iAmABoundingQbit = false;
            iAmInsideThisVoid_id = -1;
            checkIfIAmABoundingQbit_enabled = true;
            self_collider.enabled = true;
            iBoundAVoid_expanding = false;
            iBoundAVoid_contracting = false;
        }
        iAmBoundingThisVoid_isOpenPrev = iAmBoundingThisVoid_isOpen;
    }

    void CheckIfIAmInsideAnyVoid()
    {
        // test qtype2's that haven't yet been captured by a void
        // ...and kick qtype1's out that don't belong
        if( qtype != 3 )
        {
            iAmInsideThisVoid_test = false;
            kickedOutOfAVoid_underway = false;
            foreach ( Void_Cn voidEntry in voidsAllInfo )
            {
                if( voidEntry.boundingQbits_convexHullOrder_allInfo != null )
                {
                    if( voidEntry.isOpening == true || voidEntry.isOpen == true )
                    {
                        iAmInsideThisVoid_test = CheckIfIAmInsideThisVoid( voidEntry.boundingQbits_convexHullOrder_positions, new Vector2( self_position.x, self_position.z ) );
                        if( iAmInsideThisVoid_test == true )
                        {
                            if( qtype == 2 )
                            {
                                // used to say voidEntry.DeepCopy().id - still works fine?  i think so
                                iAmInsideThisVoid_id = voidEntry.id;
                                iAmInsideThisVoid_allInfo = voidEntry;

                                qtype = 3;
                                qtype3_state = Qtype3State.frozen;
                                GetComponent<Renderer>().material.color = qtype3_colorFrozen;
                                self_rigidbody.isKinematic = true;
                                self_collider.enabled = false;
                                this.transform.localScale = Vector3.one * qtype3_beginningScale;
                                beginQtype3 = true;

                                // this condition necessary so doesn't add qtype 2s that exist in the void on formation as qtype3s and mess up the qtype3 globalEvolution count
                                // but keep the above outside of the condition so the void expands to convex, de-entangles entangleds, and kicks foreigners out correctly
                                if( voidEntry.isOpen )  
                                {
                                    // DEBUGGING position added only for debug with gizmos in globalEvolution script - delete from InsideVoidQbit_Cn after fix your shit
                                    self_insideVoidQbit_cn = new InsideVoidQbit_Cn { id = self_id, qtype = qtype, position = transform.position, orbiting = false };
                                    if( voidEntry.insideVoidQbits_allInfo == null ){ voidEntry.insideVoidQbits_allInfo = new List<InsideVoidQbit_Cn>(); }
                                    voidEntry.insideVoidQbits_allInfo.Add( self_insideVoidQbit_cn );
                                }

                                // tell my partner I'm breaking up with her/him
                                QbitMovement ePartnerQbitScript = ePartnerObject.GetComponent<QbitMovement>();
                                ePartnerQbitScript.ePartner_id = -1;
                                ePartnerQbitScript.useSelfPhysics = true;

                            }
                            else if( qtype == 1 && iAmABoundingQbit == false )
                            {
                                // KICK QTYPE 1s out that don't bound the void...
                                kickedOutOfAVoid_underway = true;
                                iAmInsideThisVoid_id = voidEntry.id;
                                iAmInsideThisVoid_allInfo = voidEntry;
                                if (selectedGO != null)
                                {
                                    if (selectedGO.name == this.transform.name)
                                    {
                                        if (Input.GetKey(KeyCode.M))
                                        {
                                            Debug.Log("kickedOutOfVoid_check");
                                        }
                                    }
                                }
                                NullifyMouseHitPoint();
                            }

                            break;
                        }
                    }
                }
            }
        }

        // if already captured by a void, update iAmInsideThis_VoidAllInfo; test if void is closed
        if( qtype == 3 )
        {
            if( voidsAllInfo.Count == 0 )
            {
                theVoidIAmInsideStillExists = false;
            }
            else
            {
                foreach( Void_Cn voidEntry in voidsAllInfo )
                {
                    if( iAmInsideThisVoid_id == voidEntry.id && voidEntry.isOpen == true )
                    {
                        iAmInsideThisVoid_allInfo = voidEntry;
                        theVoidIAmInsideStillExists = true;
                        break;
                    }
                    else
                    {
                        theVoidIAmInsideStillExists = false;
                    }
                }

                if( theVoidIAmInsideStillExists == false )
                {
                    qtype = 1;
                    Filaments_DisableAll();
                    GetComponent<Renderer>().material.color = qtype1_color;
                    this.transform.localScale = new Vector3( qtype1_scale, qtype1_scale, qtype1_scale );
                    self_renderer.enabled = true;
                    iAmInsideThisVoid_allInfo = null;
                    iAmInsideThisVoid_id = -1;
                }
            }
        }
    }

    void CheckIfMyInitialPosIsInsideVoid()
    {
        myInitialPosIsInsideVoid_test = false;

        foreach( Void_Cn voidEntry in voidsAllInfo )
        {
            if( voidEntry.boundingQbits_convexHullOrder_allInfo != null )
            {
                if( voidEntry.isOpen == true )
                {
                    myInitialPosIsInsideVoid_test = CheckIfIAmInsideThisVoid( voidEntry.boundingQbits_convexHullOrder_positions, new Vector2( initialPosition.x, initialPosition.z ) );
                    if( myInitialPosIsInsideVoid_test == true )
                    {
                        break;
                    }
                }
            }
        }
    }

    public static bool CheckIfIAmInsideThisVoid( Vector2[] voidPts, Vector2 qbitPosition )
    {
        /// http://wiki.unity3d.com/index.php/PolyContainsPoint
        /// returns true if the point is inside the polygon; otherwise, false
        bool inside = false;
        int j = voidPts.Length - 1;
        for( int i = 0; i < voidPts.Length; j = i++ )
        {
            if( ( ( voidPts[i].y <= qbitPosition.y && qbitPosition.y < voidPts[j].y) || (voidPts[j].y <= qbitPosition.y && qbitPosition.y < voidPts[i].y ) ) &&
              ( qbitPosition.x < (voidPts[j].x - voidPts[i].x ) * ( qbitPosition.y - voidPts[i].y ) / ( voidPts[j].y - voidPts[i].y ) + voidPts[i].x ) )
            {
                inside = !inside;
            }
        }
        return inside;
    }

    void CheckIfBrownian()
    {
        if( brownian_moving == false )
        {
            brownian_prob_moveIfStopped = swarmParams_script.qbit_brownianProb_moveIfStopped;
            brownian_moving = Random.Range( 0f, 1f ) <= brownian_prob_moveIfStopped;
            if( brownian_moving == true )
            {
                brownian_newPoint = true;
            }
        }
        else
        {
            brownian_prob_stopIfMoving = swarmParams_script.qbit_brownianProb_stopIfMoving;
            brownian_moving = Random.Range( 0f, 1f ) >= brownian_prob_stopIfMoving;
            if( brownian_moving == false )
            {
                velocityMagnitude = 0;
                self_renderer.material.color = new Color( qtype1_color.r, qtype1_color.g, qtype1_color.b, qtype1_alphaFaded );
            }
        }
    }

    void CheckStartAndStopMoving( JSONNode sampleInfo_JSONParsed )
    {
        // stop
        if( velocityMagnitude == 0 && velocityMagnitudePrev > 0 )
        {
            // mouseclick qtype 1:
            if( ( qtype == 1 && repellingMovementJustStopped == true ) || qtype == 2 )
            {
                brownian_centerPosition = this.transform.position;
            }
            aud_stoppedMoving = true;
            // hitpoint = new Vector3( 15, 15, 15 );
            // mouseRaycast.hitpoint = new Vector3( 15, 15, 15 );
        }

        // start
        if( velocityMagnitude > 0 && velocityMagnitudePrev == 0 )
        {
            aud_startedMoving = true;
            if( qtype == 1 )
            {
                // mouse click repelling:
                if( repelling == true || moveBackToInitialPos_moving == true )
                {
                    aud_bufferName = mx_qtype1Repel_bufferName;
                    bufferDuration = sampleInfo_JSONParsed[ "qbit_rollypolly/1/duration" ];
                    aud_bufferPlayPos = Random.Range( 0f, bufferDuration );
                    Vector2 transpositionRange = swarmParams_script.qbit_qtype1Brownian_transpositionRange;
                    aud_transpGlobal = Random.Range( transpositionRange[0], transpositionRange[1] );
                }
                // brownian:
                else
                {
                    aud_bufferName = swarmParams_script.qbit_brownianBuffer_name;
                    bufferDuration = swarmParams_script.qbit_brownianBuffer_duration;
                    aud_bufferPlayPos = Random.Range( 0f, bufferDuration );
                    Vector2 transpositionRange = swarmParams_script.qbit_qtype1Brownian_transpositionRange;
                    aud_transpGlobal = Random.Range( transpositionRange[0], transpositionRange[1] );
                    // transposition also handled constantly in the BrownianMovement()
                }
            }
        }
    }

    // ***************************************
    // qtype3 inside void time and buffer management - frozen and orbiting

    public void Frozen_Phrases()
    {
        // ****
        // in Frozen() and all of the associated Frozen methods, generic param names, e.g. frozen_bufferPrefix, are set in EvolutionParams()

        if( beginQtype3 == true )
        {
            StartCoroutine( Frozen_BeginQtype3() );
            beginQtype3 = false;
        }

        frozen_phraseCurrTime = Time.time - frozen_phraseStartTime;

        // phrase is still going...lerp amp
        if( frozen_phraseCurrTime < frozen_phraseDuration )
        {
            frozen_phase = frozen_phraseCurrTime / frozen_phraseDuration;
            aud_ampLocal = Mathf.Lerp( frozen_ampLerpStart, frozen_ampLerpDestination, frozen_phase );
            float ampClamped = Mathf.Clamp01( aud_ampLocal );

            // ****
            // filament
            if( frozen_theFirstDecrescendo == false )
            {
                filament_current_go.transform.localScale = new Vector3( filament_current_go.transform.localScale.x, Scale( ampClamped, 0f, 1f, 0f, filament_currGlobalEvolution_YscaleMax ), filament_current_go.transform.localScale.z );
                Color filamentColor = qtype3_colorFrozen;
                float alpha = Scale( ampClamped, 0f, 1f, 0, 1f );
                filament_current_renderer.material.color = new Color( filamentColor.r, filamentColor.g, filamentColor.b, alpha );
            }
        }
        // phrase is over; determine where we are going next and assign new values
        else
        {
            // the prev direction assignment
            if( frozen_direction == "get loud" )
            {
                // always get quiet if was just loud
                frozen_direction = "get quiet";
                Frozen_NewQuietAmp();
            }
            else if( frozen_direction == "get quiet" )
            {
                // weighted: usually stay quiet
                frozen_changeDirection = Random.Range( 1, 6 );
                if( frozen_changeDirection <= 2 ) { frozen_direction = "get loud"; }
                else                              { frozen_direction = "get quiet"; }
            }

            // and now what to do with the new direction assignment
            if( frozen_direction == "get loud" )
            {
                del_Frozen_NewLoudAmp();
                NewFolderAndBufferHandling_WoodDarkFrozen();

                // ****
                // filament
                if( frozen_theFirstDecrescendo == true )
                {
                    self_renderer.enabled = false;
                    frozen_theFirstDecrescendo = false;
                }
                NewFilament();
            }
            else if( frozen_direction == "get quiet" )
            {
                Frozen_NewQuietAmp();
            }
        }

        // condition for change to transToOrbiting
        if( iAmInsideThisVoid_allInfo.shakeIt_displacingBegin == true && filament_current_go != null )
        {
            qtype3_state = Qtype3State.transToOrbiting;
            orbit_transToOrbiting_beginFromFrozen = true;
            orbit_section_begin = true;
        }

        // for the qbit sphere when still being sucked into the void:
        float sphereAlpha = aud_ampLocal;
        Color beginQtype3Color = new Color( qtype3_colorFrozen.r, qtype3_colorFrozen.g, qtype3_colorFrozen.b, sphereAlpha );
        this.GetComponent<Renderer>().material.color = beginQtype3Color;
    }

    void Frozen_NewLoudAmp_Unweighted()
    {
        frozen_ampLerpStart = aud_ampLocal;
        frozen_ampIndex = Random.Range( 0, frozen_localAmpLouds.Length );
        frozen_ampLerpDestination = frozen_localAmpLouds[ frozen_ampIndex ];
        frozen_phraseDuration = Random.Range( .7f, 4.0f );
        frozen_phraseStartTime = Time.time;
    }

    void Frozen_NewLoudAmp_BeginSpin()
    {
        // same as above method, but weighted amp destination selection, as height of the filament is scaled from the amplitude of the sound
        frozen_ampLerpStart = aud_ampLocal;
        frozen_ampIndex = RandomWeighted( mx_frozen_localAmpLouds_weightedProbs_beginSpin );
        frozen_ampLerpDestination = frozen_localAmpLouds[ frozen_ampIndex ];
        // Debug.Log(frozen_ampLerpDestination);
        frozen_phraseDuration = Random.Range( .7f, 4.0f );
        frozen_phraseStartTime = Time.time;
    }

    void Frozen_NewQuietAmp()
    {
        frozen_ampLerpStart = aud_ampLocal;
        frozen_ampIndex = Random.Range( 0, mx_frozen_localAmpQuiets.Length );
        frozen_ampLerpDestination = mx_frozen_localAmpQuiets[ frozen_ampIndex ];
        frozen_phraseDuration = Random.Range( 1.0f, 3.0f );
        frozen_phraseStartTime = Time.time;
    }

    void NewFolderAndBufferHandling_WoodDarkFrozen()
    {
        frozen_bufferPrefix = mx_frozen_bufferPrefix_woodDark;
        frozen_numSamplesInFolder = sampleInfo_JSONParsed[ "count/" + mx_frozen_bufferPrefix_woodDark + "1" ];
        frozen_bufferCountMax = mx_frozen_sameBufferCountMax;
        frozen_bufferTranspositionRange = mx_frozen_bufferTranspositionRange_woodDark;

        if( frozen_sameBufferCounter >= frozen_bufferCountMax || aud_beginQtype3 == true )
        {
            // folder - stays the same no matter what - only 1 folder
            frozen_folderNum = 1;

            // buffer
            frozen_sameBufferCounter = 0;
            frozen_bufferNum = Random.Range(1, frozen_numSamplesInFolder + 1);
            if (frozen_bufferNum == frozen_bufferNumPrev)
            {
                frozen_bufferNum = ( frozen_bufferNum % frozen_numSamplesInFolder ) + 1;
            }
            aud_bufferName = frozen_bufferPrefix + frozen_folderNum + "." + frozen_bufferNum;

            // pos
            aud_bufferPlayPos = 0;

            // transp
            aud_transpGlobal = Random.Range( frozen_bufferTranspositionRange[0], frozen_bufferTranspositionRange[1] );

            frozen_folderNumPrev = frozen_folderNum;
            frozen_bufferNumPrev = frozen_bufferNum;
            frozen_sendNewBuffer = true;
        }
        else
        {
            frozen_sameBufferCounter++;
        }
    }

    void NewFilament()
    {
        // there are 5 tree prefabs - you can't generate a new seed at runtime
        Filaments_DisableAll();
        filament_treeSeed = Random.Range( 1, 6 );
        switch( filament_treeSeed )
        {
            case 1: filament_current_go = filament1_go; filament_current_renderer = filament1_renderer; break;
            case 2: filament_current_go = filament2_go; filament_current_renderer = filament2_renderer; break;
            case 3: filament_current_go = filament3_go; filament_current_renderer = filament3_renderer; break;
            case 4: filament_current_go = filament4_go; filament_current_renderer = filament4_renderer; break;
            case 5: filament_current_go = filament5_go; filament_current_renderer = filament5_renderer; break;
        }

        // this gives us a random rotation around the y-axis
        filament_current_go.transform.rotation = new Quaternion( 0f, Random.Range( 0f, 1f ), 0f, Random.Range( 0f, 1f ) );
        filament_current_go.transform.localScale = new Vector3( filament_current_go.transform.localScale.x, 0f, filament_current_go.transform.localScale.z );
        filament_current_go.SetActive( true );
        filament_current_go.transform.localScale = new Vector3( filament_currGlobalEvolution_XZscale, 0f, filament_currGlobalEvolution_XZscale );
    }

    void Filaments_DisableAll()
    {
        filament1_go.SetActive( false );
        filament2_go.SetActive( false );
        filament3_go.SetActive( false );
        filament4_go.SetActive( false );
        filament5_go.SetActive( false );
    }

    void Orbit_TransToOrbiting()
    {
        // user displacing void, transitioning to orbiting

        // sample durations are uniform for short, mid, and long samples, as they were made with line msgs
        // short                                                mid          long
        // 1200 ms to brightest/orbiting/start of pin sample    2500 ms      3300 ms
        // ...then every sample stays bright for an additional 4000 ms so it can be decrescendoed with that sound.  Thus sample durs are approximately:

        // short      mid        long
        // 5200 ms    6500 ms    7300 ms
        // we'll only really use this knowledge in the Orbit_TransToFrozen() method at the end of the orbiting phrase

        // **** DESIGN: the section_phase here runs throughout both transToOrbiting ( running forward 0 - 1 ) and userLetGoTooSoon ( running in reverse 1 - 0 ); 
        //              also use the same orbit_section_duration, orbit_transition_filament_YscaleMax and orbit_transition_filament_reachYscaleMaxAtPhaseVal
        //              orbit_transition_clock is a time val that can increase or decrease so we can run the transition phase forward or reverse
        //              in transToOrbiting we add orbit_section_deltaTime to the clock; in userLetGoTooSoon we subtract orbit_section_deltaTime

        // *****
        // begin state...
        if( orbit_section_begin == true )
        {
            // ** TIME
            orbit_section_startTime = Time.time;
            if( orbit_transToOrbiting_beginFromFrozen == true )
            {
                orbit_transition_clock = 0;
            }
            // else, coming from userLetGoTooSoon, we continue with its val from the orbit_transition_clock 

            // ** FILAMENT
            Filament_TransToOrbiting_Growth();

            // ** SHORT MID LONG phrases
            int randInt = RandomWeighted( mx_orbit_transToOrbiting_probShortMidLong );
            switch( randInt )
            {
                case 0: orbit_transition_shortMidLong = Orbit_Transition_ShortMidLong.shortPhrase; orbit_transition_shortMidLong_string = "short"; orbit_section_duration = 1.2f; orbit_trans_durInBuffer_brightest = 1200; break;
                case 1: orbit_transition_shortMidLong = Orbit_Transition_ShortMidLong.midPhrase;   orbit_transition_shortMidLong_string = "mid";   orbit_section_duration = 2.5f; orbit_trans_durInBuffer_brightest = 2500; break;
                case 2: orbit_transition_shortMidLong = Orbit_Transition_ShortMidLong.longPhrase;  orbit_transition_shortMidLong_string = "long";  orbit_section_duration = 3.3f; orbit_trans_durInBuffer_brightest = 3300; break;
            }

            // ** AUDIO
            if( orbit_transToOrbiting_beginFromFrozen == true )
            {
                // Debug.Log( "max " + orbit_transToOrbiting_sameTubeCountMax + " count " + sameTubeCounter );
                if( sameTubeCounter == orbit_transToOrbiting_sameTubeCountMax )
                {
                    TubeTransition_NewTube();
                    // aud_samplePlayer_xfade = 1 - aud_samplePlayer_xfade;
                }
                else
                {
                    TubeTransition_SameTube();
                }

                sameTubeCounter++;
                orbit_transition_newBufferReportOsc = true;
            }

            orbit_section_beginReportOsc = true;
            orbit_section_begin = false;
            orbit_transToOrbiting_beginFromFrozen = false;

            // additionally, ReportOsc(): playPos 0, aud_ampGlobal = mx_orbit_ampGlobal, ampLocal 1, phaseReport 0, timestretch 1 
        }

        // *****
        // continuing this state...
        orbit_transition_clock += Time.deltaTime;
        orbit_section_phase = Mathf.Clamp( orbit_transition_clock / orbit_section_duration, 0f, 1f );

        if( orbit_section_phase <= orbit_transition_filament_reachYscaleMaxAtPhaseVal )
        {
            // goes up
            orbit_transition_filament_YscaleCurrent = Scale( orbit_section_phase, 0f, orbit_transition_filament_reachYscaleMaxAtPhaseVal, orbit_transition_filament_YscaleBegin, orbit_transition_filament_YscaleMax );
        }
        else
        {
            // goes down
            orbit_transition_filament_YscaleCurrent = Scale( orbit_section_phase, orbit_transition_filament_reachYscaleMaxAtPhaseVal, 1f, orbit_transition_filament_YscaleMax, 0f );
        }

        filament_current_go.transform.localScale = new Vector3( filament_current_go.transform.localScale.x, orbit_transition_filament_YscaleCurrent, filament_current_go.transform.localScale.z );
        filament_current_renderer.material.color = Color.Lerp( qtype3_colorFrozen, qtype3_colorOrbiting, orbit_section_phase );

        // *****
        // condition change to userLetGoTooSoon...
        if( iAmInsideThisVoid_allInfo.shakeIt_springingBegin == true && orbit_transition_clock < orbit_section_duration )
        {
            qtype3_state = Qtype3State.userLetGoTooSoon;
            orbit_section_begin = true;
        }
        // condition change to orbiting...
        else if( orbit_section_phase >= 1f )
        {
            qtype3_state = Qtype3State.orbiting;
            orbit_section_begin = true;
        }
    }

    void Filament_TransToOrbiting_Growth()
    {
        // where we begin from
        orbit_transition_filament_YscaleBegin = filament_current_go.transform.localScale.y;
        // how high to go
            // guarantee that no matter where the filament is at this beginning, it will always grow taller when we begin transitioning
            // ...contingent on the current filament_currGlobalEvolution_YscaleMax
        float YscaleBegin_deltaTo_currGlobalEvolution_YscaleMax = filament_currGlobalEvolution_YscaleMax - orbit_transition_filament_YscaleBegin;
        float growth_min_allowed = YscaleBegin_deltaTo_currGlobalEvolution_YscaleMax *  .5f;
        float growth_max_allowed = YscaleBegin_deltaTo_currGlobalEvolution_YscaleMax * 1.1f;
        float orbit_transition_filament_additionalHeight = Random.Range( growth_min_allowed, growth_max_allowed );
        orbit_transition_filament_YscaleMax = orbit_transition_filament_YscaleBegin + orbit_transition_filament_additionalHeight;
            // when to arrive at that height
        orbit_transition_filament_reachYscaleMaxAtPhaseVal = Random.Range( orbit_transition_filament_reachYscaleMaxAtPhaseVal_range[0], orbit_transition_filament_reachYscaleMaxAtPhaseVal_range[1] );
    }

    void TubeTransition_NewTube()
    {
        // orbit_transition_folderIndex is set here and then stays the same / is referenced for each orbit section
        orbit_transition_folderIndex = Random.Range( 1, sampleInfo_JSONParsed[ "numFolders/tube/trans_frozen_and_orbiting" ] + 1 );
        int numSamples = sampleInfo_JSONParsed[ mx_orbit_transition_bufferPrefix + orbit_transition_folderIndex + "_" + orbit_transition_shortMidLong_string + "/count" ];
        int bufferIndex = Random.Range( 1, numSamples + 1 );
        aud_bufferName = mx_orbit_transition_bufferPrefix + orbit_transition_folderIndex + "_" + orbit_transition_shortMidLong_string + "." + bufferIndex;
        orbit_transition_transpGlobal = Random.Range( mx_frozen_bufferTranspositionRange_tubesLow[0], mx_frozen_bufferTranspositionRange_tubesLow[1] );
        // need a local transpGlobal variable in addition to aud_transpGlobal for when go from frozen back to same tube
        aud_transpGlobal = orbit_transition_transpGlobal;
        orbit_transToOrbiting_sameTubeCountMax = Random.Range( orbit_transToOrbiting_sameTubeCountMaxRange[0], orbit_transToOrbiting_sameTubeCountMaxRange[1] + 1 );
        sameTubeCounter = 0;
    }

    void TubeTransition_SameTube()
    {
        // folderIndex and transp stay the same, but let's change the sample for variety
        int numSamples = sampleInfo_JSONParsed[ mx_orbit_transition_bufferPrefix + orbit_transition_folderIndex + "_" + orbit_transition_shortMidLong_string + "/count" ];
        int bufferIndex = Random.Range( 1, numSamples + 1 );
        aud_bufferName = mx_orbit_transition_bufferPrefix + orbit_transition_folderIndex + "_" + orbit_transition_shortMidLong_string + "." + bufferIndex;
        aud_transpGlobal = orbit_transition_transpGlobal;
    }

    void Orbit_UserLetGoTooSoon()
    {
        // user lets go of void before reached total orbit_section_duration, lerp back to frozen
        // phase runs in reverse 1 - 0.
        // if user let go in middle of phase, phase could start at e.g. .35 and go back to 0.

        // *****
        // begin state...
        if( orbit_section_begin == true )
        {
            orbit_section_startTime = Time.time;
            // orbit_transition_filament_YscaleBegin = filament_current_go.transform.localScale.y;
            // orbit_transition_filament_reachYscaleMaxAtPhaseVal = Random.Range( orbit_transition_filament_reachYscaleMaxAtPhaseVal_range[0], orbit_transition_filament_reachYscaleMaxAtPhaseVal_range[1] );
                // separate method because the phase here is something like e.g. .35 to 0.
                // so our yscaleMax target phase time target must be guaranteed to be a moment in the remaining phase back to 0. 
            Filament_UserLetGoTooSoon_Growth();
            // Debug.Log("begin " + orbit_transition_filament_YscaleBegin + " target " + orbit_transition_filament_YscaleMax + " phase " + orbit_transition_filament_reachYscaleMaxAtPhaseVal );
            orbit_section_begin = false;
            orbit_section_beginReportOsc = true;

            // ReportOsc(): phaseReport 1, timestretch -1
        }

        // *****
        // condition change to transToOrbiting again
        if( iAmInsideThisVoid_allInfo.shakeIt_displacingBegin == true )
        {
            qtype3_state = Qtype3State.transToOrbiting;
            orbit_section_begin = true;
        }
        // condition change to frozen again
        else if( orbit_section_phase <= 0 )
        {
            qtype3_state = Qtype3State.frozen;
            // without these, would immediately spring back up to previous value of aud_ampLocal upon return to frozen
            // return to silence
            aud_ampLocal = 0;
            orbit_userLetGoTooSoon_sendFadeOut = false;
            Frozen_NewQuietAmp();
            NewFolderAndBufferHandling_WoodDarkFrozen();
        }
        else
        {
            // *****
            // continuing this state...
                // orbit_section_deltaTime = Time.time - orbit_section_startTime;
            orbit_transition_clock -= Time.deltaTime; // <-- the key to reversing this: now we subtract the delta from the clock
                // phase runs in reverse 1 - 0.
            orbit_section_phase = Mathf.Clamp( orbit_transition_clock / orbit_section_duration, 0f, 1f );
            // Debug.Log("phase" + orbit_section_phase);

            if( orbit_section_phase >= orbit_transition_filament_reachYscaleMaxAtPhaseVal )
            {
                // goes up
                orbit_transition_filament_YscaleCurrent = Scale( orbit_section_phase, orbit_userLetGoTooSoon_phaseValWhenLetGo, orbit_transition_filament_reachYscaleMaxAtPhaseVal, orbit_transition_filament_YscaleBegin, orbit_transition_filament_YscaleMax );
                // Debug.Log("yscale up " + orbit_transition_filament_YscaleCurrent);
            }
            else
            {
                // goes down
                orbit_transition_filament_YscaleCurrent = Scale( orbit_section_phase, orbit_transition_filament_reachYscaleMaxAtPhaseVal, 0f, orbit_transition_filament_YscaleMax, 0f);
                // Debug.Log("yscale down " + orbit_transition_filament_YscaleCurrent);
            }

            filament_current_go.transform.localScale = new Vector3( filament_current_go.transform.localScale.x, orbit_transition_filament_YscaleCurrent, filament_current_go.transform.localScale.z );
            filament_current_renderer.material.color = Color.Lerp( qtype3_colorFrozen, qtype3_colorOrbiting, orbit_section_phase );

            // AUDIO - no xfade, just fade out ampLocal when phase is approaching 0
            if( orbit_section_phase <= mx_orbit_userLetGoTooSoon_xfadeToFrozen_phaseRange[ 0 ] )
            {
                orbit_userLetGoTooSoon_sendFadeOut = true;
                aud_ampLocal = Scale( orbit_section_phase, mx_orbit_userLetGoTooSoon_xfadeToFrozen_phaseRange[ 0 ], mx_orbit_userLetGoTooSoon_xfadeToFrozen_phaseRange[ 1 ], 1f, 0f );
            }
        }
    }

    void Filament_UserLetGoTooSoon_Growth()
    {
        // where we begin from
        orbit_transition_filament_YscaleBegin = filament_current_go.transform.localScale.y;
        // how high to go
            // keep the same height target
        if( orbit_section_phase >= orbit_transition_filament_reachYscaleMaxAtPhaseVal )
        {
            // just add a little to the previous height target
            orbit_transition_filament_YscaleMax += Random.Range( .02f, .1f );
        }
        else if( orbit_section_phase < .1f )
        {
            // else we want to make sure it doesn't go that high at all - would look silly
            orbit_transition_filament_YscaleMax = orbit_transition_filament_YscaleBegin + Random.Range( .005f, .01f );
        }
        else
        {
            // just add a little to the current height
            orbit_transition_filament_YscaleMax = orbit_transition_filament_YscaleBegin + Random.Range( .1f, .2f );
        }
        // when to arrive at that height
            // the phase here is something like e.g. .35 to 0.
            // so our yscaleMax target phase time target must be guaranteed to be a moment in the remaining phase back to 0.
            // the beginning phase value is also the delta to 0., so we'll take a proportion of that delta for min and max random values
        orbit_userLetGoTooSoon_phaseValWhenLetGo = orbit_section_phase;
        orbit_transition_filament_reachYscaleMaxAtPhaseVal = Random.Range( .2f * orbit_userLetGoTooSoon_phaseValWhenLetGo, .8f * orbit_userLetGoTooSoon_phaseValWhenLetGo);
    }

    void Orbit_Orbiting()
    {
        // round n round the qbits go

        // *****
        // begin state...
        if( orbit_section_begin == true )
        {
            // physics
            Filaments_DisableAll();
            self_renderer.enabled = true;
            self_renderer.material.color = qtype3_colorOrbiting;
            orbiting_trail.enabled = true;

            self_insideVoidQbit_cn = new InsideVoidQbit_Cn { id = self_id, qtype = qtype, position = transform.position, orbiting = true };
            for ( int i = 0; i < iAmInsideThisVoid_allInfo.insideVoidQbits_allInfo.Count; i++ )
            {
                if( self_id == iAmInsideThisVoid_allInfo.insideVoidQbits_allInfo[i].id )
                {
                    iAmInsideThisVoid_allInfo.insideVoidQbits_allInfo[i] = self_insideVoidQbit_cn;
                    break;
                }
            }

            orbit_width    = Random.Range( orbit_widthRange[0], orbit_widthRange[1] );
            orbit_height   = Random.Range( orbit_heightRange[0], orbit_heightRange[1] );
            orbit_rotationInitial = Random.Range( 0f, 360f ); // the trappedInsideVoidMovement will thereafter constantly rotate the orbit
            orbitingParentGO = Instantiate( orbitingParentPrefab, this.transform.position, Quaternion.identity );
            this.transform.SetParent( orbitingParentGO.transform );
            qbitsContainerGO = GameObject.Find( "qbitsContainer" );
            orbitingParentGO.transform.SetParent( qbitsContainerGO.transform );
            orbitingParentGO.transform.position = iAmInsideThisVoid_allInfo.centroid;
            orbitingParentGO.transform.Rotate( new Vector3( 0, orbit_rotationInitial, 0 ), Space.World );

            // audio
            int randInt = RandomWeighted( mx_orbit_orbiting_probShortLong );
            switch( randInt )
            {
                case 0: orbit_section_duration = Random.Range( mx_orbit_orbiting_durRangeShort[0], mx_orbit_orbiting_durRangeShort[1] ); break;
                case 1: orbit_section_duration = Random.Range( mx_orbit_orbiting_durRangeLong[0], mx_orbit_orbiting_durRangeLong[1] ); break;
            }

            aud_samplePlayer_xfade = 1 - aud_samplePlayer_xfade;
            // xfade is a proportion of the orbit_section_duration
            aud_samplePlayer_xfadeDur = orbit_section_duration * 1000 * Random.Range( mx_orbit_transitionXfadeOut_proportionOfTotalOrbitingDurRange[0], mx_orbit_transitionXfadeOut_proportionOfTotalOrbitingDurRange[1] );

            int randBufferIndex = Random.Range( 1, sampleInfo_JSONParsed[ "orbiting_tube" + orbit_transition_folderIndex + "/count" ] + 1 );
            aud_bufferName = mx_orbit_orbiting_bufferPrefix + orbit_transition_folderIndex + "." + randBufferIndex;
            aud_collName   = "tube" + orbit_transition_folderIndex + "." + randBufferIndex;

            transpLocal_startPt   = 1f;
            transpLocal_dest      = Random.Range( mx_orbit_orbiting_transpLocal_destRange[0], mx_orbit_orbiting_transpLocal_destRange[1] );
            transpLocal_duration  = Random.Range( mx_orbit_orbiting_transpLocal_durRange[0], mx_orbit_orbiting_transpLocal_durRange[1] );
            transpLocal_startTime = Time.time;

            orbit_orbitingXfadeOut_atDeltaTime = orbit_section_duration - ( orbit_section_duration * Random.Range( mx_orbit_orbitingXfadeOut_proportionOfTotalOrbitingDurRange[0], mx_orbit_orbitingXfadeOut_proportionOfTotalOrbitingDurRange[1] ) );

            orbit_section_startTime = Time.time;
            orbit_section_begin = false;
            orbit_section_beginReportOsc = true;
        }

        orbit_section_deltaTime = Time.time - orbit_section_startTime;
        orbit_section_phase = orbit_section_deltaTime / orbit_section_duration;

        Orbiting_TranspLocal();

        // *****
        // orbiting sound and transToFrozen sound xfade as a proportion of the orbiting dur
        // though...this "sounds and looks like it belongs" to the transToFrozen sections
        if( orbit_section_deltaTime >= orbit_orbitingXfadeOut_atDeltaTime && orbit_section_deltaTimePrev <= orbit_orbitingXfadeOut_atDeltaTime )
        {
            orbit_orbitingXfadeOut_begin = true;
            aud_samplePlayer_xfade = 1 - aud_samplePlayer_xfade;
            aud_samplePlayer_xfadeDur = ( orbit_section_duration - orbit_orbitingXfadeOut_atDeltaTime ) * 1000;
            // Debug.Log( "transToFrozen " + aud_samplePlayer_xfadeDur ); //todo test phrase
            TransToFrozen_GetSameTransitionBuffer();
            aud_bufferPlayPos = orbit_trans_durInBuffer_brightest; // <-- set in Orbit_TransToOrbiting()
            // Debug.Log( "transToFrozen " + aud_bufferName + " " + aud_bufferTransposition ); //todo test phrase
        }

        // *****
        // condition change to transToFrozen
        if( orbit_section_phase >= 1f )
        {
            // Debug.Log( "transToFrozen" ); //todo test phrase
            qtype3_state = Qtype3State.transToFrozen;
            orbit_section_begin = true;
        }

        // movement handled by TrappedInsideVoidMovement() and FixedUpdate()
    }

    void Orbiting_TranspLocal()
    {
        if( transpLocal_deltaTime > transpLocal_duration )
        {
            transpLocal_startPt   = transpLocal_dest;
            transpLocal_dest      = Random.Range( mx_orbit_orbiting_transpLocal_destRange[0], mx_orbit_orbiting_transpLocal_destRange[1] );
            transpLocal_duration  = Random.Range( mx_orbit_orbiting_transpLocal_durRange[0], mx_orbit_orbiting_transpLocal_durRange[1] );
            transpLocal_startTime = Time.time;
        }

        transpLocal_deltaTime = Time.time - transpLocal_startTime;
        transpLocal_phase = transpLocal_deltaTime / transpLocal_duration;
        aud_transpLocal = Mathf.Lerp( transpLocal_startPt, transpLocal_dest, transpLocal_phase );
    }

    void TransToFrozen_GetSameTransitionBuffer()
    {
        // same orbit_transition_folderIndex, new random buffer in that folder; also same transposition
        int numSamples = sampleInfo_JSONParsed[ mx_orbit_transition_bufferPrefix + orbit_transition_folderIndex + "_" + orbit_transition_shortMidLong_string + "/count" ];
        int bufferIndex = Random.Range( 1, numSamples + 1 );
        aud_bufferName = mx_orbit_transition_bufferPrefix + orbit_transition_folderIndex + "_" + orbit_transition_shortMidLong_string + "." + bufferIndex;
    }

    void Orbit_TransToFrozen()
    {
        // orbiting over, transition back to frozen

        // *****
        // begin state...
        if( orbit_section_begin == true )
        {
            orbit_section_startTime = Time.time;

            self_renderer.enabled = false;
            transform.localScale = Vector3.one * qtype3_beginningScale;
            NewFilament();

            orbit_section_duration = orbit_trans_durInBuffer_brightest / 1000;
            orbit_transition_filament_reachYscaleMaxAtPhaseVal = Random.Range( orbit_transition_filament_reachYscaleMaxAtPhaseVal_range[0], orbit_transition_filament_reachYscaleMaxAtPhaseVal_range[1] );
            orbit_section_begin = false;
        }

        // *****
        // continuing this state...
        orbit_section_deltaTime = Time.time - orbit_section_startTime;
        orbit_section_phase = orbit_section_deltaTime / orbit_section_duration;

        if( orbit_section_phase <= orbit_transition_filament_reachYscaleMaxAtPhaseVal )
        {
            // goes up
            orbit_transition_filament_YscaleCurrent = Scale( orbit_section_phase, 0f, orbit_transition_filament_reachYscaleMaxAtPhaseVal, 0, orbit_transition_filament_YscaleMax );
        }
        else
        {
            // goes down
            orbit_transition_filament_YscaleCurrent = Scale( orbit_section_phase, orbit_transition_filament_reachYscaleMaxAtPhaseVal, 1f, orbit_transition_filament_YscaleMax, 0f );
        }
        filament_current_go.transform.localScale = new Vector3( filament_current_go.transform.localScale.x, orbit_transition_filament_YscaleCurrent, filament_current_go.transform.localScale.z );
        filament_current_renderer.material.color = Color.Lerp( qtype3_colorOrbiting, qtype3_colorFrozen, orbit_section_phase );

        if( orbit_section_phase >= .4f )
        {
            // no xfade of transition buffer to frozen buffer - just fade out the transition file
            aud_ampLocal = 1f - orbit_section_phase;
            orbit_transToFrozenFadeOut = true;
        }

        if( orbit_section_phase >= 1f )
        {
            qtype3_state = Qtype3State.frozen;
            // without these, would immediately spring back up to previous value of aud_ampLocal upon return to frozen
            // return to silence
            aud_ampLocal = 0;
            orbit_transToFrozenFadeOut = false;
            orbit_userLetGoTooSoon_sendFadeOut = false;
            orbiting_trail.enabled = false;
            Frozen_NewQuietAmp();
            NewFolderAndBufferHandling_WoodDarkFrozen();

            self_insideVoidQbit_cn = new InsideVoidQbit_Cn { id = self_id, qtype = qtype, position = transform.position, orbiting = false };
            for( int i = 0; i < iAmInsideThisVoid_allInfo.insideVoidQbits_allInfo.Count; i++ )
            {
                if( self_id == iAmInsideThisVoid_allInfo.insideVoidQbits_allInfo[i].id )
                {
                    iAmInsideThisVoid_allInfo.insideVoidQbits_allInfo[i] = self_insideVoidQbit_cn;
                    break;
                }
            }

            this.transform.SetParent( qbitsContainerGO.transform );
            Destroy( orbitingParentGO );
        }
    }

    //****************************************
    // coroutines
    IEnumerator NewBrownianRollyPollyTransposition()
    {
        Vector2 transpositionRange = swarmParams_script.qbit_qtype1Brownian_transpositionRange;
        float randomValue = Random.Range( 0.0f, 1.0f );
        int randomSeconds;
        if( randomValue > 0.0f && randomValue <= .03f )
        {
            randomSeconds = 0;
        }
        else if( randomValue > 0.03f && randomValue <= .1f )
        {
            randomSeconds = 1;
        }
        else if( randomValue > 0.1f && randomValue <= .2f )
        {
            randomSeconds = 2;
        }
        else if (randomValue > 0.2f && randomValue <= .35f )
        {
            randomSeconds = 3;
        }
        else if (randomValue > 0.35f && randomValue <= .55f )
        {
            randomSeconds = 4;
        }
        else if( randomValue > 0.55f && randomValue <= .8f )
        {
            randomSeconds = 5;
        }
        else
        {
            randomSeconds = 6;
        }
        yield return new WaitForSeconds( randomSeconds );
        aud_transpGlobal = Random.Range( transpositionRange[0], transpositionRange[1] );
    }

    IEnumerator Frozen_BeginQtype3()
    {
        // qtype3s change the GlobalEvolutionState
        // if this qbit itself causes the change in evolution state, we must delay long enough for this qbit to get counted by the
        // globalEvolution_script before this qbit goes to get its frozen folder and buffer
        // hence this coroutine
        yield return null; // <-- wait for 1 frame

        // Debug.Log(globalEvolutionState);
        aud_beginQtype3 = true;
        aud_ampLocal = 1;
        frozen_direction = "get quiet";
        frozen_theFirstDecrescendo = true;
        Frozen_NewQuietAmp();

        // folder stays same for state "begin"; wood_dark_legato is just one folder
        // on aud_begin_qtype3 == true, it always runs New_Folder() and New_Buffer(), but just call the check to set the counters to 0 too
        NewFolderAndBufferHandling_WoodDarkFrozen();
    }

    //****************************************
    // mixer vals
    void MixerValues_Init()
    {
        mx_velocitymagAsAmplitude_multiplier                          = mixer.qbit_velocitymagAsAmplitude_multiplier;
        mx_qtype0_bufferNamePrefix                                    = mixer.qbit_qtype0_bufferNamePrefix;
        mx_qtype0_bufferPlayPos                                       = mixer.qbit_qtype0_bufferPlayPos;
        mx_qtype0_ampGlobal                                           = mixer.qbit_qtype0_ampGlobal;
        mx_qtype0_transposition                                       = mixer.qbit_qtype0_transposition;
        mx_qtype1Repel_bufferName                                     = mixer.qbit_qtype1Repel_bufferName;
        mx_qtype1Repel_ampGlobal                                      = mixer.qbit_qtype1Repel_ampGlobal;
        mx_qtype1Brownian_ampGlobal_begin                             = mixer.qbit_qtype1Brownian_ampGlobal_begin;
        mx_qtype1Brownian_ampGlobal_lowerAndRougher                   = mixer.qbit_qtype1Brownian_ampGlobal_lowerAndRougher;
        mx_qtype2_bufferNamePrefix                                    = mixer.qbit_qtype2_bufferNamePrefix;
        mx_qtype2_bufferPlayPos                                       = mixer.qbit_qtype2_bufferPlayPos;
        mx_qtype2_ampGlobal                                           = mixer.qbit_qtype2_ampGlobal;
        mx_qtype2_transposition                                       = mixer.qbit_qtype2_transposition;
        mx_frozen_sameBufferCountMax                                  = mixer.qbit_frozen_sameBufferCountMax;
        mx_frozen_bufferPrefix_woodDark                               = mixer.qbit_frozen_bufferPrefix_woodDark;
        mx_frozen_bufferPrefix_tubesLow                               = mixer.qbit_frozen_bufferPrefix_tubesLow;
        mx_frozen_bufferTranspositionRange_woodDark                   = mixer.qbit_frozen_bufferTranspositionRange_woodDark;
        mx_frozen_bufferTranspositionRange_tubesLow                   = mixer.qbit_frozen_bufferTranspositionRange_tubesLow;
        mx_frozen_ampGlobal_begin                                     = mixer.qbit_frozen_ampGlobal_begin;
        mx_frozen_ampGlobal_lowerAndRougher                           = mixer.qbit_frozen_ampGlobal_lowerAndRougher;
        mx_frozen_localAmpLouds_begin                                 = mixer.qbit_frozen_localAmpLouds_begin;
        mx_frozen_localAmpQuiets                                      = mixer.qbit_frozen_localAmpQuiets;
        mx_frozen_localAmpLouds_weightedProbs_beginSpin               = mixer.qbit_frozen_localAmpLouds_weightedProbs_beginSpin;
        mx_filament_YscaleMax_begin                                   = mixer.qbit_filament_YscaleMax_begin;
        mx_filament_YscaleMax_tallerTubes                             = mixer.qbit_filament_YscaleMax_tallerTubes;
        mx_filament_YscaleMax_beginSpin                               = mixer.qbit_filament_YscaleMax_beginSpin;
        mx_orbit_transToOrbiting_ampGlobal_begin                      = mixer.qbit_orbit_transToOrbiting_ampGlobal_begin;
        mx_orbit_transToOrbiting_ampGlobal_lowerAndRougher            = mixer.qbit_orbit_transToOrbiting_ampGlobal_lowerAndRougher;
        mx_orbit_transToOrbiting_sameTubeCountMaxRange_begin          = mixer.qbit_orbit_transToOrbiting_sameTubeCountMaxRange_begin;
        mx_orbit_transToOrbiting_sameTubeCountMaxRange_beginCeiling   = mixer.qbit_orbit_transToOrbiting_sameTubeCountMaxRange_beginCeiling;
        mx_orbit_transToOrbiting_sameTubeCountMaxRange_tallerTubes    = mixer.qbit_orbit_transToOrbiting_sameTubeCountMaxRange_tallerTubes;
        mx_orbit_orbiting_ampGlobal_begin                             = mixer.qbit_orbit_orbiting_ampGlobal_begin;
        mx_orbit_orbiting_ampGlobal_lowerAndRougher                   = mixer.qbit_orbit_orbiting_ampGlobal_lowerAndRougher;
        mx_orbit_orbiting_bufferPrefix                                = mixer.qbit_orbit_orbiting_bufferPrefix;
        mx_orbit_transition_bufferPrefix                              = mixer.qbit_orbit_transition_bufferPrefix;
        mx_orbit_transToOrbiting_probShortMidLong                     = mixer.qbit_orbit_transToOrbiting_probShortMidLong;
        mx_orbit_frozen_and_transToOrbiting_xfadeDur                  = mixer.qbit_orbit_frozen_and_transToOrbiting_xfadeDur;
        mx_orbit_userLetGoTooSoon_xfadeToFrozen_phaseRange            = mixer.qbit_orbit_userLetGoTooSoon_xfadeToFrozen_phaseRange;
        mx_orbit_orbiting_probShortLong                               = mixer.qbit_orbit_orbiting_probShortLong;
        mx_orbit_orbiting_durRangeShort                               = mixer.qbit_orbit_orbiting_durRangeShort;
        mx_orbit_orbiting_durRangeLong                                = mixer.qbit_orbit_orbiting_durRangeLong;
        mx_orbit_orbiting_transpLocal_destRange                       = mixer.qbit_orbit_orbiting_transpLocal_destRange;
        mx_orbit_orbiting_transpLocal_durRange                        = mixer.qbit_orbit_orbiting_transpLocal_durRange;
        mx_orbit_transToFrozen_ampGlobal                              = mixer.qbit_orbit_transToFrozen_ampGlobal;
        mx_orbit_transitionXfadeOut_proportionOfTotalOrbitingDurRange = mixer.qbit_orbit_transitionXfadeOut_proportionOfTotalOrbitingDurRange;
        mx_orbit_orbitingXfadeOut_proportionOfTotalOrbitingDurRange   = mixer.qbit_orbit_orbitingXfadeOut_proportionOfTotalOrbitingDurRange;
    }

    //****************************************
    // ><>  ><>  ><>  ><>
    void EvolutionParams()
    {
        if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.begin || globalEvolutionState == GlobalEvolution.GlobalEvolutionState.beginCeiling )
        {
            frozen_localAmpLouds = mx_frozen_localAmpLouds_begin;
            filament_currGlobalEvolution_YscaleMax = mx_filament_YscaleMax_begin;
        }

        if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.begin )
        {
            orbit_transToOrbiting_sameTubeCountMaxRange = mx_orbit_transToOrbiting_sameTubeCountMaxRange_begin;
        }
        else if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.beginCeiling )
        {
            orbit_transToOrbiting_sameTubeCountMaxRange = mx_orbit_transToOrbiting_sameTubeCountMaxRange_beginCeiling;
        }
        else if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.tallerTubes )
        {
            filament_currGlobalEvolution_YscaleMax = mx_filament_YscaleMax_tallerTubes;
            orbit_transToOrbiting_sameTubeCountMaxRange = mx_orbit_transToOrbiting_sameTubeCountMaxRange_tallerTubes;
        }
        else if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.beginSpin )
        {
            // DESIGN: the radius of the qbit tree collision influence is determined by the radius of the capsule collider on the tree ( each filament on the qbit prefab ) 
            filament_currGlobalEvolution_YscaleMax = mx_filament_YscaleMax_beginSpin;
        }

        if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.beginSpin )
        {
            // weighted prob so that tubes more likely to be the loudest amp, i.e. touch the ceiling
            del_Frozen_NewLoudAmp = Frozen_NewLoudAmp_BeginSpin;
        }
        else
        {
            del_Frozen_NewLoudAmp = Frozen_NewLoudAmp_Unweighted;
        }

        if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.tallerTubes || globalEvolutionState == GlobalEvolution.GlobalEvolutionState.beginSpin )
        {
            filament_currGlobalEvolution_XZscale = filament_XZscale_thinner;
        }
        else
        {
            filament_currGlobalEvolution_XZscale = filament_XZscale_thicker;
        }

        if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.lowerAndRougher )
        {
            aud_qtype1Brownian_ampGlobal = mx_qtype1Brownian_ampGlobal_lowerAndRougher; 
            aud_frozen_ampGlobal = mx_frozen_ampGlobal_lowerAndRougher;
            aud_orbit_orbiting_ampGlobal = mx_orbit_orbiting_ampGlobal_lowerAndRougher;
            aud_orbit_transToOrbiting_ampGlobal = mx_orbit_transToOrbiting_ampGlobal_lowerAndRougher;
        }
        else
        {
            aud_qtype1Brownian_ampGlobal = mx_qtype1Brownian_ampGlobal_begin;
            aud_frozen_ampGlobal = mx_frozen_ampGlobal_begin;
            aud_orbit_orbiting_ampGlobal = mx_orbit_orbiting_ampGlobal_begin;
            aud_orbit_transToOrbiting_ampGlobal = mx_orbit_transToOrbiting_ampGlobal_begin;
        }
    }

    //****************************************
    // etc
    void NullifyMouseHitPoint()
    {
        // this is so that prev mouseclick doesn't sit around and re-effect a particle when we don't want it to
        // can't nullify a vector3 so we throw the hitpoint outside the scene space
        hitpoint = new Vector3( 15, 15, 15 );
        // mouseRaycast.hitpoint = new Vector3( 15, 15, 15 );
    }

    public float Scale( float oldValue, float oldMin, float oldMax, float newMin, float newMax )
    {

        float oldRange = oldMax - oldMin;
        float newRange = newMax - newMin;
        float newValue = (((oldValue - oldMin) * newRange) / oldRange) + newMin;

        return newValue;
    }

    int RandomWeighted( List<int> weights )
    {
        // weights are a list of integers
        // returns a random index associated with the weight
        // 0 indexed

        int sumWeights = new int();
        int selectedIndex = new int();
        foreach( int weight in weights )
        {
            sumWeights += weight;
        }

        float randWeight = Random.Range( 1, sumWeights );

        for( int i = 0; i < weights.Count; i++ )
        {
            randWeight -= weights[i];
            if( randWeight <= 0 )
            {
                selectedIndex = i;
                break;
            }
        }

        return selectedIndex;
    }


    void OnDrawGizmos()
    {
        /*
        if( iAmABoundingQbit == true)
        {
            Gizmos.color = Color.cyan;
            if( thisIsMy_qbit_hullover_allInfo.expandToMakeConvex == true )
            {
                Gizmos.color = Color.cyan;
            }
            else if( thisIsMy_qbit_hullover_allInfo.expandToMakeConvex == false )
            {
                Gizmos.color = Color.magenta;
            }
            Gizmos.DrawSphere(self_position, .08f);
        }*/

        /*
        if( kickedOutOfAVoid_underway == true )
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(self_position, .06f);
        }

        if( moveBackToInitialPos_moving == true )
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(self_position, .08f);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(initialPosition, .04f);
        }*/

        /*
        if( repelling == true )
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(self_position, .06f);
        }
        else
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(self_position, .06f);
        }*/

        /*
        if (selectedGO != null)
        {
            if (selectedGO.name == this.transform.name)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(initialPosition, .04f);
            }
        }*/

        /*
        else if (moveBackToInitialPos_moving == false)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(self_position, .08f);
        }*/

        /*
        if (repelling == true)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, .1f);
        }*/
        if (iAmABoundingQbit == true)
        {
            if( iAmConvexWhileOpeningTest == false )
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(transform.position, .1f);
            }
        }


        if(qtype==3)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(transform.position, .1f);
        }
    }

}