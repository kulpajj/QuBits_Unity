using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SimpleJSON;

public class VoidIgniterMovement : MonoBehaviour
{
    // OSC
    OscOut oscOutScript;

    // global evolution
    GlobalEvolution globalEvolution_script;
    GlobalEvolution.GlobalEvolutionState globalEvolutionState;

    // JSON
    JSONNode sampleInfo_JSONParsed;

    // mixer
    Mixer mixer;

    // ****
    // mixer values
    float   mx_destroyWhileFalling_prob_begin;
    string  mx_falling_bufferName;
    Vector2 mx_falling_delay_timeRange;
    Vector2 mx_falling_delay_lineDurRange;
    Vector2 mx_falling_ampGlobalRange_begin;
    Vector2 mx_falling_ampGlobalRange_beginCeiling;
    Vector2 mx_falling_ampGlobalRange_beginSpin;
    string  mx_curving_casetopRolly_bufferName;
    string  mx_curving_figure8s_1_bufferName;
    string  mx_curving_figure8s_2_bufferName;
    // casetopTranspRange constantly changing - inside the poly and switches between high range and low range, repeat
    // the poly also determines for itself whether to open gate and play each figure8s curving file / but casetopRolly always plays ( igniterCurving.1 )
    Vector2 mx_curving_casetopRolly_delay_timeRange;
    Vector2 mx_curving_figure8s_1_delay_timeRange;
    Vector2 mx_curving_figure8s_2_delay_timeRange;
    Vector2 mx_curving_casetopRolly_delay_lineDurRange;
    Vector2 mx_curving_figure8s_1_delay_lineDurRange;
    Vector2 mx_curving_figure8s_2_delay_lineDurRange;
    Vector2 mx_curving_ampGlobalRange_begin;
    Vector2 mx_curving_ampGlobalRange_beginCeiling;
    Vector2 mx_curving_ampGlobalRange_beginSpin;

    // ****
    // additional values to audio/Max
    bool    aud_GO_born;
    bool    aud_GO_destroyed;
    bool    aud_falling_begin;
    bool    aud_falling_end;
    float   aud_falling_bufferStartDur;
    Vector2 aud_falling_transpRange;
    float   aud_falling_globalAmp;
    bool    aud_curving_begin;
    bool    aud_curving_end;
    float   aud_curving_casetopRolly_bufferStartDur;
    float   aud_curving_figure8s_1_bufferStartDur;
    float   aud_curving_figure8s_2_bufferStartDur;
    Vector2 aud_curving_figure8s_1_transpRange;
    Vector2 aud_curving_figure8s_2_transpRange;
    float   aud_curving_casetopRolly_globalAmp;
    float   aud_curving_figure8s_1_globalAmp;
    float   aud_curving_figure8s_2_globalAmp;

    // stupid: osc engine can't send Vector2's directly so have to use its own .Add() method to add to these addresses 
    // <-- actually can; use commas between values
    OscMessage osc_falling_transpRange;
    OscMessage osc_falling_delay_timeRange;
    OscMessage osc_falling_delay_lineDurRange;
    OscMessage osc_curving_casetopRolly_delay_timeRange;
    OscMessage osc_curving_figure8s_1_transpRange;
    OscMessage osc_curving_figure8s_2_transpRange;
    OscMessage osc_curving_figure8s_1_delay_timeRange;
    OscMessage osc_curving_figure8s_2_delay_timeRange;
    OscMessage osc_curving_casetopRolly_delay_lineDurRange;
    OscMessage osc_curving_figure8s_1_delay_lineDurRange;
    OscMessage osc_curving_figure8s_2_delay_lineDurRange;

    public int self_id;
    float self_height;
    Rigidbody self_rigidbody;
    Color self_colorSaturated;
    Color self_colorFadedCompletely;
    Color self_colorFadedFlicker;
    Material self_material;
    public Void_Cn.HitByVoidIgniterType self_voidIgniterType;
    float self_voidIgniterType_randomWeighted;
    float qbitIgniter_threshold;

    float qbitIgniterFlicker_duration = .5f;
    float qbitIgniterFlicker_startTime;
    float qbitIgniterFlicker_deltaTime;
    float qbitIgniterFlicker_phase;
    Color qbitIgniterFlicker_startColor;
    Color qbitIgniterFlicker_targetColor;

    bool    falling = true;
    Vector2 falling_ampGlobalRange;
    bool    curving;
    bool    curvingBegin = true;
    float   curvingBeginTime;
    Vector3 curvingBeginPosition;
    Vector2 curving_ampGlobalRange;

    bool    destroyWhileFalling;
    float   destroyWhileFalling_prob;
    float   destroyWhileFalling_random;
    Vector2 destroyWhileFalling_durTilFadeRange = new Vector2( 1.5f, 2.5f );
    float   destroyWhileFalling_durTilFade;
    float   destroyWhileFalling_fallingStartTime;
    float   destroyWhileFalling_fallingDeltaTime;
    float   destroyWhileFalling_fadingDuration = 1f;
    bool    reportDestroyToUpdate;
    float lifeOnFloorDuration;
    float birthTime;
    bool  fading;
    float fading_beginsAtPercentFloorDuration = .8f;
    float fading_duration;
    float fading_startTime;
    bool  fading_begin = true;
    float fading_deltaTime;
    float fading_phase;

    Vector3 parabola;
    bool newParabola;
    float parabolaDuration;
    float parabolaStartTime;
    Vector3 parabolaStartPt;
    Vector3 parabolaEndPt;
    float parabolaHeight;
    int parabolaRandomDirectionOfHeightInt;
    int parabolaRandomDirectionOfHeightInt_prev = -1;
    string parabolaRandomDirectionOfHeightString;
    float parabolaPhase;
    GameObject floor;
    float floorY;
    Vector3 floorMin;
    Vector3 floorMax;

    float fallingStepSize = .008f;
    float drunkStepSize   = .008f;

    // check if I am inside any void:
    VoidsAllInfo voidsAllInfo_script;
    List<Void_Cn> voidsAllInfo;
    int  iAmInsideThisVoid_id = -1;
    bool iAmInsideThisVoid_test;
    bool report_hit = true;

    void Start()
    {
        oscOutScript = GameObject.Find( "osc" ).GetComponent<OscOut>();
        sampleInfo_JSONParsed = GameObject.Find( "loadJSON_fromMax" ).GetComponent<LoadJSON_FromMax>().sampleInfo_JSONParsed;

        floor = GameObject.Find( "floor" );
        floorY = floor.transform.position.y;
        floorMin = floor.GetComponent<Renderer>().bounds.min;
        floorMax = floor.GetComponent<Renderer>().bounds.max;
        self_height = GetComponent<Renderer>().bounds.size.y;
        self_material = GetComponent<Renderer>().material;
        self_rigidbody = GetComponent<Rigidbody>();

        lifeOnFloorDuration = UnityEngine.Random.Range( 4.0f, 11.0f );
        fading_duration = ( 1 - fading_beginsAtPercentFloorDuration ) * lifeOnFloorDuration;
        voidsAllInfo_script = GameObject.Find( "voidsAllInfo" ).GetComponent<VoidsAllInfo>();

        globalEvolution_script = GameObject.Find( "globalEvolution" ).GetComponent<GlobalEvolution>();
        globalEvolutionState = GlobalEvolution.GlobalEvolutionState.begin;
        mixer = new Mixer();
        MixerValues_Init();
        EvolutionParams();

        self_voidIgniterType_randomWeighted = UnityEngine.Random.Range( 0f, 1f );
        self_colorSaturated = self_material.color;

        // the IgniterType is determined here...the VoidMesh writes my igniterType to VoidAllInfo if I am inside the VoidMesh
        if( self_voidIgniterType_randomWeighted < qbitIgniter_threshold )
        {
            self_voidIgniterType = Void_Cn.HitByVoidIgniterType.staticAndQbit;
            //self_colorSaturated = new Color( .051f, .511f, .953f, 1f );
            qbitIgniterFlicker_startTime = Time.time;
            qbitIgniterFlicker_startColor = self_colorSaturated;
            self_colorFadedFlicker = new Color( self_colorSaturated.r, self_colorSaturated.g, self_colorSaturated.b, 0.8f );
            qbitIgniterFlicker_targetColor = self_colorFadedFlicker;
            this.transform.localScale = new Vector3( .3f, this.transform.localScale.y, .3f );
        }
        else if( self_voidIgniterType_randomWeighted >= qbitIgniter_threshold )
        {
            self_voidIgniterType = Void_Cn.HitByVoidIgniterType.staticOnly;
        }

        self_colorFadedCompletely = new Color( self_colorSaturated.r, self_colorSaturated.g, self_colorSaturated.b, 0.0f );
        self_material.color = self_colorSaturated;

        aud_GO_born = true;
        aud_falling_begin = true;
        aud_falling_bufferStartDur              = UnityEngine.Random.Range( 0f, sampleInfo_JSONParsed[ "igniterFalling/1/duration" ] );
        aud_curving_casetopRolly_bufferStartDur = UnityEngine.Random.Range( 0f, sampleInfo_JSONParsed[ "igniterCurving/1/duration" ] );
        aud_curving_figure8s_1_bufferStartDur   = UnityEngine.Random.Range( 0f, sampleInfo_JSONParsed[ "igniterCurving/2/duration" ] );
        aud_curving_figure8s_2_bufferStartDur   = UnityEngine.Random.Range( 0f, sampleInfo_JSONParsed[ "igniterCurving/3/duration" ] );
        aud_falling_globalAmp = UnityEngine.Random.Range( falling_ampGlobalRange[0], falling_ampGlobalRange[1] );
        float curving_globalAmp = UnityEngine.Random.Range( curving_ampGlobalRange[0], curving_ampGlobalRange[1] );
        aud_curving_casetopRolly_globalAmp = curving_globalAmp;
        aud_curving_figure8s_1_globalAmp   = curving_globalAmp;
        aud_curving_figure8s_2_globalAmp   = curving_globalAmp;
        destroyWhileFalling_random = UnityEngine.Random.Range( 0f, 1f );
        // Debug.Log("prob " + destroyWhileFalling_prob + " value " + destroyWhileFalling_random);
        if( destroyWhileFalling_random <= destroyWhileFalling_prob )
        {
            destroyWhileFalling = true;
            destroyWhileFalling_fallingStartTime = Time.time;
            destroyWhileFalling_durTilFade = UnityEngine.Random.Range( destroyWhileFalling_durTilFadeRange[0], destroyWhileFalling_durTilFadeRange[1] );
            // Debug.Log(destroyWhileFalling_durTilFade);
        }

        New_FallingTranspRange();
        New_Curving_figure8s_1_TranspRange();
        New_Curving_figure8s_2_TranspRange();
        InstantiateOscLists();
        ReportOscStart();
    }

    void Update()
    {
        if( curving == true )
        {
            voidsAllInfo = voidsAllInfo_script.voidsAllInfo;
            CheckIfIAmInsideAnyVoid();
        }

        if( self_voidIgniterType == Void_Cn.HitByVoidIgniterType.staticAndQbit && fading == false )
        {
            if( qbitIgniterFlicker_deltaTime > qbitIgniterFlicker_duration )
            {
                qbitIgniterFlicker_startTime = Time.time;
                if( qbitIgniterFlicker_targetColor == self_colorSaturated )
                {
                    qbitIgniterFlicker_startColor = self_colorSaturated;
                    qbitIgniterFlicker_targetColor = self_colorFadedFlicker;
                }
                else if( qbitIgniterFlicker_targetColor == self_colorFadedFlicker )
                {
                    qbitIgniterFlicker_startColor = self_colorFadedFlicker;
                    qbitIgniterFlicker_targetColor = self_colorSaturated;
                }
            }

            qbitIgniterFlicker_deltaTime = Time.time - qbitIgniterFlicker_startTime;

            qbitIgniterFlicker_phase = qbitIgniterFlicker_deltaTime / qbitIgniterFlicker_duration;

            self_material.color = Color.Lerp( qbitIgniterFlicker_startColor, qbitIgniterFlicker_targetColor, qbitIgniterFlicker_phase );
        }

        ReportOscUpdate();

        if( reportDestroyToUpdate == true )
        {
            Destroy( gameObject );
        }
    }

    void FixedUpdate()
    {
        if( this.transform.position.y <= ( floorY + .5f * self_height ) )
        {
            falling = false;
            curving = true;
        }

        if( falling == true )
        {
            this.transform.position = new Vector3( this.transform.position.x, this.transform.position.y - fallingStepSize, this.transform.position.z );

            if( destroyWhileFalling == true )
            {
                destroyWhileFalling_fallingDeltaTime = Time.time - destroyWhileFalling_fallingStartTime;
                if( destroyWhileFalling_fallingDeltaTime >= destroyWhileFalling_durTilFade )
                {
                    if( fading_begin == true )
                    {
                        fading_startTime = Time.time;
                        aud_falling_end = true;
                        fading_begin = false;
                    }

                    fading_deltaTime = Time.time - fading_startTime;
                    fading_phase = fading_deltaTime / destroyWhileFalling_fadingDuration;
                    self_material.color = Color.Lerp( self_colorSaturated, self_colorFadedCompletely, fading_phase );

                    if( fading_phase >= 1f )
                    {
                        reportDestroyToUpdate = true;
                        aud_GO_destroyed = true;
                    }
                }
            }
        }

        if( curving == true )
        {
            if( curvingBegin == true )
            {
                curvingBeginTime = Time.time;
                curvingBeginPosition = this.transform.position;
                fading_startTime = curvingBeginTime + fading_beginsAtPercentFloorDuration * lifeOnFloorDuration;
                aud_curving_begin = true;
                aud_falling_end = true;
                curvingBegin = false;
            }

            if( Time.time >= fading_startTime )
            {
                if( fading_begin == true )
                {
                    aud_curving_end = true;
                    fading_begin = false;
                }
                fading = true;
                fading_deltaTime = Time.time - fading_startTime;
                fading_phase = fading_deltaTime / fading_duration;
                self_material.color = Color.Lerp( self_colorSaturated, self_colorFadedCompletely, fading_phase );
            }

            if( Time.time - curvingBeginTime > lifeOnFloorDuration )
            {
                reportDestroyToUpdate = true;
                aud_GO_destroyed = true;
            }

            if( Time.time - parabolaStartTime > parabolaDuration )
            {
                newParabola = true;
            }

            if( newParabola == true )
            {
                parabolaStartPt = this.transform.position;
                parabolaEndPt = UnityEngine.Random.insideUnitCircle * 1f;
                parabolaEndPt = new Vector3( parabolaEndPt.x + this.transform.position.x, this.transform.position.y, parabolaEndPt.y + this.transform.position.z );
                parabolaHeight = UnityEngine.Random.Range( .2f, .7f );
                if( parabolaRandomDirectionOfHeightInt_prev == -1 )
                {
                    parabolaRandomDirectionOfHeightInt = UnityEngine.Random.Range( 0, 4 );
                }
                else
                {
                    parabolaRandomDirectionOfHeightInt = UnityEngine.Random.Range( 0, 4 );
                    if( parabolaRandomDirectionOfHeightInt == parabolaRandomDirectionOfHeightInt_prev )
                    {
                        while( parabolaRandomDirectionOfHeightInt == parabolaRandomDirectionOfHeightInt_prev )
                        {
                            parabolaRandomDirectionOfHeightInt = UnityEngine.Random.Range( 0, 4 );
                        }
                    }
                }
                parabolaDuration = UnityEngine.Random.Range( .9f, 1.5f );
                parabolaStartTime = Time.time;
                newParabola = false;
            }

            parabolaPhase = ( Time.time - parabolaStartTime ) / parabolaDuration;

            if( parabolaRandomDirectionOfHeightInt == 0 ) { parabolaRandomDirectionOfHeightString = "posX"; }
            if( parabolaRandomDirectionOfHeightInt == 1 ) { parabolaRandomDirectionOfHeightString = "negX"; }
            if( parabolaRandomDirectionOfHeightInt == 2 ) { parabolaRandomDirectionOfHeightString = "posZ"; }
            if( parabolaRandomDirectionOfHeightInt == 3 ) { parabolaRandomDirectionOfHeightString = "negZ"; }

            parabola = Parabola( parabolaStartPt, parabolaEndPt, parabolaHeight, parabolaRandomDirectionOfHeightString, parabolaPhase );

            this.transform.position = parabola;

            parabolaRandomDirectionOfHeightInt_prev = parabolaRandomDirectionOfHeightInt;
        }
    }

    // from DitzelGames    
    // https://www.youtube.com/watch?v=ddakS7BgHRI
    // https://gist.github.com/ditzel/68be36987d8e7c83d48f497294c66e08
    Vector3 Parabola(Vector3 start, Vector3 end, float height, string direction, float t)
    {
        Func<float, float> f = x => -4 * height * x * x + 4 * height * x;

        var mid = Vector3.Lerp(start, end, t);
        Vector3 position = new Vector3();

        if (direction == "posX") { position = new Vector3(f(t) + Mathf.Lerp(start.x, end.x, t), mid.y, mid.z); }
        else if (direction == "negX") { position = new Vector3(-f(t) + Mathf.Lerp(start.x, end.x, t), mid.y, mid.z); }
        else if (direction == "posZ") { position = new Vector3(mid.x, mid.y, f(t) + Mathf.Lerp(start.z, end.z, t)); }
        else if (direction == "negZ") { position = new Vector3(mid.x, mid.y, -f(t) + Mathf.Lerp(start.z, end.z, t)); }

        return position;
    }

    void CheckIfIAmInsideAnyVoid()
    {
        iAmInsideThisVoid_test = false;
        foreach( Void_Cn voidEntry in voidsAllInfo )
        {
            if( voidEntry.boundingQbits_convexHullOrder_allInfo != null )
            {
                if( voidEntry.isOpen == true )
                {
                    iAmInsideThisVoid_test = CheckIfIAmInsideThisVoid( voidEntry.boundingQbits_convexHullOrder_positions, new Vector2( this.transform.position.x, this.transform.position.z ) );
                    if( iAmInsideThisVoid_test == true && iAmInsideThisVoid_id == -1 )
                    {
                        iAmInsideThisVoid_id = voidEntry.id;
                        // report hit ONCE until exit void and re-enter
                        if( report_hit == true )
                        {
                            VoidMesh voidMeshScript = GameObject.Find( "void_" + iAmInsideThisVoid_id ).GetComponent<VoidMesh>();
                            voidMeshScript.aud_static_state = "igniter event";
                            voidMeshScript.aud_hitby_igniter = true;
                            voidMeshScript.hitby_voidIgniterType = self_voidIgniterType;
                            report_hit = false;
                        }
                        break;
                    }
                }
            }
        }

        if( iAmInsideThisVoid_test == false )
        {
            iAmInsideThisVoid_id = -1;
            report_hit = true;
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

    // ><>  ><>  ><>  ><>
    void EvolutionParams()
    {
        if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.begin )
        {
            // this makes it so we will never produce a qbit igniter
            qbitIgniter_threshold = -1;
            destroyWhileFalling_prob = mx_destroyWhileFalling_prob_begin;
            falling_ampGlobalRange = mx_falling_ampGlobalRange_begin;
            curving_ampGlobalRange = mx_curving_ampGlobalRange_begin;
        }
        else if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.beginCeiling )
        {
            falling_ampGlobalRange = mx_falling_ampGlobalRange_beginCeiling;
            curving_ampGlobalRange = mx_curving_ampGlobalRange_beginCeiling;
        }
        else if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.beginSpin )
        {
            falling_ampGlobalRange = mx_falling_ampGlobalRange_beginSpin;
            curving_ampGlobalRange = mx_curving_ampGlobalRange_beginSpin;
        }
    }

    void InstantiateOscLists()
    { 
        // all the stupid Vector2s that need to be made through OscMessage.Add()
        osc_falling_transpRange                     = new OscMessage( "/igniter/" + self_id + "/falling/transpRange" );
        osc_falling_delay_timeRange                 = new OscMessage( "/igniter/" + self_id + "/falling/delay/msRange" );
        osc_falling_delay_lineDurRange              = new OscMessage( "/igniter/" + self_id + "/falling/delay/lineDurationRange" );
        osc_curving_casetopRolly_delay_timeRange    = new OscMessage( "/igniter/" + self_id + "/curving/casetopRollyPolly/delay/msRange" );
        osc_curving_casetopRolly_delay_lineDurRange = new OscMessage( "/igniter/" + self_id + "/curving/casetopRollyPolly/delay/lineDurationRange" );
        osc_curving_figure8s_1_transpRange          = new OscMessage( "/igniter/" + self_id + "/curving/figure8s/1/transpRange" );
        osc_curving_figure8s_1_delay_timeRange      = new OscMessage( "/igniter/" + self_id + "/curving/figure8s/1/delay/msRange" );
        osc_curving_figure8s_1_delay_lineDurRange   = new OscMessage( "/igniter/" + self_id + "/curving/figure8s/1/delay/lineDurationRange" );
        osc_curving_figure8s_2_transpRange          = new OscMessage( "/igniter/" + self_id + "/curving/figure8s/2/transpRange" );
        osc_curving_figure8s_2_delay_timeRange      = new OscMessage( "/igniter/" + self_id + "/curving/figure8s/2/delay/msRange" );
        osc_curving_figure8s_2_delay_lineDurRange   = new OscMessage( "/igniter/" + self_id + "/curving/figure8s/2/delay/lineDurationRange" );
    }

    void MixerValues_Init()
    {
        mx_destroyWhileFalling_prob_begin          = mixer.igniter_destroyWhileFalling_prob_begin;
        mx_falling_bufferName                      = mixer.igniter_falling_bufferName;
        mx_falling_delay_timeRange                 = mixer.igniter_falling_delay_timeRange;
        mx_falling_delay_lineDurRange              = mixer.igniter_falling_delay_lineDurRange;
        mx_falling_ampGlobalRange_begin            = mixer.igniter_falling_ampGlobalRange_begin;
        mx_falling_ampGlobalRange_beginCeiling     = mixer.igniter_falling_ampGlobalRange_beginCeiling;
        mx_falling_ampGlobalRange_beginSpin         = mixer.igniter_falling_ampGlobalRange_beginSpin;
        mx_curving_casetopRolly_bufferName         = mixer.igniter_curving_casetopRolly_bufferName;
        mx_curving_figure8s_1_bufferName           = mixer.igniter_curving_figure8s_1_bufferName;
        mx_curving_figure8s_2_bufferName           = mixer.igniter_curving_figure8s_2_bufferName;
        mx_curving_casetopRolly_delay_timeRange    = mixer.igniter_curving_casetopRolly_delay_timeRange;
        mx_curving_figure8s_1_delay_timeRange      = mixer.igniter_curving_figure8s_1_delay_timeRange;
        mx_curving_figure8s_2_delay_timeRange      = mixer.igniter_curving_figure8s_2_delay_timeRange;
        mx_curving_casetopRolly_delay_lineDurRange = mixer.igniter_curving_casetopRolly_delay_lineDurRange;
        mx_curving_figure8s_1_delay_lineDurRange   = mixer.igniter_curving_figure8s_1_delay_lineDurRange;
        mx_curving_figure8s_2_delay_lineDurRange   = mixer.igniter_curving_figure8s_2_delay_lineDurRange;
        mx_curving_ampGlobalRange_begin            = mixer.igniter_curving_ampGlobalRange_begin;
        mx_curving_ampGlobalRange_beginCeiling     = mixer.igniter_curving_ampGlobalRange_beginCeiling;
        mx_falling_ampGlobalRange_beginSpin        = mixer.igniter_curving_ampGlobalRange_beginSpin;
    }

    void ReportOscStart()
    {
        // stupid Vector2s:
        osc_falling_transpRange.Add( aud_falling_transpRange[0] );
        osc_falling_transpRange.Add( aud_falling_transpRange[1] );
        osc_falling_delay_timeRange.Add( mx_falling_delay_timeRange[0] );
        osc_falling_delay_timeRange.Add( mx_falling_delay_timeRange[1] );
        osc_falling_delay_lineDurRange.Add( mx_falling_delay_lineDurRange[0] );
        osc_falling_delay_lineDurRange.Add( mx_falling_delay_lineDurRange[1] );
        osc_curving_casetopRolly_delay_timeRange.Add( mx_curving_casetopRolly_delay_timeRange[0] );
        osc_curving_casetopRolly_delay_timeRange.Add( mx_curving_casetopRolly_delay_timeRange[1] );
        osc_curving_casetopRolly_delay_lineDurRange.Add( mx_curving_casetopRolly_delay_lineDurRange[0] );
        osc_curving_casetopRolly_delay_lineDurRange.Add( mx_curving_casetopRolly_delay_lineDurRange[1] );
        osc_curving_figure8s_1_transpRange.Add( aud_curving_figure8s_1_transpRange[0] );
        osc_curving_figure8s_1_transpRange.Add( aud_curving_figure8s_1_transpRange[1] );
        osc_curving_figure8s_1_delay_timeRange.Add( mx_curving_figure8s_1_delay_timeRange[0] );
        osc_curving_figure8s_1_delay_timeRange.Add( mx_curving_figure8s_1_delay_timeRange[1] );
        osc_curving_figure8s_1_delay_lineDurRange.Add( mx_curving_figure8s_1_delay_lineDurRange[0] );
        osc_curving_figure8s_1_delay_lineDurRange.Add( mx_curving_figure8s_1_delay_lineDurRange[1] );
        osc_curving_figure8s_2_transpRange.Add( aud_curving_figure8s_2_transpRange[0] );
        osc_curving_figure8s_2_transpRange.Add( aud_curving_figure8s_2_transpRange[1] );
        osc_curving_figure8s_2_delay_timeRange.Add( mx_curving_figure8s_2_delay_timeRange[0] );
        osc_curving_figure8s_2_delay_timeRange.Add( mx_curving_figure8s_2_delay_timeRange[1] );
        osc_curving_figure8s_2_delay_lineDurRange.Add( mx_curving_figure8s_2_delay_lineDurRange[0] );
        osc_curving_figure8s_2_delay_lineDurRange.Add( mx_curving_figure8s_2_delay_lineDurRange[1] );

        oscOutScript.Send( "/igniter/" + self_id + "/on", aud_GO_born );
        oscOutScript.Send( "/igniter/" + self_id + "/falling/begin",                        aud_falling_begin );
        oscOutScript.Send( "/igniter/" + self_id + "/falling/bufferName",                   mx_falling_bufferName );
        oscOutScript.Send( "/igniter/" + self_id + "/falling/startDur",                     aud_falling_bufferStartDur );
        oscOutScript.Send( osc_falling_transpRange );
        oscOutScript.Send( osc_falling_delay_timeRange );
        oscOutScript.Send( osc_falling_delay_lineDurRange );
        oscOutScript.Send( "/igniter/" + self_id + "/falling/amp/global",                   aud_falling_globalAmp );

        oscOutScript.Send( "/igniter/" + self_id + "/curving/casetopRollyPolly/bufferName", mx_curving_casetopRolly_bufferName );
        oscOutScript.Send( "/igniter/" + self_id + "/curving/casetopRollyPolly/startDur",   aud_curving_casetopRolly_bufferStartDur );
        oscOutScript.Send( osc_curving_casetopRolly_delay_timeRange );
        oscOutScript.Send( osc_curving_casetopRolly_delay_lineDurRange );
        oscOutScript.Send( "/igniter/" + self_id + "/curving/casetopRollyPolly/amp/global", aud_curving_casetopRolly_globalAmp );

        oscOutScript.Send( "/igniter/" + self_id + "/curving/figure8s/1/bufferName",        mx_curving_figure8s_1_bufferName );
        oscOutScript.Send( "/igniter/" + self_id + "/curving/figure8s/1/startDur",          aud_curving_figure8s_1_bufferStartDur );
        oscOutScript.Send( osc_curving_figure8s_1_transpRange );
        oscOutScript.Send( osc_curving_figure8s_1_delay_timeRange );
        oscOutScript.Send( osc_curving_figure8s_1_delay_lineDurRange );
        oscOutScript.Send( "/igniter/" + self_id + "/curving/figure8s/1/amp/global",        aud_curving_figure8s_1_globalAmp );

        oscOutScript.Send( "/igniter/" + self_id + "/curving/figure8s/2/bufferName",        mx_curving_figure8s_2_bufferName );
        oscOutScript.Send( "/igniter/" + self_id + "/curving/figure8s/2/startDur",          aud_curving_figure8s_2_bufferStartDur );
        oscOutScript.Send( osc_curving_figure8s_2_transpRange );
        oscOutScript.Send( osc_curving_figure8s_2_delay_timeRange );
        oscOutScript.Send( osc_curving_figure8s_2_delay_lineDurRange );
        oscOutScript.Send( "/igniter/" + self_id + "/curving/figure8s/2/amp/global",        aud_curving_figure8s_2_globalAmp );

        aud_GO_born = false;
        aud_falling_begin = false;
    }

    void ReportOscUpdate()
    {
        if( aud_curving_begin == true )
        {
            oscOutScript.Send( "/igniter/" + self_id + "/curving/begin", aud_curving_begin );
            oscOutScript.Send( "/igniter/" + self_id + "/falling/end", aud_falling_end );
            aud_curving_begin = false;
            aud_falling_end = false;
        }
        if( aud_curving_end == true )
        {
            oscOutScript.Send( "/igniter/" + self_id + "/curving/end", aud_curving_end );
            aud_curving_end = false;
        }


        if( aud_GO_destroyed == true )
        {
            oscOutScript.Send( "/igniter/" + self_id + "/off", aud_GO_destroyed );
            aud_GO_destroyed = false;
        }
    }

    void New_FallingTranspRange()
    {
        int randomInt = UnityEngine.Random.Range( 1, 11 );
        switch( randomInt )
        {
            case 1: aud_falling_transpRange = new Vector2( .7f, .78f ); break;
            case 2: aud_falling_transpRange = new Vector2( .8f, .85f ); break;
            case 3: aud_falling_transpRange = new Vector2( 1.0f, 1.1f ); break;
            case 4: aud_falling_transpRange = new Vector2( 1.2f, 1.3f ); break;
            case 5: aud_falling_transpRange = new Vector2( 1.3f, 1.4f ); break;
            case 6: aud_falling_transpRange = new Vector2( 1.4f, 1.5f ); break;
            case 7: aud_falling_transpRange = new Vector2( 1.5f, 1.6f ); break;
            case 8: aud_falling_transpRange = new Vector2( 1.6f, 1.7f ); break;
            case 9: aud_falling_transpRange = new Vector2( 1.7f, 1.8f ); break;
            case 10: aud_falling_transpRange = new Vector2(1.8f, 1.9f); break;
        }
    }

    void New_Curving_figure8s_1_TranspRange()
    {
        int randomInt = UnityEngine.Random.Range( 1, 5 );
        switch( randomInt )
        {
            case 1: aud_curving_figure8s_1_transpRange = new Vector2( .8f, .85f ); break;
            case 2: aud_curving_figure8s_1_transpRange = new Vector2( 1.0f, 1.1f ); break;
            case 3: aud_curving_figure8s_1_transpRange = new Vector2( 1.2f, 1.3f ); break;
            case 4: aud_curving_figure8s_1_transpRange = new Vector2( 1.3f, 1.4f ); break;
        }
    }

    void New_Curving_figure8s_2_TranspRange()
    {
        int randomInt = UnityEngine.Random.Range( 1, 6 );
        switch( randomInt )
        {
            case 1: aud_curving_figure8s_2_transpRange = new Vector2( .7f, .73f ); break;
            case 2: aud_curving_figure8s_2_transpRange = new Vector2( .8f, .85f ); break;
            case 3: aud_curving_figure8s_2_transpRange = new Vector2( 1.0f, 1.1f ); break;
            case 4: aud_curving_figure8s_2_transpRange = new Vector2( 1.2f, 1.3f ); break;
            case 5: aud_curving_figure8s_2_transpRange = new Vector2( 1.3f, 1.4f ); break;
        }
    }
}
