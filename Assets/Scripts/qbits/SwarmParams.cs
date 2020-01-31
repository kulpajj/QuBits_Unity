using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class SwarmParams : MonoBehaviour 
{
    // if you have a swarm of prefabs, but want the each individual to adhere to a particular "color" 
    // shared with the rest of the prefab swarm, script it here; the individuals can check in with this script
    // to know which "color" they should adhere to at a given moment

    // ****
    bool  start = true;
    float randomFloat;
    int   randomInt;

    // ****
    // JASON from max:
    JSONNode sampleInfo_JSONParsed;

    // OSC
    OscOut oscOutScript;

    // ****
    // global evolution:
    GlobalEvolution globalEvolution_script;
    GlobalEvolution.GlobalEvolutionState globalEvolutionState;

    // mixer
    Mixer mixer;

    // ****
    // mixer values
    Vector2 mx_qbit_qtype1Repel_transpRange_high;
    Vector2 mx_qbit_qtype1Repel_transpRange_mid;
    Vector2 mx_qbit_qtype1Repel_transpRange_low;
    Vector2 mx_qbit_qtype1Brownian_transpRange_high;
    Vector2 mx_qbit_qtype1Brownian_transpRange_mid;
    float   mx_qbit_qtype1Brownian_transpRange_low_max;
    Vector2 mx_qbit_qtype1Brownian_transpRange_low_minRandom;
    Vector2 mx_qbit_qtype1Brownian_transp_durTilNextRange;
    string  mx_qbit_qtype1Brownian_bufferPrefix;
    Vector2 mx_void_static_onDurRange;
    Vector2 mx_void_static_offDurRange;
    Vector2 mx_void_upsideDownReveal_on_longer_durRange;
    Vector2 mx_void_upsideDownReveal_on_shorter_durRange;
    float   mx_void_upsideDownReveal_prob_onLonger;
    Vector2 mx_void_upsideDownReveal_off_longer_durRange;
    Vector2 mx_void_upsideDownReveal_off_shorter_durRange;
    float   mx_void_upsideDownReveal_prob_offLonger;
    string  mx_void_upsideDownReveal_bufferPrefix;
    float   mx_void_upsideDownReveal_ampGlobal_begin;
    float   mx_void_upsideDownReveal_ampGlobal_beginSpin;
    Vector2 mx_void_upsideDownReveal_transpRange;

    // ****
    // other scripts
    CameraExplorerParent_Movement cameraExplorerParent_script;
    VoidsAllInfo  voidsAllInfo_script;
    List<Void_Cn> voidsAllInfo;

    // ****
    // qtype1 transposition - for both Brownian and Repel:
    int qbit_qtype1_transpositionState;
    int            qbit_qtype1_transpositionStatePrev;
    public bool    qbit_qtype1_getNewTranspositionRange;
    float          qbit_qtype1_transposition_durTilNext;
    float          qbit_qtype1_transposition_timeStart;
    // qtype1 repel transposition
    public Vector2 qbit_qtype1Repel_transpositionRange;
    // qtype1 brownian transposition
    public Vector2 qbit_qtype1Brownian_transpositionRange;

    // ****
    // brownian density state:
    public bool    qbit_brownianState_getNew;
    int            qbit_brownianState_numStates = 4;
    int            qbit_brownianState_randomState;
    int            qbit_brownianState_randomStatePrev;
    float          qbit_brownianState_durTilNext;
    float          qbit_brownianState_timeStart;
    int            qbit_brownian_numSamples;
    public string  qbit_brownianBuffer_name;
    public float   qbit_brownianBuffer_duration;
    Vector2        qbit_brownianBuffer_durTilNextRange_densest   = new Vector2( 2f, 5f );
    Vector2        qbit_brownianBuffer_durTilNextRange_denser    = new Vector2( 4f, 8f );
    Vector2        qbit_brownianBuffer_durTilNextRange_sparser   = new Vector2( 5f, 9f );
    Vector2        qbit_brownianBuffer_durTilNextRange_sparsest  = new Vector2( 5f, 9f );
    // a range of probability of whether to stop or start moving for each state, so there is more variation in each kind of state
    Vector2        qbit_brownianProb_moveIfStoppedRange_densest  = new Vector2( .007f, .015f  );
    Vector2        qbit_brownianProb_moveIfStoppedRange_denser   = new Vector2( .007f, .015f );
    Vector2        qbit_brownianProb_moveIfStoppedRange_sparser  = new Vector2( .001f, .004f );
    Vector2        qbit_brownianProb_moveIfStoppedRange_sparsest = new Vector2( .0002f, .0007f );
    Vector2        qbit_brownianProb_stopIfMovingRange_densest   = new Vector2( .02f, .025f );
    Vector2        qbit_brownianProb_stopIfMovingRange_denser    = new Vector2( .05f, .06f );
    Vector2        qbit_brownianProb_stopIfMovingRange_sparser   = new Vector2( .03f, .04f );
    Vector2        qbit_brownianProb_stopIfMovingRange_sparsest  = new Vector2( .03f, .04f );
    public float   qbit_brownianProb_moveIfStopped;
    public float   qbit_brownianProb_stopIfMoving;
    // look to the qbitMovement script for what the qtype1_alphaFaded value is and set as the minimum here or near there:
    Vector2        qbit_brownianAlphaRange_densest  = new Vector2( .8f, 1.1f );
    Vector2        qbit_brownianAlphaRange_denser   = new Vector2( .7f, .94f );
    Vector2        qbit_brownianAlphaRange_sparser  = new Vector2( .7f, .9f );
    Vector2        qbit_brownianAlphaRange_sparsest = new Vector2( .7f, .9f );
    public Vector2 qbit_brownianAlphaRange_current;
    // localAmp in max:
    public Vector2 qbit_brownianReportedVelocityRange;
    Vector2        qbit_brownianReportedVelocityRange_densest;
    Vector2        qbit_brownianReportedVelocityRange_denser;
    Vector2        qbit_brownianReportedVelocityRange_sparser;
    Vector2        qbit_brownianReportedVelocityRange_sparsest;

    // ****
    // void static on/off
    OnOff_Cn     void_static_onOffCn;
    OnOffTracker void_static_onOffTracker;
    public bool  void_static_on;
    bool         void_static_initOn = true;

    // ****
    // void upsideDownReveal on/off and growAndFade
    OnOff_Cn             void_upsideDownReveal_onOffCn;
    OnOffTrackerWeighted void_upsideDownReveal_onOffTracker;
    GrowAndFade          void_upsideDownReveal_growAndFade;
    public bool          void_upsideDownReveal_on;
    float                void_upsideDownReveal_eventOnDur;
    bool                 void_upsideDownReveal_initOn = true;
    float                void_upsideDownReveal_growDur;
    float                void_upsideDownReveal_stayDur;
    float                void_upsideDownReveal_fadeDur;
    int                  void_upsideDownReveal_polyID;
    float                void_upsideDownReveal_ampGlobal;
    public float         void_upsideDownReveal_ampLocal;
    float                void_upsideDownReveal_prerecorded_playPos;
    float                void_upsideDownReveal_prerecorded_brownianBufferDur;
    float                void_upsideDownReveal_transp;
        // for old realtime granular version
    float                void_upsideDownReveal_realtimeGran_yshape;

    void Start() 
    {
        // find scripts
        sampleInfo_JSONParsed = GameObject.Find( "loadJSON_fromMax" ).GetComponent<LoadJSON_FromMax>().sampleInfo_JSONParsed;
        globalEvolution_script = GameObject.Find( "globalEvolution" ).GetComponent<GlobalEvolution>();
        oscOutScript = GameObject.Find( "osc" ).GetComponent<OscOut>();
        cameraExplorerParent_script = GameObject.Find( "camera_explorer_parent" ).GetComponent<CameraExplorerParent_Movement>();
        voidsAllInfo_script = GameObject.Find( "voidsAllInfo" ).GetComponent<VoidsAllInfo>();

        // init new() shit
        mixer                    = new Mixer();
        void_static_onOffCn      = new OnOff_Cn();
        void_static_onOffTracker = new OnOffTracker();
        void_upsideDownReveal_onOffCn      = new OnOff_Cn();
        void_upsideDownReveal_onOffTracker = new OnOffTrackerWeighted();

        // evolution and mixer
        globalEvolutionState = GlobalEvolution.GlobalEvolutionState.begin;
        EvolutionParams();
        MixerValues_Init();

        // get the methods goin'
        New_Qbit_Qtype1_Transposition();
        New_Qbit_Brownian_State();

        // init Prevs

        // json data
        void_upsideDownReveal_prerecorded_brownianBufferDur = sampleInfo_JSONParsed[ "upsideDownReveal/1/duration" ];

        // start over for Methods()
        start = false;
    }
	
	void Update() 
    {
        voidsAllInfo = voidsAllInfo_script.voidsAllInfo; //<-- for whether to sned upsideDownReveal or not
        globalEvolutionState = globalEvolution_script.globalEvolutionState;
        EvolutionParams();

        // rolly transposition
        if ( Time.time - qbit_qtype1_transposition_timeStart >= qbit_qtype1_transposition_durTilNext )
        {
            qbit_qtype1_getNewTranspositionRange = true;
            New_Qbit_Qtype1_Transposition();
        }
        else { qbit_qtype1_getNewTranspositionRange = false; }

        // brownian state
        if( Time.time - qbit_brownianState_timeStart >= qbit_brownianState_durTilNext )
        {
            qbit_brownianState_getNew = true;
            New_Qbit_Brownian_State();
        }
        else { qbit_brownianState_getNew = false; }

        // void static on/off
        Void_Static_OnOff();

        // void upsideDownReveal on/off - don't do it in the actual upsideDown
        if( cameraExplorerParent_script.aud_upsideDown == false )
        {
            Void_UpsideDownReveal_OnOff();
        }

        ReportOsc();
    }

    //***********************
    void New_Qbit_Qtype1_Transposition()
    {
        if( start == true )
        {
            qbit_qtype1Repel_transpositionRange    = new Vector2( .98f, 1f );
            qbit_qtype1Brownian_transpositionRange = new Vector2( .8f, .9f );
            qbit_qtype1_transpositionStatePrev = 1;
        }
        else
        {
            qbit_qtype1_transpositionState = Random.Range( 1, 4 );
            if( qbit_qtype1_transpositionState == 3 && qbit_qtype1_transpositionStatePrev == 3 )
            {
                qbit_qtype1_transpositionState = Random.Range( 1, 3 );
            }


            if ( qbit_qtype1_transpositionState == 1 )
            {
                // high
                qbit_qtype1Repel_transpositionRange    = mx_qbit_qtype1Repel_transpRange_high;
                qbit_qtype1Brownian_transpositionRange = mx_qbit_qtype1Brownian_transpRange_high;
            }
            else if( qbit_qtype1_transpositionState == 2 )
            {
                // mid
                qbit_qtype1Repel_transpositionRange    = mx_qbit_qtype1Repel_transpRange_mid;
                qbit_qtype1Brownian_transpositionRange = mx_qbit_qtype1Brownian_transpRange_mid;
            }
            else if( qbit_qtype1_transpositionState == 3 )
            {
                // low
                qbit_qtype1Repel_transpositionRange    = mx_qbit_qtype1Repel_transpRange_low;
                qbit_qtype1Brownian_transpositionRange = new Vector2( Random.Range( mx_qbit_qtype1Brownian_transpRange_low_minRandom[0], mx_qbit_qtype1Brownian_transpRange_low_minRandom[1] ), mx_qbit_qtype1Brownian_transpRange_low_max );
            } 
            // Debug.Log(qbit_qtype1_transpositionState);
        }

        qbit_qtype1_transposition_timeStart = Time.time;
        qbit_qtype1_transposition_durTilNext = Random.Range( mx_qbit_qtype1Brownian_transp_durTilNextRange[0], mx_qbit_qtype1Brownian_transp_durTilNextRange[1] );
        qbit_qtype1_transpositionStatePrev = qbit_qtype1_transpositionState;
    }

    //***********************
    void New_Qbit_Brownian_State()
    {
        if( start == true )
        {
            // randomState is the same as the buffer number, see comments below on how this corresponds to density
            qbit_brownianState_randomState = 2;
            qbit_brownianBuffer_name =  mx_qbit_qtype1Brownian_bufferPrefix + ".2";
            // Debug.Log(qbit_brownianBuffer_name);
            qbit_brownianBuffer_duration = sampleInfo_JSONParsed[ mx_qbit_qtype1Brownian_bufferPrefix + "/2/duration" ];
        }
        else
        {
            qbit_brownian_numSamples = sampleInfo_JSONParsed[ mx_qbit_qtype1Brownian_bufferPrefix + "/count" ];
            qbit_brownianState_randomState = Random.Range( 1, qbit_brownianState_numStates + 1 );
            if( qbit_brownianState_randomState == qbit_brownianState_randomStatePrev )
            {
                if( qbit_brownianState_randomState != 2 )
                {
                    // sparsest and dense always % + 1, i.e. 1 goes to 2 and 3 goes to 1
                    qbit_brownianState_randomState = ( qbit_brownianState_randomState % qbit_brownian_numSamples ) + 1;
                }
                else
                {
                    // BUT, allow 2 (sparser) to adjust to either 1 or 3, or it will be much less often to ever get a 1, i.e. dense 
                    randomInt = Random.Range( 1, 2 );
                    if( randomInt == 1 ) { qbit_brownianState_randomState = 1; }
                    else                 { qbit_brownianState_randomState = 3; }
                }
            }
            if( ( qbit_brownianState_randomStatePrev == 3 || qbit_brownianState_randomStatePrev == 4 ) && qbit_brownianState_randomState == 1 )
            {
                // don't go from lowest to highest densities cuz sounds dumb; make it pass through 2, medium density
                qbit_brownianState_randomState = 2;
            }

            if( qbit_brownianState_randomState == 1 || qbit_brownianState_randomState == 2 )
            {
                qbit_brownianBuffer_name = mx_qbit_qtype1Brownian_bufferPrefix + "." + qbit_brownianState_randomState; //<--- state/density is same as buffer index
                qbit_brownianBuffer_duration = sampleInfo_JSONParsed[ mx_qbit_qtype1Brownian_bufferPrefix + "/" + qbit_brownianState_randomState + "/duration" ];
            }
            else
            {
                qbit_brownianBuffer_name = mx_qbit_qtype1Brownian_bufferPrefix + ".3"; //<---- both of the sparsest states use this file
                qbit_brownianBuffer_duration = sampleInfo_JSONParsed[ mx_qbit_qtype1Brownian_bufferPrefix + "/3/duration" ];
            }
        }

        // *****
        // Debug.Log( "brownianState " + qbit_brownianState_randomState );

        qbit_brownianState_timeStart = Time.time;
        // in my folder, the densest sample is the first sample, and thus qbit_rollypolly.1
        //               the sparsest sample is the last sample, and thus qbit_rollypolly.3
        //               the 2nd indexed file is sparser but not sparsest...thus: 
        if( qbit_brownianState_randomState == 1 )
        {
            // densest
            qbit_brownianState_durTilNext   = Random.Range( qbit_brownianBuffer_durTilNextRange_densest[0], qbit_brownianBuffer_durTilNextRange_densest[1] );
            qbit_brownianProb_moveIfStopped = Random.Range( qbit_brownianProb_moveIfStoppedRange_densest[0], qbit_brownianProb_moveIfStoppedRange_densest[1] );
            qbit_brownianProb_stopIfMoving  = Random.Range( qbit_brownianProb_stopIfMovingRange_densest[0], qbit_brownianProb_stopIfMovingRange_densest[1] );
            qbit_brownianAlphaRange_current = qbit_brownianAlphaRange_densest;
            qbit_brownianReportedVelocityRange = qbit_brownianReportedVelocityRange_densest;
        }
        else if( qbit_brownianState_randomState == 2 )
        {
            // denser
            qbit_brownianState_durTilNext = Random.Range( qbit_brownianBuffer_durTilNextRange_denser[0], qbit_brownianBuffer_durTilNextRange_denser[1] );
            qbit_brownianProb_moveIfStopped = Random.Range( qbit_brownianProb_moveIfStoppedRange_denser[0], qbit_brownianProb_moveIfStoppedRange_denser[1] );
            qbit_brownianProb_stopIfMoving = Random.Range( qbit_brownianProb_stopIfMovingRange_denser[0], qbit_brownianProb_stopIfMovingRange_denser[1] );
            qbit_brownianAlphaRange_current = qbit_brownianAlphaRange_denser;
            qbit_brownianReportedVelocityRange = qbit_brownianReportedVelocityRange_denser;
        }
        else if( qbit_brownianState_randomState == 3 )
        {
            // sparser
            qbit_brownianState_durTilNext = Random.Range( qbit_brownianBuffer_durTilNextRange_sparser[0], qbit_brownianBuffer_durTilNextRange_sparser[1] );
            qbit_brownianProb_moveIfStopped = Random.Range( qbit_brownianProb_moveIfStoppedRange_sparser[0], qbit_brownianProb_moveIfStoppedRange_sparser[1] );
            qbit_brownianProb_stopIfMoving = Random.Range( qbit_brownianProb_stopIfMovingRange_sparser[0], qbit_brownianProb_stopIfMovingRange_sparser[1] );
            qbit_brownianAlphaRange_current = qbit_brownianAlphaRange_sparser;
            qbit_brownianReportedVelocityRange = qbit_brownianReportedVelocityRange_sparser;
        }
        else if( qbit_brownianState_randomState == 4 )
        {
            // sparsest
            qbit_brownianState_durTilNext = Random.Range( qbit_brownianBuffer_durTilNextRange_sparsest[0], qbit_brownianBuffer_durTilNextRange_sparsest[1] );
            qbit_brownianProb_moveIfStopped = Random.Range( qbit_brownianProb_moveIfStoppedRange_sparsest[0], qbit_brownianProb_moveIfStoppedRange_sparsest[1] );
            qbit_brownianProb_stopIfMoving = Random.Range( qbit_brownianProb_stopIfMovingRange_sparsest[0], qbit_brownianProb_stopIfMovingRange_sparsest[1] );
            qbit_brownianAlphaRange_current = qbit_brownianAlphaRange_sparsest;
            qbit_brownianReportedVelocityRange = qbit_brownianReportedVelocityRange_sparsest;
        }

        qbit_brownianState_randomStatePrev = qbit_brownianState_randomState;
    }

    //***********************
    void Void_Static_OnOff()
    {
        void_static_onOffCn = void_static_onOffTracker.Return_OnOff_Cn( void_static_initOn, mx_void_static_onDurRange, mx_void_static_offDurRange, 1f ); //<--- we don't care bout fadeOut - just give it any dur
        void_static_on      = void_static_onOffCn.on;
    }

    //***********************
    void Void_UpsideDownReveal_OnOff()
    {
        // here's a nice coordination between OnOffTrackerWeighted and GrowAndFade
        // we get the onDur and divide it into the proportions we want for grow, stay, and fade
        void_upsideDownReveal_onOffCn = void_upsideDownReveal_onOffTracker.Return_OnOff_Cn( void_upsideDownReveal_initOn, mx_void_upsideDownReveal_on_shorter_durRange, mx_void_upsideDownReveal_on_longer_durRange, mx_void_upsideDownReveal_prob_onLonger, mx_void_upsideDownReveal_off_shorter_durRange, mx_void_upsideDownReveal_off_longer_durRange, mx_void_upsideDownReveal_prob_offLonger, 1f );
        void_upsideDownReveal_on = void_upsideDownReveal_onOffCn.on;
        if( void_upsideDownReveal_on == true )
        {
            if( void_upsideDownReveal_onOffCn.onClick == true )
            {
                void_upsideDownReveal_eventOnDur = void_upsideDownReveal_onOffCn.onDur;
                void_upsideDownReveal_growAndFade = new GrowAndFade();
                // just make sure all adds to 1.
                void_upsideDownReveal_growDur = void_upsideDownReveal_eventOnDur * .35f;
                void_upsideDownReveal_stayDur = void_upsideDownReveal_eventOnDur * .15f;
                void_upsideDownReveal_fadeDur = void_upsideDownReveal_eventOnDur * .5f;
                void_upsideDownReveal_prerecorded_playPos = Random.Range( 0, void_upsideDownReveal_prerecorded_brownianBufferDur );
                void_upsideDownReveal_transp = Random.Range( mx_void_upsideDownReveal_transpRange[0], mx_void_upsideDownReveal_transpRange[1] );
            }

            void_upsideDownReveal_ampLocal = void_upsideDownReveal_growAndFade.ReturnCurrentValue( false, false, new Vector2( .7f, 1f ), Vector2.one * 0, Vector2.one * void_upsideDownReveal_growDur, Vector2.one * void_upsideDownReveal_fadeDur, Vector2.one * void_upsideDownReveal_stayDur, Vector2.one * 0f );
        }
    }

    //****************************************
    // ><>  ><>  ><>  ><>
    void EvolutionParams()
    {
        if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.begin )
        {
            // equal amp
            qbit_brownianReportedVelocityRange_densest  = new Vector2( .0000005f, .00025f );
            qbit_brownianReportedVelocityRange_denser   = new Vector2( .0000005f, .00025f );
            qbit_brownianReportedVelocityRange_sparser  = new Vector2( .0000005f, .00025f );
            qbit_brownianReportedVelocityRange_sparsest = new Vector2( .00001f,   .00075f );
            void_upsideDownReveal_ampGlobal = mx_void_upsideDownReveal_ampGlobal_begin;
        }
        else if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.beginCeiling )
        {
            // bring out denser in volume??
            qbit_brownianReportedVelocityRange_densest  = new Vector2( .0000005f, .00025f );
            qbit_brownianReportedVelocityRange_denser   = new Vector2( .0000005f, .00025f );
            qbit_brownianReportedVelocityRange_sparser  = new Vector2( .0000005f, .00025f );
            qbit_brownianReportedVelocityRange_sparsest = new Vector2( .00001f,   .00075f );
            void_upsideDownReveal_ampGlobal = mx_void_upsideDownReveal_ampGlobal_begin;
        }
        else if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.beginSpin )
        {
            void_upsideDownReveal_ampGlobal = mx_void_upsideDownReveal_ampGlobal_beginSpin;
        }
    }

    void MixerValues_Init()
    {
        mx_qbit_qtype1Repel_transpRange_high                         = mixer.qbit_qtype1Repel_transpRange_high;
        mx_qbit_qtype1Repel_transpRange_mid                          = mixer.qbit_qtype1Repel_transpRange_mid;
        mx_qbit_qtype1Repel_transpRange_low                          = mixer.qbit_qtype1Repel_transpRange_low;
        mx_qbit_qtype1Brownian_transpRange_high                      = mixer.qbit_qtype1Brownian_transpRange_high;
        mx_qbit_qtype1Brownian_transpRange_mid                       = mixer.qbit_qtype1Brownian_transpRange_mid;
        mx_qbit_qtype1Brownian_transpRange_low_max                   = mixer.qbit_qtype1Brownian_transpRange_low_max;
        mx_qbit_qtype1Brownian_transpRange_low_minRandom             = mixer.qbit_qtype1Brownian_transpRange_low_minRandom;
        mx_qbit_qtype1Brownian_transp_durTilNextRange                = mixer.qbit_qtype1Brownian_transp_durTilNextRange;
        mx_qbit_qtype1Brownian_bufferPrefix                          = mixer.qbit_qtype1Brownian_bufferPrefix;
        mx_void_static_onDurRange                                    = mixer.void_static_onDurRange;
        mx_void_static_offDurRange                                   = mixer.void_static_offDurRange;
        mx_void_upsideDownReveal_on_longer_durRange                  = mixer.void_upsideDownReveal_on_longer_durRange;
        mx_void_upsideDownReveal_on_shorter_durRange                 = mixer.void_upsideDownReveal_on_shorter_durRange;
        mx_void_upsideDownReveal_prob_onLonger                       = mixer.void_upsideDownReveal_prob_onLonger;
        mx_void_upsideDownReveal_off_longer_durRange                 = mixer.void_upsideDownReveal_off_longer_durRange;
        mx_void_upsideDownReveal_off_shorter_durRange                = mixer.void_upsideDownReveal_off_shorter_durRange;
        mx_void_upsideDownReveal_prob_offLonger                      = mixer.void_upsideDownReveal_prob_offLonger;
        mx_void_upsideDownReveal_bufferPrefix                        = mixer.void_upsideDownReveal_bufferPrefix;
        mx_void_upsideDownReveal_ampGlobal_begin                     = mixer.void_upsideDownReveal_ampGlobal_begin;
        mx_void_upsideDownReveal_ampGlobal_beginSpin                 = mixer.void_upsideDownReveal_ampGlobal_beginSpin;
        mx_void_upsideDownReveal_transpRange                         = mixer.void_upsideDownReveal_transpRange;
    }

    void ReportOsc()
    {
        if( voidsAllInfo.Count != 0 )
        {
            if( void_upsideDownReveal_onOffCn.on == true )
            {
                if( void_upsideDownReveal_onOffCn.onClick == true )
                {
                    void_upsideDownReveal_polyID = 1;

                    oscOutScript.Send( "/theUpsideDown/reveal/" + void_upsideDownReveal_polyID + "/on",           1 );
                    oscOutScript.Send( "/theUpsideDown/reveal/" + void_upsideDownReveal_polyID + "/amp/global",   void_upsideDownReveal_ampGlobal );
                    oscOutScript.Send( "/theUpsideDown/reveal/" + void_upsideDownReveal_polyID + "/buffer",       mx_void_upsideDownReveal_bufferPrefix + ".1" );
                    oscOutScript.Send( "/theUpsideDown/reveal/" + void_upsideDownReveal_polyID + "/playPos",      void_upsideDownReveal_prerecorded_playPos );
                    oscOutScript.Send( "/theUpsideDown/reveal/" + void_upsideDownReveal_polyID + "/transp",       void_upsideDownReveal_transp );
                    oscOutScript.Send( "/theUpsideDown/reveal/" + void_upsideDownReveal_polyID + "/downsamp/end", 1 );
                }
                oscOutScript.Send( "/theUpsideDown/reveal/" + void_upsideDownReveal_polyID +  "/amp/local", void_upsideDownReveal_ampLocal );

            }
            else if( void_upsideDownReveal_onOffCn.offClick == true )
            {
                oscOutScript.Send( "/theUpsideDown/reveal/" + void_upsideDownReveal_polyID + "/off",             1 );
            }
        }
    }
}
