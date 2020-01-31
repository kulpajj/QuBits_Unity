using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class CameraExplorerParent_Movement : MonoBehaviour 
{
    // osc
    OscOut oscOut_script;
    OscIn  oscIn_script;

    // global evolution
    GlobalEvolution globalEvolution_script;
    GlobalEvolution.GlobalEvolutionState globalEvolutionState;

    // mixer
    Mixer mixer;

    // mixer values
    float   mx_upsideDown_ampGlobal_begin;
    float   mx_upsideDown_ampGlobal_lowerAndRougher;
    float   mx_upsideDown_flange_rate;
    float   mx_upsideDown_flange_depth;
    float   mx_upsideDown_distantOrClose_distFromCenter;
    Vector2 mx_upsideDown_distant_stutter_repeatprobRange;
    float   mx_upsideDown_distant_stutter_shiftamt;
    float   mx_upsideDown_distant_stutter_feedback;
    float   mx_upsideDown_distant_stutter_dropoutprob;
    float   mx_upsideDown_distant_reverb_roomsize;
    float   mx_upsideDown_distant_reverb_decay;
    float   mx_upsideDown_distant_reverb_damping;
    float   mx_upsideDown_distant_reverb_diffusion;
    float   mx_upsideDown_distant_filter_freq;
    Vector2 mx_upsideDown_close_stutter_repeatprobRange;
    float   mx_upsideDown_close_stutter_shiftamt;
    float   mx_upsideDown_close_stutter_feedback;
    float   mx_upsideDown_close_stutter_dropoutprob;
    Vector2 mx_upsideDown_close_2reverbs_roomsize;
    Vector2 mx_upsideDown_close_2reverbs_decay;
    Vector2 mx_upsideDown_close_2reverbs_damping;
    Vector2 mx_upsideDown_close_2reverbs_diffusion;
    float   mx_upsideDown_close_filter_freq;
    Vector2 mx_upsideDown_transpRange;
    Vector2 mx_upsideDown_rotation_velocityY_scaledToTranspDeltaRange;
    Vector2 mx_upsideDown_downsamp_intervalRange;
    float   mx_upsideDown_distToVoidCentroid_delayOn;
    float   mx_upsideDown_delay_ampGlobal;
    float   mx_upsideDown_delay_feedbackMultMin;
    Vector2 mx_upsideDown_delay_feedbackMultMaxRangeLow;
    Vector2 mx_upsideDown_delay_feedbackMultMaxRangeHigh;
    float   mx_upsideDown_delay_feedbackMult_probRangeLow;
    float   mx_upsideDown_delay_feedbackMult_ampLimiterThresh;
    float   mx_upsideDown_bg_ampGlobal;
    float   mx_rightsideUp_distToVoidCentroid_suck;
    float   mx_rightsideUp_distToVoidCentroid_distort;
    float   mx_rightsideUp_approachingVoid_stutter_shiftamt_maxDeviateFrom1;
    Vector2 mx_rightsideUp_approachingVoid_stutter_feedbackRange;
    Vector2 mx_rightsideUp_approachingVoid_stutter_repeatprobRange;
    Vector2 mx_rightsideUp_approachingVoid_reverb_roomsizeRange;
    Vector2 mx_rightsideUp_approachingVoid_reverb_decayRange;
    Vector2 mx_rightsideUp_approachingVoid_reverb_dampingRange;
    Vector2 mx_rightsideUp_approachingVoid_reverb_diffusionRange;

    // other max values
    float aud_upsideDown_ampGlobal;
    float aud_upsideDown_stutter_shiftamt;
    float aud_upsideDown_stutter_feedback;
    float aud_upsideDown_stutter_dropoutprob;
    float aud_upsideDown_reverb_roomsize;
    float aud_upsideDown_reverb_decay;
    float aud_upsideDown_reverb_damping;
    float aud_upsideDown_reverb_diffusion;
    float aud_upsideDown_filter_freq;
    float aud_upsideDown_transp;
    bool  aud_upsideDown_downsamp_begin; // <-- when camera rotating on y-axis ( left/right )
    bool  aud_upsideDown_downsamp_end;
    float aud_upsideDown_downsamp_interval;
    bool  aud_upsideDown_delayOn_click;
    bool  aud_upsideDown_delayOff_click;
    bool  aud_rightsideUp_approachingVoid_onClick;
    bool  aud_rightsideUp_approachingVoid_offClick;
    float aud_rightsideUp_approachingVoid_stutter_shiftamt;
    float aud_rightsideUp_approachingVoid_stutter_feedback;
    float aud_rightsideUp_approachingVoid_reverb_roomsize;
    float aud_rightsideUp_approachingVoid_reverb_decay;
    float aud_rightsideUp_approachingVoid_reverb_damping;
    float aud_rightsideUp_approachingVoid_reverb_diffusion;

    Camera  cameraExplorer;
    Vector3 cameraExplorer_position;
    Vector3 cameraExplorer_rotation;
    Grayscale grayscale;
    Twirl twirl;
    Vortex vortex;
    VignetteAndChromaticAberration blur;
    DirectionalLightMovement directionalLightMovement_script;

    Transform  playArea_bottom_transform;
    Transform  playArea_top_transform;
    Transform  playArea_side_stageFront_transform;
    Transform  playArea_side_stageLeft_transform;
    Transform  playArea_side_stageRight_transform;
    Transform  playArea_side_stageBack_transform;
    GameObject playArea_topGO;
    float      playArea_top_size;
    GameObject floorGO;

    bool beyondPlayArea_bottom_thresh;
    bool beyondPlayArea_top_thresh;
    bool beyondPlayArea_side_stageFront_thresh;
    bool beyondPlayArea_side_stageLeft_thresh;
    bool beyondPlayArea_side_stageRight_thresh;
    bool beyondPlayArea_side_stageBack_thresh;

    // stay in bounds:
    float posX_clamped;
    float posY_clamped;
    float posZ_clamped;
    Ray          ray_toMouse;
    RaycastHit[] ray_toMouse_hits;
    float        ray_toMouse_maxDist = 3f;
    float        zoom_stopAtDistToPlayAreaBoundary = .9f;
    Vector3      moveToPoint_zoom;
    string       forwardLooking_playAreaBoundary;
    string       camera_playAreaboundaryGO_forwardDir;
    bool         outOfbounds;

    bool    rotating; // <-- 2-finger dragging
    bool    rotating_yaxis; // <-- we need to differentiate this from x-axis rotation ( up /down ), so that up/down doesn't cause downsampling changes in pitch all the time
    float   rotation_velocityThresh_begin = .1f; // <-- todo delete
    float   rotation_velocityThresh_endPhysics    = .04f;
    float   rotation_velocityThresh_endDownsamp = .2f;
    Vector3 rotation_velocityVector;
    Vector3 rotation_velocityVectorPrev;
    float   rotation_velocityY_clamped;
    float   rotation_velocityY_clampedPrev;
    float   rotation_velocityY_scaledToTranspDelta;
    float   rotation_Xscaler = 1.3f;
    float   rotation_Yscaler = 1.8f;
    float   rotation_friction = .9f;
    float   rotation_paddingToDetectPlayArea = .2f;
    float   headLooksAround_stepsize = 1f;
    float   mouseScrollWheel;
    float   zoom_stepSize;

    // the Upside Down
        // check if inside any void + need to know distance to each centroid for delay with feedback 
    VoidsAllInfo  voidsAllInfo_script;
    List<Void_Cn> voidsAllInfo;
    bool          iAmInsideThisVoid_test;
        // etc
    bool          upsideDown_change;
    public bool   aud_upsideDown; // <--- upsideDown or not
    Vector3       upsideDown_startPoint;
        // we'll switch certain audio settings in the upsideDown based on camera distance to floor center 
        // - when the zoomcrosses the distFromCenter_onOff threshold
    float         cameraDistToFloorCenter;
    float         cameraDistToFloorCenterPrev;
    bool          upsideDown_distant;
    bool          upsideDown_distantOrClose_change;
    bool          upsideDown_delayOn; //<-- vs not
    bool          upsideDown_delayOnPrev;
    float         upsideDown_delay_feedbackMult;
    float         upsideDown_delay_feedbackMultMax;
    bool          upsideDown_delay_feedbackLimited; // <-- reported by max when amp is >~ mx_upsideDown_delay_feedbackMult_ampLimiterThresh 
    bool          upsideDown_reportFeedbackMult;
    bool          upsideDown_pushExplorerAwayFromVoid_lerping;
    bool          upsideDown_pushExplorerAwayFromVoid_lerpingPrev;
    float         upsideDown_pushExplorerAwayFromVoid_totalDistToDelayOnThresh;
    float         upsideDown_pushExplorerAwayFromVoid_deltaDistToDelayOnThresh;
    float         upsideDown_pushExplorerAwayFromVoid_phase;
    Ray           ray_toMouseReverse;
    RaycastHit[]  ray_toMouseReverseHits;
    Vector3       upsideDown_pushExplorerAwayFromVoid_destinationPt;
    bool          upsideDown_beginFromSuckedIn;
    float         upsideDown_beginFromSuckedIn_startTime;
    float         upsideDown_beginFromSuckedIn_duration = .7f;
    float         upsideDown_beginFromSuckedIn_deltaTime;
    float         upsideDown_beginFromSuckedIn_phase;
    bool          rightsideUp_suckingExplorerIntoVoid;
    bool          rightsideUp_suckingExplorerIntoVoidPrev;
    float         rightsideUp_suckingExplorerIntoVoid_stepSize = .1f;
    bool          rightsideUp_approachingVoid;
    bool          rightsideUp_approachingVoidPrev;
    float         rightsideUp_approachingVoid_stutter_shiftamt_devFrom1; 
    float         explorer_distToClosestVoidCentroid;
    Vector3       explorer_closestVoidCentroid;

    float   directionalLight_rotation_x_euler;
    Vector2 directionalLight_rotation_x_eulerRange;

    // debugging:
    public float gizscale1 = .05f;
    public float gizscale2 = .05f;
    public float gizscale3 = .05f;
    public float gizscale4 = .05f;


    void OnEnable()
    {
        IT_Gesture.onDraggingE += OnDragging;
        IT_Gesture.onDraggingEndE += OnDraggingEnd;
        IT_Gesture.onMFDraggingE += OnMFDragging;
    }

    void OnDisable()
    {
        IT_Gesture.onDraggingE -= OnDragging;
        IT_Gesture.onDraggingEndE -= OnDraggingEnd;
        IT_Gesture.onMFDraggingE -= OnMFDragging;
    }

    void Start()
    {
        oscOut_script = GameObject.Find( "osc" ).GetComponent<OscOut>();
        oscIn_script =  GameObject.Find( "osc" ).GetComponent<OscIn>();
        oscIn_script.MapInt( "/upsideDown/delay/feedbackLimited", OscIn_FeedbackLimited );
        globalEvolution_script = GameObject.Find( "globalEvolution" ).GetComponent<GlobalEvolution>();
        globalEvolutionState = GlobalEvolution.GlobalEvolutionState.begin;
        cameraExplorer = GetComponentInChildren<Camera>();
        playArea_bottom_transform          = GameObject.Find( "playArea_bottom" ).transform;
        playArea_top_transform             = GameObject.Find( "playArea_top" ).transform;
        playArea_side_stageFront_transform = GameObject.Find( "playArea_side_stageFront" ).transform;
        playArea_side_stageLeft_transform  = GameObject.Find( "playArea_side_stageLeft" ).transform;
        playArea_side_stageRight_transform = GameObject.Find( "playArea_side_stageRight" ).transform;
        playArea_side_stageBack_transform  = GameObject.Find( "playArea_side_stageBack" ).transform;
        playArea_topGO = GameObject.Find( "playArea_top" );
        playArea_top_size = playArea_topGO.GetComponent<Renderer>().bounds.size.x;
        floorGO = GameObject.Find( "floor" );
        directionalLightMovement_script = GameObject.Find( "directionalLight" ).GetComponent<DirectionalLightMovement>();
        directionalLight_rotation_x_eulerRange = directionalLightMovement_script.aud_rotationChange_eulerRange;

        voidsAllInfo_script = GameObject.Find( "voidsAllInfo" ).GetComponent<VoidsAllInfo>();
        grayscale = transform.GetComponentInChildren<Grayscale>();
        twirl = transform.GetComponentInChildren<Twirl>();
        vortex = transform.GetComponentInChildren<Vortex>();
        blur = transform.GetComponentInChildren<VignetteAndChromaticAberration>();
        grayscale.enabled = false;
        twirl.enabled = false;
        vortex.enabled = false;
        blur.enabled = false;
        blur.blur = .65f;

        mixer = new Mixer();
        MixerValues_Init();

        EvolutionParams();
        ReportOscStart();
    }

    void Update()
    {
        cameraExplorer_position = cameraExplorer.transform.position;
        cameraExplorer_rotation = cameraExplorer.transform.eulerAngles;
        voidsAllInfo = voidsAllInfo_script.voidsAllInfo;
        globalEvolutionState = globalEvolution_script.globalEvolutionState;
        upsideDown_change = false;

        EvolutionParams();

        // real input from mouse or forced zoom for suckedIntoVoid
        MouseZoom_RealOrForced();

        if( mouseScrollWheel != 0 )
        {
            // we can now be in this method for both real mouse input and the forced suckedIntoVoid
            Input_2FingerScrollZoom();
        }
        if( aud_upsideDown == false && voidsAllInfo != null )
        {
            if( voidsAllInfo.Count != 0 )
            {
                RightsideUp_ParamsByZooming();
            }
        }

        if( rotating == true ) // <-- set in method below from IT_Gesture assets package, e.g. OnDraggingE()
        {
            Input_2FingerDragRotate();
        }

        Input_ArrowKeysHeadRotation();

        Check_CurrPos_AtBoundaryOfPlayArea();


        // upsideDown: distant or close on/off, distant or close yshape, and delay with feedback when get close to a void centroid
        if( aud_upsideDown == true )
        {
            UpsideDown_ParamsByZooming();
        }

        ReportOscUpdate();
        rotation_velocityVectorPrev = rotation_velocityVector;
        rotation_velocityY_clampedPrev = rotation_velocityY_clamped;
        upsideDown_pushExplorerAwayFromVoid_lerpingPrev = upsideDown_pushExplorerAwayFromVoid_lerping;
        rightsideUp_approachingVoidPrev = rightsideUp_approachingVoid;
        rightsideUp_suckingExplorerIntoVoidPrev = rightsideUp_suckingExplorerIntoVoid;
    }

    void MouseZoom_RealOrForced()
    {
        mouseScrollWheel = Input.GetAxis( "Mouse ScrollWheel" );

        // real mousescrollwheel and zoom ray
        if( mouseScrollWheel != 0 )
        {
            ray_toMouse = cameraExplorer.ScreenPointToRay( Input.mousePosition );
            rightsideUp_suckingExplorerIntoVoid = false;
            upsideDown_beginFromSuckedIn = false;

            // Debug.DrawRay( ray_toMouse.GetPoint(5), ray_toMouse.GetPoint(20), Color.red, 1f );
        }
        else if( voidsAllInfo != null )
        {
            if( aud_upsideDown == false )
            {
                GetClosestVoidInfo();
                if( explorer_distToClosestVoidCentroid <= mx_rightsideUp_distToVoidCentroid_suck )
                {
                    // forced or faked mousescrollwheel and zoom ray to void centroid so that suckedIntoVoid works correctly
                    mouseScrollWheel = .1f;
                    Vector3 voidCentroidPixel = cameraExplorer.WorldToScreenPoint( explorer_closestVoidCentroid );
                    ray_toMouse = cameraExplorer.ScreenPointToRay( voidCentroidPixel );
                    rightsideUp_suckingExplorerIntoVoid = true;
                }
                else
                {
                    // else the actual mousescrollwheel 0
                    mouseScrollWheel = 0f;
                    rightsideUp_suckingExplorerIntoVoid = false;
                }
            }
            else if( aud_upsideDown == true )
            {
                if( upsideDown_beginFromSuckedIn == true )
                {
                    // forced or faked mousescrollwheel and zoom ray to floor center so that suckedIntoVoid zooms a lil when first enter upsideDown
                    mouseScrollWheel = .1f;
                    Vector3 floorCenterPixel = cameraExplorer.WorldToScreenPoint( floorGO.transform.position );
                    ray_toMouse = cameraExplorer.ScreenPointToRay( floorCenterPixel );

                    upsideDown_beginFromSuckedIn_deltaTime = Time.time - upsideDown_beginFromSuckedIn_startTime;
                    upsideDown_beginFromSuckedIn_phase = upsideDown_beginFromSuckedIn_deltaTime / upsideDown_beginFromSuckedIn_duration;
                    if( upsideDown_beginFromSuckedIn_phase >= 1f )
                    {
                        upsideDown_beginFromSuckedIn = false;
                    }
                }
                else
                {
                    // else the actual mousescrollwheel 0
                    mouseScrollWheel = 0f;
                }
            }
        }
        else
        {
            // no voids open - the actual mousescrollwheel 0
            mouseScrollWheel = 0f;
            rightsideUp_suckingExplorerIntoVoid = false;
        }
    }

    // **********
    // ~~~~~~~~~~
    // types of input gestures
    // ~~~~~~~~~~
    void Input_2FingerScrollZoom()
    {
        ray_toMouse_hits = Physics.RaycastAll( ray_toMouse, 25 );
        moveToPoint_zoom = ray_toMouse.GetPoint( zoom_stopAtDistToPlayAreaBoundary );

        // ***
        // initial checks
        // ***
        Identify_ForwardLookingPlayAreaBoundary();
        Check_MoveToPt_OutOfBounds();
        if( beyondPlayArea_bottom_thresh == true )
        {
            CheckIfCameraInsideAnyVoid(); // <---, i.e. check if upsideDown_change
        }

        // ***
        // MouseScrollWheel - other checks and actual movement
        // ***
        // make sure UpsideDown_Begin() stays in an if / else if relationship with zoom-as-usual or it negates the beginning upsideDown position
        if( upsideDown_change == true )
        {
            UpsideDown_Change();
        }
        else if( upsideDown_pushExplorerAwayFromVoid_lerping == false )                                                                     
        {
            MouseScrollCalibratedTo_CameraOrientationAndOutOfBounds();
            cameraExplorer.transform.position = Vector3.MoveTowards( cameraExplorer_position, moveToPoint_zoom, zoom_stepSize );
        }
    }

    void Input_2FingerDragRotate()
    {
        // rotation speed, friction, and end
        rotation_velocityVector *= rotation_friction;
        this.transform.eulerAngles += rotation_velocityVector;
        Mathf.Clamp( cameraExplorer.transform.eulerAngles.z, 0, 0 );
        Mathf.Clamp( cameraExplorer.transform.eulerAngles.y, 0, 0 );
        Mathf.Clamp( cameraExplorer.transform.eulerAngles.z, 0, 0 );
        if( Mathf.Abs( rotation_velocityVector.y ) <= rotation_velocityThresh_endDownsamp )
        {
            if( rotating_yaxis == true )
            {
                rotating_yaxis = false;
                aud_upsideDown_downsamp_end = true;
            }
        }
        if( Mathf.Abs( rotation_velocityVector.y ) <= rotation_velocityThresh_endPhysics )
        {
            rotating = false;
        }

        // transposition
        // spin clockwise is actually positive and spin counterclockwise is negative, but do transp in reverse of that
        rotation_velocityY_clamped = Mathf.Clamp( rotation_velocityVector.y, -8f, 8f ); // <-- left/right
        rotation_velocityY_scaledToTranspDelta = Scale( rotation_velocityY_clamped, 8f, -8f, mx_upsideDown_rotation_velocityY_scaledToTranspDeltaRange[0], mx_upsideDown_rotation_velocityY_scaledToTranspDeltaRange[1] );
        aud_upsideDown_transp += rotation_velocityY_scaledToTranspDelta;
        aud_upsideDown_transp = Mathf.Clamp( aud_upsideDown_transp, mx_upsideDown_transpRange[0], mx_upsideDown_transpRange[1] );

        // downsampling
        // differentiate between left/right rotation and up/down rotation ( which we don't want to trigger downsamp_begin )
        if( rotating_yaxis == false )
        {
            if( rotation_velocityVector.x == 0 && rotation_velocityVector.y != 0 )
            {
                rotating_yaxis = true;
                aud_upsideDown_downsamp_begin = true;
                aud_upsideDown_downsamp_interval = Random.Range( mx_upsideDown_downsamp_intervalRange[0], mx_upsideDown_downsamp_intervalRange[1] );
            }
        }

        posX_clamped = Mathf.Clamp( cameraExplorer.transform.position.x, playArea_side_stageLeft_transform.position.x + rotation_paddingToDetectPlayArea, playArea_side_stageRight_transform.position.x - rotation_paddingToDetectPlayArea );
        posY_clamped = Mathf.Clamp( cameraExplorer.transform.position.y, playArea_bottom_transform.position.y + rotation_paddingToDetectPlayArea, playArea_top_transform.position.y - rotation_paddingToDetectPlayArea );
        posZ_clamped = Mathf.Clamp( cameraExplorer.transform.position.z, playArea_side_stageFront_transform.position.z + rotation_paddingToDetectPlayArea, playArea_side_stageBack_transform.position.z - rotation_paddingToDetectPlayArea );
        cameraExplorer.transform.position = new Vector3( posX_clamped, posY_clamped, posZ_clamped );
    }

    void Input_ArrowKeysHeadRotation()
    {
        if( Input.GetKey( KeyCode.UpArrow ) )
        {
            cameraExplorer.transform.eulerAngles = new Vector3( cameraExplorer_rotation.x - headLooksAround_stepsize, cameraExplorer_rotation.y, cameraExplorer_rotation.z );
        }
        else if( Input.GetKey( KeyCode.DownArrow ) )
        {
            cameraExplorer.transform.eulerAngles = new Vector3( cameraExplorer_rotation.x + headLooksAround_stepsize, cameraExplorer_rotation.y, cameraExplorer_rotation.z );
        }
        else if( Input.GetKey( KeyCode.LeftArrow ) )
        {
            cameraExplorer.transform.eulerAngles = new Vector3( cameraExplorer_rotation.x, cameraExplorer_rotation.y - headLooksAround_stepsize, cameraExplorer_rotation.z );
        }
        else if( Input.GetKey( KeyCode.RightArrow ) )
        {
            cameraExplorer.transform.eulerAngles = new Vector3( cameraExplorer_rotation.x, cameraExplorer_rotation.y + headLooksAround_stepsize, cameraExplorer_rotation.z );
        }
    }

    // *********
    // out of bounds checks
    void Check_MoveToPt_OutOfBounds()
    {
        beyondPlayArea_bottom_thresh          = moveToPoint_zoom.y < playArea_bottom_transform.position.y;
        beyondPlayArea_top_thresh             = moveToPoint_zoom.y > playArea_top_transform.position.y;
        beyondPlayArea_side_stageFront_thresh = moveToPoint_zoom.z < playArea_side_stageFront_transform.position.z;
        beyondPlayArea_side_stageBack_thresh  = moveToPoint_zoom.z > playArea_side_stageBack_transform.position.z;
        beyondPlayArea_side_stageLeft_thresh  = moveToPoint_zoom.x < playArea_side_stageLeft_transform.position.x;
        beyondPlayArea_side_stageRight_thresh = moveToPoint_zoom.x > playArea_side_stageRight_transform.position.x;
    }

    void Check_CurrPos_AtBoundaryOfPlayArea()
    {
        beyondPlayArea_bottom_thresh          = cameraExplorer_position.y < playArea_bottom_transform.position.y + rotation_paddingToDetectPlayArea;
        beyondPlayArea_top_thresh             = cameraExplorer_position.y > playArea_top_transform.position.y + rotation_paddingToDetectPlayArea;
        beyondPlayArea_side_stageFront_thresh = cameraExplorer_position.z < playArea_side_stageFront_transform.position.z + rotation_paddingToDetectPlayArea;
        beyondPlayArea_side_stageBack_thresh  = cameraExplorer_position.z > playArea_side_stageBack_transform.position.z + rotation_paddingToDetectPlayArea;
        beyondPlayArea_side_stageLeft_thresh  = cameraExplorer_position.x < playArea_side_stageLeft_transform.position.x + rotation_paddingToDetectPlayArea;
        beyondPlayArea_side_stageRight_thresh = cameraExplorer_position.x > playArea_side_stageRight_transform.position.x + rotation_paddingToDetectPlayArea;
    }

    void Identify_ForwardLookingPlayAreaBoundary()
    {
        // we need to know what playAreaBoundary is in front of the camera's nose to calibrate what to do with out of bounds
        foreach( RaycastHit hit in ray_toMouse_hits )
        {
            if( hit.transform.tag == "playAreaBoundary" )
            {
                forwardLooking_playAreaBoundary = hit.transform.name;
                break;
            }
        }
    }

    void MouseScrollCalibratedTo_CameraOrientationAndOutOfBounds()
    {
        // out of bounds handling:
        // use the scrollwheel value as the MoveTowards stepsize unless at a playArea boundary - then stepSize = 0
        // if out of bounds, calibrate what to do with positive or negative mousescrollwheel based on which playAreaBoundary is in front of the camera's nose
        if( beyondPlayArea_bottom_thresh == true )
        {
            camera_playAreaboundaryGO_forwardDir = "playArea_bottom";
            outOfbounds = true;
        }
        else if( beyondPlayArea_top_thresh == true )
        {
            camera_playAreaboundaryGO_forwardDir = "playArea_top";
            outOfbounds = true;
        }
        else if( beyondPlayArea_side_stageFront_thresh == true )
        {
            camera_playAreaboundaryGO_forwardDir = "playArea_side_stageFront";
            outOfbounds = true;
        }
        else if( beyondPlayArea_side_stageBack_thresh == true )
        {
            camera_playAreaboundaryGO_forwardDir = "playArea_side_stageBack";
            outOfbounds = true;
        }
        else if( beyondPlayArea_side_stageLeft_thresh == true )
        {
            camera_playAreaboundaryGO_forwardDir = "playArea_side_stageLeft";
            outOfbounds = true;
        }
        else if( beyondPlayArea_side_stageRight_thresh == true )
        {
            camera_playAreaboundaryGO_forwardDir = "playArea_side_stageRight";
            outOfbounds = true;
        }
        else
        {
            // else not out of bounds...
            outOfbounds = false;
        }

        if( outOfbounds == true )
        {
            // calibrate whether pos or neg mouseScroll values are a problem based on what's in front of camera's nose 
            // ( zooming in forward = positive mouse scroll wheel values; zooming out = negative )
            if( forwardLooking_playAreaBoundary == camera_playAreaboundaryGO_forwardDir )
            {
                if( mouseScrollWheel > 0 ) { zoom_stepSize = 0; }
                else                       { zoom_stepSize = mouseScrollWheel; }
            }
            else
            {
                if( mouseScrollWheel < 0 ) { zoom_stepSize = 0; }
                else                       { zoom_stepSize = mouseScrollWheel; }
            }
        }
        else
        {
            zoom_stepSize = mouseScrollWheel;
        }
    }

    //***********
    // upsideDown
    void CheckIfCameraInsideAnyVoid()
    {
        iAmInsideThisVoid_test = false;
        foreach( Void_Cn voidEntry in voidsAllInfo )
        {
            if( voidEntry.boundingQbits_convexHullOrder_allInfo != null )
            {
                if( voidEntry.isOpen == true )
                {
                    iAmInsideThisVoid_test = CheckIfIAmInsideThisVoid( voidEntry.boundingQbits_convexHullOrder_positions, new Vector2( cameraExplorer_position.x, cameraExplorer_position.z ) );
                    if( iAmInsideThisVoid_test == true )
                    {
                        aud_upsideDown = !aud_upsideDown;
                        upsideDown_change = true;
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

    void UpsideDown_Change()
    {
        // *** always rotate 90 deg from where you were, go up to the playArea_top and to its edge, and face the floor center

        // if didn't rotate parent first, eulerAngle values wouldn't stay consistent for what they meant for the camera's orientation 
        this.transform.eulerAngles = new Vector3( this.transform.eulerAngles.x, this.transform.eulerAngles.y + 180, this.transform.eulerAngles.z );

        // before upsideDown: was facing back; then, above: rotated to face front
        if( this.transform.eulerAngles.y >= 135 && this.transform.eulerAngles.y <= 225 )
        {
            cameraExplorer.transform.position = new Vector3( 0f, playArea_top_transform.position.y - .4f, playArea_top_size / 2 );
        }
        // before upsideDown: was facing right; then, above: rotated to face left
        else if( this.transform.eulerAngles.y >= 225 && this.transform.eulerAngles.y <= 315 )
        {
            cameraExplorer.transform.position = new Vector3(playArea_top_size / 2, playArea_top_transform.position.y - .4f, 0f);
        }
        // before upsideDown: facing front; then, above: rotated to face back
        else if( ( this.transform.eulerAngles.y >= 0 && this.transform.eulerAngles.y <= 45 ) || ( this.transform.eulerAngles.y >= 315 && this.transform.eulerAngles.y <= 360 ) )
        {
            cameraExplorer.transform.position = new Vector3( 0f, playArea_top_transform.position.y - .4f, -playArea_top_size / 2 );
        }
        // before upsideDown: facing left; then, above: rotated to face back
        else if( this.transform.eulerAngles.y >= 45 && this.transform.eulerAngles.y <= 135 )
        {
            cameraExplorer.transform.position = new Vector3( -playArea_top_size / 2, playArea_top_transform.position.y - .4f, 0f );
        }

        // and then rotate the camera itself to face floor's center:
        cameraExplorer.transform.LookAt( floorGO.transform.position );

        // effects:
        grayscale.enabled = aud_upsideDown;
        twirl.enabled = aud_upsideDown;
        vortex.enabled = aud_upsideDown;
        blur.enabled = aud_upsideDown;

        if( aud_upsideDown == true )
        {
            twirl.center = Vector2.one * .5f;
            twirl.radius = Vector2.one * .4f;
            twirl.angle = 50f;
            vortex.center = Vector2.one * .5f;
            vortex.radius = Vector2.one * .4f;
            vortex.angle = 50f;
            aud_upsideDown_transp = 0f;
        }

        upsideDown_delay_feedbackLimited = false; //<-- statefulness of c# needs this reset here or suckedIntoVoid thinks we're still rebounding from prev visits to the upsideDown

        // if got sucked in, looks better if begin by still moving a little - we'll give it a fake mousescrollwheel and ray above
        if( rightsideUp_suckingExplorerIntoVoid == true )
        {
            upsideDown_beginFromSuckedIn = true;
            upsideDown_beginFromSuckedIn_startTime = Time.time;
        }

    }

    void RightsideUp_ParamsByZooming()
    {
        if( explorer_distToClosestVoidCentroid <= mx_rightsideUp_distToVoidCentroid_distort )
        {
            rightsideUp_approachingVoid = true;
            if( rightsideUp_approachingVoidPrev == false )
            {
                blur.enabled = true;
                twirl.enabled = true;
                vortex.enabled = true;
                aud_rightsideUp_approachingVoid_onClick = true;
            }
            // screenToViewPort gives you the normalizes pos of the mouse in the game window with ( 0, 0 ) in the lower left corner and ( -1, -1 ) in upper right
            // this is also the range and behavior of the center values of these visual effects
            twirl.center  = cameraExplorer.WorldToViewportPoint( explorer_closestVoidCentroid );
            twirl.radius  = Vector2.one * Scale( explorer_distToClosestVoidCentroid, mx_rightsideUp_distToVoidCentroid_distort, 0f, .06f, .4f );
            twirl.angle   = Scale( explorer_distToClosestVoidCentroid, mx_rightsideUp_distToVoidCentroid_distort, 0f, 75f, 100f );
            vortex.center = cameraExplorer.WorldToViewportPoint( explorer_closestVoidCentroid );
            vortex.radius = Vector2.one * Scale( explorer_distToClosestVoidCentroid, mx_rightsideUp_distToVoidCentroid_distort, 0f, .06f, .4f );
            vortex.angle  = Scale( explorer_distToClosestVoidCentroid, mx_rightsideUp_distToVoidCentroid_distort, 0f, 75f, 600f );

            aud_rightsideUp_approachingVoid_stutter_feedback          = Scale( explorer_distToClosestVoidCentroid, mx_rightsideUp_distToVoidCentroid_distort, 0f, mx_rightsideUp_approachingVoid_stutter_feedbackRange[0], mx_rightsideUp_approachingVoid_stutter_feedbackRange[1] );
            rightsideUp_approachingVoid_stutter_shiftamt_devFrom1 = Scale( explorer_distToClosestVoidCentroid, mx_rightsideUp_distToVoidCentroid_distort, 0f, 0f, mx_rightsideUp_approachingVoid_stutter_shiftamt_maxDeviateFrom1 );
            aud_rightsideUp_approachingVoid_stutter_shiftamt          = 1f + rightsideUp_approachingVoid_stutter_shiftamt_devFrom1;
            aud_rightsideUp_approachingVoid_reverb_roomsize   = Scale( explorer_distToClosestVoidCentroid, mx_rightsideUp_distToVoidCentroid_distort, 0f, mx_rightsideUp_approachingVoid_reverb_roomsizeRange[0], mx_rightsideUp_approachingVoid_reverb_roomsizeRange[1] );
            aud_rightsideUp_approachingVoid_reverb_decay      = Scale( explorer_distToClosestVoidCentroid, mx_rightsideUp_distToVoidCentroid_distort, 0f, mx_rightsideUp_approachingVoid_reverb_decayRange[0], mx_rightsideUp_approachingVoid_reverb_decayRange[1] );
            aud_rightsideUp_approachingVoid_reverb_damping    = Scale( explorer_distToClosestVoidCentroid, mx_rightsideUp_distToVoidCentroid_distort, 0f, mx_rightsideUp_approachingVoid_reverb_dampingRange[0], mx_rightsideUp_approachingVoid_reverb_dampingRange[1] );
            aud_rightsideUp_approachingVoid_reverb_diffusion  = Scale( explorer_distToClosestVoidCentroid, mx_rightsideUp_distToVoidCentroid_distort, 0f, mx_rightsideUp_approachingVoid_reverb_diffusionRange[0], mx_rightsideUp_approachingVoid_reverb_diffusionRange[1] );
        }
        else
        {
            rightsideUp_approachingVoid = false;
            if( rightsideUp_approachingVoidPrev == true )
            {
                blur.enabled = false;
                twirl.enabled = false;
                vortex.enabled = false;
                aud_rightsideUp_approachingVoid_offClick = true;
            }
        }
    }

    void UpsideDown_ParamsByZooming()
    {
        // ** CLOSE vs DISTANT audio
        cameraDistToFloorCenter = Vector3.Distance( cameraExplorer_position, floorGO.transform.position );
        if( upsideDown_change == true )
        {
            cameraDistToFloorCenterPrev = cameraDistToFloorCenter;
        }

        // params that interpolate from distant to close ( to floor center )
        float cameraDistToFloorCenter_clampedForScaling = Mathf.Clamp( cameraDistToFloorCenter, 0f, 14f );
        aud_upsideDown_stutter_feedback    = Scale( cameraDistToFloorCenter_clampedForScaling, 14f, 0f, mx_upsideDown_distant_stutter_feedback, mx_upsideDown_close_stutter_feedback );
        aud_upsideDown_filter_freq         = Scale( cameraDistToFloorCenter_clampedForScaling, 14f, 0f, mx_upsideDown_distant_filter_freq, mx_upsideDown_close_filter_freq );

            // ** distant:
        if( upsideDown_distant == true )
        {

        }
            // ** close:
        else
        {
            aud_upsideDown_stutter_dropoutprob = Mathf.Clamp( Scale( cameraDistToFloorCenter, mx_upsideDown_distantOrClose_distFromCenter, 0f, mx_upsideDown_distant_stutter_dropoutprob, mx_upsideDown_close_stutter_dropoutprob ), 0f, mx_upsideDown_distantOrClose_distFromCenter);
        }

            // distant or close thresh crossing 
        if( ( cameraDistToFloorCenter < mx_upsideDown_distantOrClose_distFromCenter && cameraDistToFloorCenterPrev >= mx_upsideDown_distantOrClose_distFromCenter) || ( cameraDistToFloorCenter > mx_upsideDown_distantOrClose_distFromCenter && cameraDistToFloorCenterPrev <= mx_upsideDown_distantOrClose_distFromCenter) )
        {
            upsideDown_distant = !upsideDown_distant;
            upsideDown_distantOrClose_change = true;
        }

        // DELAY WITH FEEDBACK...
        // as get close to the floor, distance to closest centroid is the value that plays the feedback multiplier in the delay with feedback
        if( rotating == true || mouseScrollWheel != 0 )
        {
            GetClosestVoidInfo();

            // ...ON/OFF
            if( explorer_distToClosestVoidCentroid <= mx_upsideDown_distToVoidCentroid_delayOn )
            {
                upsideDown_delayOn = true;
                if( upsideDown_delayOnPrev == false )
                {
                    aud_upsideDown_delayOn_click = true;
                    float randVal = Random.Range( 0f, 1f );
                    if( randVal <= mx_upsideDown_delay_feedbackMult_probRangeLow )
                    {
                        upsideDown_delay_feedbackMultMax = Random.Range( mx_upsideDown_delay_feedbackMultMaxRangeLow[0], mx_upsideDown_delay_feedbackMultMaxRangeLow[1] );
                    }
                    else
                    {
                        upsideDown_delay_feedbackMultMax = Random.Range( mx_upsideDown_delay_feedbackMultMaxRangeHigh[0], mx_upsideDown_delay_feedbackMultMaxRangeHigh[1] );
                    }
                }
            }
            else
            {
                upsideDown_delayOn = false;
                if( upsideDown_delayOnPrev == true )
                {
                    aud_upsideDown_delayOff_click = true;
                }
            }

            //...FEEDBACKMULT and WHETHER TO REPORT
            if( explorer_distToClosestVoidCentroid <= mx_upsideDown_distToVoidCentroid_delayOn && upsideDown_change == false )
            {
                // only report feedbackMult if user is actively moving toward the void ( > 0 ), eventually bursting through to the rightSideUp,
                // otherwise, let max mechanism take over which limits the feedbackMult for us 
                // in Max see: "theUpsideDown" > "approaching void centroid - delay with feedback" > "limit feedback / amp explosion" for mechanism
                // >~ and edge~ packed to odot report to this script oscIn
                // this script then does the following to handle the situation
                if( mouseScrollWheel > 0f )
                {
                    upsideDown_reportFeedbackMult = true;
                    // above, get a new feedbackMultMax value every time the delayOnClick happens
                    upsideDown_delay_feedbackMult = Scale( explorer_distToClosestVoidCentroid, mx_upsideDown_distToVoidCentroid_delayOn, 0f, mx_upsideDown_delay_feedbackMultMin, upsideDown_delay_feedbackMultMax );
                }
                else
                {
                    upsideDown_reportFeedbackMult = false;
                }
            }
        }
        else
        {
            upsideDown_reportFeedbackMult = false; // <-- need this to make sure report isn't stuck as true after get pushed away from void and not mouseScrollWheel == 0
        }

        // ...LIMIT FEEDBACKMULT ( oscIn from Max )
            // lerping away from void or not
        if( upsideDown_delay_feedbackLimited == true && mouseScrollWheel <= 0f ) // <-- oscIn from max && not moving toward void
        {
            upsideDown_pushExplorerAwayFromVoid_lerping = true;
            upsideDown_delay_feedbackLimited = false;
        }
        else if( mouseScrollWheel > 0f ) // <-- moving toward void
        {
            upsideDown_pushExplorerAwayFromVoid_lerping = false;
        }

        // lerping away from void
        if( upsideDown_pushExplorerAwayFromVoid_lerping == true && mouseScrollWheel <= 0f )
        {
            // Debug.Log("push");
            PushExplorerAwayFromVoid();
        }

        cameraDistToFloorCenterPrev = cameraDistToFloorCenter;
        upsideDown_delayOnPrev = upsideDown_delayOn;
    }

    void GetClosestVoidInfo()
    {
        explorer_distToClosestVoidCentroid = Mathf.Infinity;
        foreach( Void_Cn voidEntry in voidsAllInfo )
        {
            float thisDist = Vector3.Distance( cameraExplorer.transform.position, voidEntry.centroid );
            if( thisDist < explorer_distToClosestVoidCentroid )
            {
                explorer_distToClosestVoidCentroid = thisDist;
                explorer_closestVoidCentroid = voidEntry.centroid;
            }
        }
    }

    void PushExplorerAwayFromVoid()
    {
        // get the destination pt behind the camera if max sends the too-loud flag
        if( upsideDown_pushExplorerAwayFromVoid_lerpingPrev == false )
        {
            // goal: we want to push the camera back to the limit of the delayOn (distToVoidCentroid_delayOn), and along the ray trajectory of cameraToMouse, 
            // which requires reversing the cameraToMouse ray in the other direction back out away from the void.
            // can't flip the origin and direction because direction Vectors are normalized in Unity, even if you provide world coords
            // https://docs.unity3d.com/ScriptReference/Ray-direction.html
            // an easy trick is, let the origin now be a point on the initial ray, and the dir is simply -ray_initial.direction
            ray_toMouseReverse = new Ray( ray_toMouse.GetPoint( .5f ), -ray_toMouse.direction );
            // Debug.DrawRay( ray_toMouse.origin, ray_toMouse.direction, Color.red ); //<-- the original camera to mousePt ray

            /*
            if( Input.GetKey( KeyCode.T ) )
            {
                Debug.DrawRay( ray_toMouse.GetPoint( .5f ), -ray_toMouse.direction, Color.cyan ); //<-- the reverse of the camera to mousePt - mousePt to camera ray
            }*/

            ray_toMouseReverseHits = Physics.RaycastAll( ray_toMouseReverse, 25f );
            foreach( RaycastHit hit in ray_toMouseReverseHits )
            {
                if( hit.transform.name == "camera_explorer" )
                {
                    // a not so accurate but good enough measure of how far to push the camera back to arrive near the threshold of the on/off for the dek
                    upsideDown_pushExplorerAwayFromVoid_destinationPt = ray_toMouseReverse.GetPoint( ( mx_upsideDown_distToVoidCentroid_delayOn - explorer_distToClosestVoidCentroid ) + 12 );
                    upsideDown_pushExplorerAwayFromVoid_totalDistToDelayOnThresh = Vector3.Distance( cameraExplorer_position, upsideDown_pushExplorerAwayFromVoid_destinationPt );
                    break;
                }
            }

            upsideDown_pushExplorerAwayFromVoid_lerping = true;
        }

        upsideDown_pushExplorerAwayFromVoid_deltaDistToDelayOnThresh = upsideDown_pushExplorerAwayFromVoid_totalDistToDelayOnThresh - Vector3.Distance( cameraExplorer_position, upsideDown_pushExplorerAwayFromVoid_destinationPt );
        upsideDown_pushExplorerAwayFromVoid_phase = upsideDown_pushExplorerAwayFromVoid_deltaDistToDelayOnThresh / upsideDown_pushExplorerAwayFromVoid_totalDistToDelayOnThresh;
        cameraExplorer.transform.position = Vector3.MoveTowards( cameraExplorer_position, upsideDown_pushExplorerAwayFromVoid_destinationPt, .5f );
        if( upsideDown_pushExplorerAwayFromVoid_phase >= .99f )
        {
            upsideDown_pushExplorerAwayFromVoid_lerping = false;
        }

        /*
        if( Input.GetKey( KeyCode.T ) )
        {
            Debug.Log( "lerping " + upsideDown_pushExplorerAwayFromVoid_lerping + " phase " + upsideDown_pushExplorerAwayFromVoid_phase + " report " + upsideDown_reportFeedbackMult );
        }*/
    }

    // ************
    // osc out
    void ReportOscStart()
    {
        oscOut_script.Send( "/theUpsideDown/realtimeGran/1/amp/global",                 aud_upsideDown_ampGlobal );
        oscOut_script.Send( "/theUpsideDown/delay/amp/global",                          mx_upsideDown_delay_ampGlobal );
        oscOut_script.Send( "/theUpsideDown/delay/ampLimiterThresh",                    mx_upsideDown_delay_feedbackMult_ampLimiterThresh );
        oscOut_script.Send( "/theUpsideDown/bg/amp/global",                             mx_upsideDown_bg_ampGlobal );
        oscOut_script.Send( "/theUpsideDown/world/1/flange/rate",                       mx_upsideDown_flange_rate );
        oscOut_script.Send( "/theUpsideDown/world/1/flange/depth",                      mx_upsideDown_flange_depth );
        oscOut_script.Send( "/theRightsideUp/approachingVoid/stutter/repeatprobRange",  mx_rightsideUp_approachingVoid_stutter_repeatprobRange[0], mx_rightsideUp_approachingVoid_stutter_repeatprobRange[1] );
    }

    void ReportOscUpdate()
    {
        if( upsideDown_change == true )
        {
            oscOut_script.Send( "/theUpsideDown/world/globalBool",                      aud_upsideDown );

            if( aud_upsideDown == false )
            {
                oscOut_script.Send( "/theUpsideDown/world/1/off",                       1 );
            }
            else
            {
                oscOut_script.Send( "/theRightsideUp/approachingVoid/on",               0 );
                oscOut_script.Send( "/theUpsideDown/world/1/on",                        1 );
                oscOut_script.Send( "/theUpsideDown/world/1/amp/global",                aud_upsideDown_ampGlobal );
                Osc_ConstantValues_Distant();
            }
        }

        if( aud_upsideDown == false )
        {
            if( rightsideUp_approachingVoid == true )
            {
                if( aud_rightsideUp_approachingVoid_onClick == true )
                {
                    oscOut_script.Send( "/theRightsideUp/approachingVoid/on",           1 );
                    aud_rightsideUp_approachingVoid_onClick = false;
                }

                oscOut_script.Send( "/theRightsideUp/approachingVoid/stutter/feedback", aud_rightsideUp_approachingVoid_stutter_feedback );
                oscOut_script.Send( "/theRightsideUp/approachingVoid/stutter/shiftamt", aud_rightsideUp_approachingVoid_stutter_shiftamt );
                oscOut_script.Send( "/theRightsideUp/approachingVoid/reverb/size",      aud_rightsideUp_approachingVoid_reverb_roomsize );
                oscOut_script.Send( "/theRightsideUp/approachingVoid/reverb/decay",     aud_rightsideUp_approachingVoid_reverb_decay );
                oscOut_script.Send( "/theRightsideUp/approachingVoid/reverb/damping",   aud_rightsideUp_approachingVoid_reverb_damping );
                oscOut_script.Send( "/theRightsideUp/approachingVoid/reverb/diffusion", aud_rightsideUp_approachingVoid_reverb_diffusion );
            }
            else
            {
                if( aud_rightsideUp_approachingVoid_offClick == true )
                {
                    oscOut_script.Send( "/theRightsideUp/approachingVoid/on",           0 );
                    aud_rightsideUp_approachingVoid_offClick = false;
                }
            }
        }
        else if( aud_upsideDown == true )
        {
            // ** constantly interpolating params - from distant to close
            oscOut_script.Send( "/theUpsideDown/world/1/transp",                        aud_upsideDown_transp );
            oscOut_script.Send( "/theUpsideDown/world/1/stutter/feedback",              aud_upsideDown_stutter_feedback );
            oscOut_script.Send( "/theUpsideDown/world/1/filter/freq",                   aud_upsideDown_filter_freq );

            // ** when crossing distant/close thresh
            if( upsideDown_distantOrClose_change == true )
            {
                if( upsideDown_distant == true )
                {
                    Osc_ConstantValues_Distant();
                }
                else
                {
                    Osc_ConstantValues_Close();
                    // reverb will switch between two states
                }
                upsideDown_distantOrClose_change = false;
            }

            // ** depending on distant or close 
            if( upsideDown_distant == false )
            {
                // dropoutprob is a constant value while distant, and scaled when close
                oscOut_script.Send( "/theUpsideDown/world/1/stutter/dropoutprob",       aud_upsideDown_stutter_dropoutprob );
            }

            // ** downsamp
            if( aud_upsideDown_downsamp_begin == true )
            {
                oscOut_script.Send( "/theUpsideDown/world/1/downsamp/begin",            1 );
                oscOut_script.Send( "/theUpsideDown/world/1/downsamp/interval",         aud_upsideDown_downsamp_interval );
                aud_upsideDown_downsamp_begin = false;
            }
            else if( aud_upsideDown_downsamp_end == true )
            {
                oscOut_script.Send( "/theUpsideDown/world/1/downsamp/end",              1 );
                aud_upsideDown_downsamp_end = false;
            }

            // ** delay
            if( upsideDown_delayOn == true )
            {
                if( aud_upsideDown_delayOn_click == true )
                {
                    oscOut_script.Send( "/theUpsideDown/delay/on",                      1 );
                    aud_upsideDown_delayOn_click = false;
                }
                if( upsideDown_reportFeedbackMult == true )
                {
                    oscOut_script.Send( "/theUpsideDown/delay/feedbackMult",            upsideDown_delay_feedbackMult );
                }
            }
            else
            {
                if( aud_upsideDown_delayOff_click == true )
                {
                    oscOut_script.Send( "/theUpsideDown/delay/off",                     1 );
                    aud_upsideDown_delayOff_click = false;
                }
            }
        }
    }

    void Osc_ConstantValues_Distant()
    {
        oscOut_script.Send( "/theUpsideDown/world/1/stutter/repeatprobRange",           mx_upsideDown_distant_stutter_repeatprobRange[0], mx_upsideDown_distant_stutter_repeatprobRange[1] );
        oscOut_script.Send( "/theUpsideDown/world/1/stutter/shiftamt",                  mx_upsideDown_distant_stutter_shiftamt );
        oscOut_script.Send( "/theUpsideDown/world/1/stutter/dropoutprob",               mx_upsideDown_distant_stutter_dropoutprob );
        oscOut_script.Send( "/theUpsideDown/world/1/reverb/size",                       mx_upsideDown_distant_reverb_roomsize );
        oscOut_script.Send( "/theUpsideDown/world/1/reverb/decay",                      mx_upsideDown_distant_reverb_decay );
        oscOut_script.Send( "/theUpsideDown/world/1/reverb/damping",                    mx_upsideDown_distant_reverb_damping );
        oscOut_script.Send( "/theUpsideDown/world/1/reverb/diffusion",                  mx_upsideDown_distant_reverb_diffusion );
    }

    void Osc_ConstantValues_Close()
    {
        oscOut_script.Send( "/theUpsideDown/world/1/stutter/repeatprobRange",           mx_upsideDown_close_stutter_repeatprobRange[0], mx_upsideDown_close_stutter_repeatprobRange[1] );
        oscOut_script.Send( "/theUpsideDown/world/1/stutter/shiftamt",                  mx_upsideDown_close_stutter_shiftamt );
        oscOut_script.Send( "/theUpsideDown/world/1/stutter/dropoutprob",               mx_upsideDown_close_stutter_dropoutprob );
        // reverb will switch between two states?  right now doesn't
        oscOut_script.Send( "/theUpsideDown/world/1/reverb/size",                       mx_upsideDown_close_2reverbs_roomsize[0] );
        oscOut_script.Send( "/theUpsideDown/world/1/reverb/decay",                      mx_upsideDown_close_2reverbs_decay[0] );
        oscOut_script.Send( "/theUpsideDown/world/1/reverb/damping",                    mx_upsideDown_close_2reverbs_damping[0] );
        oscOut_script.Send( "/theUpsideDown/world/1/reverb/diffusion",                  mx_upsideDown_close_2reverbs_diffusion[0] );
    }

    // *********
    // osc in
    void OscIn_FeedbackLimited( int bang )
    {
        upsideDown_delay_feedbackLimited = true;
        // Debug.Log(upsideDown_delay_feedbackLimited);
    }

    // *********
    // asset package multitouch detection
    // 
    void OnDragging( DragInfo dragInfo )
    {
        // two-finger drag
        if( dragInfo.isMouse && dragInfo.index == 1 )
        {
            OnMFDragging( dragInfo );
            rotating = true;
        }
        // else single finger
        else
        {
            rotating = false;
        }
    }

    void OnMFDragging( DragInfo dragInfo )
    {
        rotation_velocityVector = new Vector3( -dragInfo.delta.y * rotation_Xscaler * Time.deltaTime, dragInfo.delta.x * rotation_Yscaler * Time.deltaTime, 0 );
        // Debug.Log(rotation_velocityVector);
    }

    void OnDraggingEnd( DragInfo dragInfo )
    {

    }

    //**********
    // mx values

    void MixerValues_Init()
    {
        mx_upsideDown_ampGlobal_begin                                   = mixer.upsideDown_ampGlobal_begin;
        mx_upsideDown_ampGlobal_lowerAndRougher                         = mixer.upsideDown_ampGlobal_lowerAndRougher;
        mx_upsideDown_flange_rate                                       = mixer.upsideDown_flange_rate;
        mx_upsideDown_flange_depth                                      = mixer.upsideDown_flange_depth;
        mx_upsideDown_distantOrClose_distFromCenter                     = mixer.upsideDown_distantOrClose_distFromCenter;
        mx_upsideDown_distant_stutter_repeatprobRange                   = mixer.upsideDown_distant_stutter_repeatprobRange;
        mx_upsideDown_distant_stutter_shiftamt                          = mixer.upsideDown_distant_stutter_shiftamt;
        mx_upsideDown_distant_stutter_feedback                          = mixer.upsideDown_distant_stutter_feedback;
        mx_upsideDown_distant_stutter_dropoutprob                       = mixer.upsideDown_distant_stutter_dropoutprob;
        mx_upsideDown_distant_reverb_roomsize                           = mixer.upsideDown_distant_reverb_roomsize;
        mx_upsideDown_distant_reverb_decay                              = mixer.upsideDown_distant_reverb_decay;
        mx_upsideDown_distant_reverb_damping                            = mixer.upsideDown_distant_reverb_damping;
        mx_upsideDown_distant_reverb_diffusion                          = mixer.upsideDown_distant_reverb_diffusion;
        mx_upsideDown_distant_filter_freq                               = mixer.upsideDown_distant_filter_freq;
        mx_upsideDown_close_stutter_repeatprobRange                     = mixer.upsideDown_close_stutter_repeatprobRange;
        mx_upsideDown_close_stutter_shiftamt                            = mixer.upsideDown_close_stutter_shiftamt;
        mx_upsideDown_close_stutter_feedback                            = mixer.upsideDown_close_stutter_feedback;
        mx_upsideDown_close_stutter_dropoutprob                         = mixer.upsideDown_close_stutter_dropoutprob;
        mx_upsideDown_close_2reverbs_roomsize                           = mixer.upsideDown_close_2reverbs_roomsize;
        mx_upsideDown_close_2reverbs_decay                              = mixer.upsideDown_close_2reverbs_decay;
        mx_upsideDown_close_2reverbs_damping                            = mixer.upsideDown_close_2reverbs_damping;
        mx_upsideDown_close_2reverbs_diffusion                          = mixer.upsideDown_close_2reverbs_diffusion;
        mx_upsideDown_close_filter_freq                                 = mixer.upsideDown_close_filter_freq;
        mx_upsideDown_transpRange                                       = mixer.upsideDown_transpRange;
        mx_upsideDown_rotation_velocityY_scaledToTranspDeltaRange       = mixer.upsideDown_rotation_velocityY_scaledToTranspDeltaRange;
        mx_upsideDown_downsamp_intervalRange                            = mixer.upsideDown_downsamp_intervalRange;
        mx_upsideDown_distToVoidCentroid_delayOn                        = mixer.upsideDown_distToVoidCentroid_delayOn;
        mx_upsideDown_delay_ampGlobal                                   = mixer.upsideDown_delay_ampGlobal;
        mx_upsideDown_delay_feedbackMultMin                             = mixer.upsideDown_delay_feedbackMultMin;
        mx_upsideDown_delay_feedbackMultMaxRangeLow                     = mixer.upsideDown_delay_feedbackMultMaxRangeLow;
        mx_upsideDown_delay_feedbackMultMaxRangeHigh                    = mixer.upsideDown_delay_feedbackMultMaxRangeHigh;
        mx_upsideDown_delay_feedbackMult_probRangeLow                   = mixer.upsideDown_delay_feedbackMult_probRangeLow;
        mx_upsideDown_delay_feedbackMult_ampLimiterThresh               = mixer.upsideDown_delay_feedbackMult_ampLimiterThresh;
        mx_upsideDown_bg_ampGlobal                                      = mixer.upsideDown_bg_ampGlobal;
        mx_rightsideUp_distToVoidCentroid_suck                          = mixer.rightsideUp_distToVoidCentroid_suck;
        mx_rightsideUp_distToVoidCentroid_distort                       = mixer.rightsideUp_distToVoidCentroid_distort;
        mx_rightsideUp_approachingVoid_stutter_shiftamt_maxDeviateFrom1 = mixer.rightsideUp_approachingVoid_stutter_shiftamt_maxDeviateFrom1;
        mx_rightsideUp_approachingVoid_stutter_feedbackRange            = mixer.rightsideUp_approachingVoid_stutter_feedbackRange;
        mx_rightsideUp_approachingVoid_stutter_repeatprobRange          = mixer.rightsideUp_approachingVoid_stutter_repeatprobRange;
        mx_rightsideUp_approachingVoid_reverb_roomsizeRange             = mixer.rightsideUp_approachingVoid_reverb_roomsizeRange;
        mx_rightsideUp_approachingVoid_reverb_decayRange                = mixer.rightsideUp_approachingVoid_reverb_decayRange;
        mx_rightsideUp_approachingVoid_reverb_dampingRange              = mixer.rightsideUp_approachingVoid_reverb_dampingRange;
        mx_rightsideUp_approachingVoid_reverb_diffusionRange            = mixer.rightsideUp_approachingVoid_reverb_diffusionRange;
    }

    //****************************************
    // ><>  ><>  ><>  ><>
    void EvolutionParams()
    {
        if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.lowerAndRougher )
        {
            aud_upsideDown_ampGlobal = mx_upsideDown_ampGlobal_lowerAndRougher;
        }
        else
        {
            aud_upsideDown_ampGlobal = mx_upsideDown_ampGlobal_begin;
        }
    }

    public float Scale(float oldValue, float oldMin, float oldMax, float newMin, float newMax)
    {

        float oldRange = oldMax - oldMin;
        float newRange = newMax - newMin;
        float newValue = (((oldValue - oldMin) * newRange) / oldRange) + newMin;

        return newValue;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if( aud_upsideDown == true )
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere( ray_toMouse.GetPoint( 4f ), gizscale1 ); //<-- origin of ray_behindCamera

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(upsideDown_pushExplorerAwayFromVoid_destinationPt, gizscale2);
            /*
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(ray_behindCamera.origin, gizscale3 );
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere( ray_behindCamera.direction, gizscale4 ); */

            /*
            Gizmos.color = Color.green;
            Gizmos.DrawSphere( ray_behindCamera_pointClosestToCamera, .1f );

            Gizmos.color = Color.white;
            Gizmos.DrawLine( ray_behindCamera.origin, ray_behindCamera.direction );
            Gizmos.DrawSphere( upsideDown_pushExplorerAwayFromVoid_destinationPt, .1f );*/

            //Gizmos.color = Color.cyan;
            //Gizmos.DrawRay(ray_toMouse.origin, ray_toMouse.direction);

            /*
            Gizmos.color = Color.red;
            Gizmos.DrawRay(ray_toMouse.direction, ray_toMouse.origin);*/
        }

        /*
        if(upsideDown == true)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(upsideDown_startPoint,.3f);
        }*/
    }
}
