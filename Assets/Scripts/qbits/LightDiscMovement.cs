using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightDiscMovement : MonoBehaviour 
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

    // global evolution
    GlobalEvolution globalEvolution_script;
    GlobalEvolution.GlobalEvolutionState globalEvolutionState;
    GlobalEvolution.GlobalEvolutionState globalEvolutionStatePrev;

    // osc
    OscOut oscOutScript;
    OscMessage osc_freqsFlare;
    OscMessage osc_ampsFlare;
    OscMessage osc_decaysFlare;
    OscMessage osc_freqsSpin;
    OscMessage osc_ampsSpin;
    OscMessage osc_decaysSpin;

    // mixer
    Mixer mixer;

    // score for discs values actually come from the one and only instance of the ScoreDiscs class that exists in the MakeDiscsModel script
    DiscModelParams_Cn discModelParams;

    // ****
    // mixer values
    float   mx_lightFlareEvent_ampGlobal_begin;
    float   mx_lightFlareEvent_ampGlobal_beginSpin;
    float   mx_lightFlareEvent_ampGlobal_scaleValuesInScore_lowerAndRougher;
    float   mx_lightSpinEvent_ampGlobal_scaleValuesInScore_lowerAndRougher;
    Vector2 mx_lightFlareEvent_durationRangePerSwell_begin;
    float   mx_lightSpinEvent_prob_begin; // DELETE all spinProbs
    float   mx_lightSpinEvent_prob_beginSpin;
    float   mx_lightSpinEvent_resonance_ampGlobal;
    float   mx_lightSpinEvent_attack_ampGlobal;
    float   mx_lightSpinEvent_attack_prob;
    Vector2 mx_lightSpinEvent_fund_percentOfCriticalBandRange;
    Vector2 mx_flare_randAmp_softerRange;
    Vector2 mx_flare_randAmp_louderRange;
    Vector2 mx_spin_randAmp_softerRange;
    Vector2 mx_spin_randAmp_louderRange;
    Vector2 mx_randDecay_fasterRange;
    Vector2 mx_randDecay_slowerRange;
    int     mx_flare_lowerAndRougher_maxNumFlares;
    float   mx_flare_lowerAndRougher_growAndFade_probInitStayPut;
    Vector2 mx_flare_lowerAndRougher_growAndFade_targetFadeRange;
    Vector2 mx_flare_lowerAndRougher_growAndFade_targetGrowRange;
    Vector2 mx_flare_lowerAndRougher_growAndFade_durToFadeRange;
    Vector2 mx_flare_lowerAndRougher_growAndFade_durToGrowRange;
    Vector2 mx_flare_lowerAndRougher_growAndFade_durToStayFadedRange;
    Vector2 mx_flare_lowerAndRougher_growAndFade_durToStayGrownRange;

    // disc score values that actually come from the one and only instance of the ScoreDiscs class that exists in the MakeDiscsModel script
    Vector2 sc_lightFlareEvent_shimmer_percentOfCriticalBandRange;

    // additional values to audio/Max
    bool   aud_attack;
    float  aud_ampLocal;
    bool   aud_lightSpinEvent_start;
    bool   aud_lightSpinEvent_end;
    string aud_lightEventType;
    bool   aud_lightFlareEvent_start;
    bool   aud_lightFlareEvent_end;
    float  aud_lightFlareEvent_ampGlobal;
    float  aud_lightSpinEvent_resonance_ampGlobal;
    List<float> lightFlareEvent_freqs;
    List<float> lightFlareEvent_amps;
    List<float> lightFlareEvent_decays;
    List<float> lightSpinEvent_freqs;
    List<float> lightSpinEvent_amps;
    List<float> lightSpinEvent_decays;

    // delegates
    delegate void GeyserCollision();
    delegate void FlareHandling();
    delegate void SelectDiscModel();
    GeyserCollision  del_geyserCollision;
    FlareHandling    del_flareHandling;
    SelectDiscModel  del_selectDiscModel;

    public int self_id;
    Light lightComponent;
    Rigidbody rigidbodyComponent;
    Quaternion transform_rotationPrev;

    MakeDiscsModel makeDiscsModel_script;
    DiscModel_Cn discModel_selected;
    public List<float> modelFreqs;
    public List<float> modelAmps;
    public List<float> modelDecays;
    public DiscModelParams_Cn.ModelTransposition modelTransposition;
    GrowAndFade lowerAndRougher_growAndFade;

    LightDiscs_NumBusyFlare lightDiscs_numBusyFlare_script;

    bool collisionGeyser;
    public bool collisionQbit; // the qbit filament sets this to true
    RandomGeysers geyserScript;
    float geyserSpeed;
    Vector2 geyser_startSpeedRange;
    public bool busyFlare;
    public bool busySpin;
    float   lightSpinEvent_prob; // DELETE all spinProbs

    Vector2 ampLocalRange = new Vector2( 0f, 1f );
    Vector2 randAmp_softerRange;
    Vector2 randAmp_louderRange;

    Vector2 lightFlareEvent_durationRangePerSwell;
    int     lightFlareEvent_1or2swells;
    int     lightFlareEvent_swellsCounter;
    Vector2 lightFlareEvent_intensityRange_beginCeiling    = new Vector2( .12f, 1.5f );
    Vector2 lightFlareEvent_intensityRange_lowerAndRougher = new Vector2( .12f, 1.1f );
    int     discsModel_indexRandom;
    bool    lightFlareEvent_transposeDownAnOctave;
    bool    flare_lowerAndRougher_growAndFade_initStayPut;

    Vector2 lightSpinEvent_intensityRange = new Vector2( .5f, 2.8f );
    float   lightSpinEvent_spinDelta;
    float   lightSpinEvent_spinDeltaStopThresh = .003f;
    int     lightSpinEvent_modelIndex;
    int     lightSpin_frameStepSize = 3;
    int     lightSpin_frameCounter;

    float criticalBand;
    float half_criticalBand;
    float percentOfCriticalBand;
    // there are two, one on each side of the center freq, but we'll just use the one above
    float percentOfCriticalBandFreq;
    // Vector2 rangeAround_percentOfCriticalBandFreq;
    // float randomFreqNear_percentOfCriticalBandFreq;

    // the actual min and max scale the velocity of the geyser that hit it to the range
    float  lightIntensity_min;
    float  lightIntensity_max;
    float  lightIntensity_phraseDuration;
    float  lightIntensity_startTime;
    float  lightIntensity_currentTime;
    float  lightIntensity_phase;
    float  lightIntensity_startValue;
    float  lightIntensity_destinationValue;
    float  lightIntensity_current;
    string lightIntensity_increase_or_decrease;

    // troubleshooting
    bool beginSpin;

    void Start()
    {
        oscOutScript = GameObject.Find( "osc" ).GetComponent<OscOut>();
        lightComponent = GetComponent<Light>();
        rigidbodyComponent = GetComponent<Rigidbody>();

        makeDiscsModel_script  = GameObject.Find( "makeDiscsModel" ).GetComponent<MakeDiscsModel>();
        globalEvolution_script = GameObject.Find( "globalEvolution" ).GetComponent<GlobalEvolution>();
        lightDiscs_numBusyFlare_script = GameObject.Find( "lightDiscs_numBusyFlare" ).GetComponent<LightDiscs_NumBusyFlare>();

        mixer = new Mixer();
        MixerValues_Init();
    }

    void Update() 
    {
        // ><>  ><>  ><>  ><>
        globalEvolutionState = globalEvolution_script.globalEvolutionState;
        EvolutionParams();
        // ><>  ><>  ><>  ><>
        selectedGO = UnityEditor.Selection.activeGameObject;

        // flares cannot interrupt other flares or spins
            // beginCeiling, geyser collisions cause the lightFlareEvents to begin
            // lowerAndRougher, geyser collisions do nothing - see its BusyFlare_lowerAndRougher for both beginnings and proceedings
            // this delegate structure is left here in case want to make the geysers do something during lowerAndRougher, i.e. see GeyserCollision_lowerAndRougher()
        if( busyFlare == false && busySpin == false )
        {
            if( collisionGeyser == true )
            {
                del_geyserCollision();
            }
        }

        // spins can interrupt flares and other spins
        if( collisionQbit == true )
        {
            StartLightSpinEvent();
        }

        // flares add all freqs at event beginning ( including beatings )
        // spins add diff freqs every frame 
        del_flareHandling();

        if( busySpin == true )
        {
            BusySpin();
        }

        ReportOsc();

        globalEvolutionStatePrev = globalEvolutionState;
    }

    // DESIGN: the qbit has the is trigger collider, not the light disc 
    //         the radius of the qbit tree collision influence is determined by the radius of the capsule collider on the tree ( each filament on the qbit prefab )
    void OnParticleCollision( GameObject other )
    {
        if( busyFlare == false )
        {
            collisionGeyser = true;
            geyserSpeed = other.GetComponent<ParticleSystem>().main.startSpeed.constant;
            geyserScript = other.GetComponent<RandomGeysers>();
        }
    }

    void StartLightFlareSwell_BeginCeiling()
    {
        aud_lightEventType = "flare";
        aud_lightFlareEvent_start = true;
        lightComponent.enabled = true;
        lightComponent.type = LightType.Point;
        lightIntensity_min = lightFlareEvent_intensityRange_beginCeiling[0];
        lightIntensity_max = Scale( geyserSpeed, geyser_startSpeedRange[0], geyser_startSpeedRange[1], lightFlareEvent_intensityRange_beginCeiling[0], lightFlareEvent_intensityRange_beginCeiling[1] );
        lightIntensity_phraseDuration = Scale( geyserSpeed, geyser_startSpeedRange[0], geyser_startSpeedRange[1], lightFlareEvent_durationRangePerSwell[0], lightFlareEvent_durationRangePerSwell[1] );
        lightIntensity_increase_or_decrease = "increase";
        lightIntensity_startTime = Time.time;

        del_selectDiscModel();
        // model freqs as-is or transpose down an octave...all the additional freqs will then also be transposed down an octave 
        //       bool transposeDownAnOctave set in EvolutionParams()
        if( lightFlareEvent_transposeDownAnOctave == true )
        {
            for( int f = 0; f < modelFreqs.Count; f++ )
            {
                modelFreqs[ f ] /= 4;
            }
        }
        Beatings_FlareEvent();
    }

    void StartLightFlareSwell_LowerAndRougher()
    {
        aud_lightEventType = "flare";
        aud_lightFlareEvent_start = true;
        lightComponent.enabled = true;
        lightComponent.type = LightType.Point;
        lightComponent.color = new Color( .66f, Random.Range( .3f, .8f ), .95f, 1f ); // but randomize the G value between .3 and .76

        del_selectDiscModel();

        //Debug.Log( makeDiscsModel_script.discsModel[0].freqs[0] );

        // transp requests set by ScoreDiscs
        int flipCoin = Random.Range(0, 2);
        if( modelTransposition == DiscModelParams_Cn.ModelTransposition.oneOctaveLower )
        {
            for( int f = 0; f < modelFreqs.Count; f++ )
            {
                modelFreqs[ f ] /= 2;
            }
        }
        else if( modelTransposition == DiscModelParams_Cn.ModelTransposition.twoOctavesLower )
        {
            for( int f = 0; f < modelFreqs.Count; f++ )
            {
                modelFreqs[ f ] /= 4;
            }
        }
        else if( modelTransposition == DiscModelParams_Cn.ModelTransposition.oneOrTwoOctavesLower )
        {
            if( flipCoin == 0 )
            {
                for( int f = 0; f < modelFreqs.Count; f++ )
                {
                    modelFreqs[ f ] /= 2;
                }
            }
            else
            {
                for( int f = 0; f < modelFreqs.Count; f++ )
                {
                    modelFreqs[ f ] /= 4;
                }
            }
        }
        else if( modelTransposition == DiscModelParams_Cn.ModelTransposition.noneOrOneOctaveLower )
        {
            if( flipCoin == 0 )
            {
                for( int f = 0; f < modelFreqs.Count; f++ )
                {
                    modelFreqs[ f ] /= 2;
                }
            }
        }
        else if( modelTransposition == DiscModelParams_Cn.ModelTransposition.noneOrTwoOctavesLower )
        {
            if( flipCoin == 0 )
            {
                for( int f = 0; f < modelFreqs.Count; f++ )
                {
                    modelFreqs[ f ] /= 4;
                }
            }
        }
        else if( modelTransposition == DiscModelParams_Cn.ModelTransposition.noneOrOneOctaveHigher )
        {
            if( flipCoin == 0 )
            {
                for( int f = 0; f < modelFreqs.Count; f++ )
                {
                    modelFreqs[ f ] *= 2;
                }
            }
        }
        else if( modelTransposition == DiscModelParams_Cn.ModelTransposition.oneOctaveHigher )
        {
            for( int f = 0; f < modelFreqs.Count; f++ )
            {
                modelFreqs[ f ] *= 2;
            }
        }


        Beatings_FlareEvent();
    }

    void GeyserCollision_BeginCeiling()
    {
        // begin the lightFlareEvent
        collisionGeyser = false;
        busyFlare = true;
        geyser_startSpeedRange = geyserScript.startSpeedRange;

        lightFlareEvent_1or2swells = Random.Range( 1, 3 );
        lightFlareEvent_swellsCounter = 1;
        StartLightFlareSwell_BeginCeiling();
    }

    void GeyserCollision_LowerAndRougher()
    {
        // do nothing
        // for LowerAndRougher, event beginnings and proceedings are all handled by an EventRandTracker in its BusyFlare method
        // ( event beginnings are not determined by geyser collisions ) 
    }

    void StartLightSpinEvent()
    {
        // spins can interrupt flares and other spins
        busyFlare = false;
        busySpin = true;
        beginSpin = true;
        collisionQbit = false;

        aud_lightEventType = "spin";
        aud_lightSpinEvent_start = true;
        lightComponent.enabled = true;
        lightComponent.type = LightType.Spot;
        // for max: whether to play the pin sample attack and in max determines the amp env for whether we hear the click~ excitation
        DetermineSpinAttack();
        del_selectDiscModel(); // TODO TODO TODO make a separate SelectDiscModel_Spin?
    }

    void FlareHandling_BeginCeiling()
    {
        if( busyFlare == true )
        {
            // for BeginCeiling, event beginnings are determined by del_GeyserCollision()
            // thus this delegate is just about amplitude and light intensity
            // this delegate makes it so can have 1 or 2 flares per event
            lightIntensity_currentTime = Time.time - lightIntensity_startTime;
            if( lightIntensity_currentTime >= lightIntensity_phraseDuration / 2 )
            {
                lightIntensity_increase_or_decrease = "decrease";
            }

            if( lightIntensity_increase_or_decrease == "increase" )
            {
                lightIntensity_startValue = lightIntensity_min;
                lightIntensity_destinationValue = lightIntensity_max;
                lightIntensity_phase = lightIntensity_currentTime / ( lightIntensity_phraseDuration / 2 );
            }
            else
            {
                lightIntensity_startValue = lightIntensity_max;
                lightIntensity_destinationValue = lightIntensity_min;
                lightIntensity_phase = ( lightIntensity_currentTime - ( lightIntensity_phraseDuration / 2 ) ) / ( lightIntensity_phraseDuration / 2 );
            }

            lightIntensity_current = Mathf.Lerp( lightIntensity_startValue, lightIntensity_destinationValue, lightIntensity_phase );
            lightComponent.intensity = lightIntensity_current;
            aud_ampLocal = Scale( lightIntensity_current, lightFlareEvent_intensityRange_beginCeiling[0], lightFlareEvent_intensityRange_beginCeiling[1], 0f, 1f );
            if( lightIntensity_increase_or_decrease == "decrease" && lightIntensity_currentTime >= lightIntensity_phraseDuration )
            {
                if( lightFlareEvent_1or2swells == 1 && lightFlareEvent_swellsCounter == 1 )
                {
                    busyFlare = false;
                    aud_lightFlareEvent_end = true;
                    lightComponent.enabled = false;
                }
                else if( lightFlareEvent_1or2swells == 2 )
                {
                    if( lightFlareEvent_swellsCounter == 1 )
                    {
                        lightFlareEvent_swellsCounter++;
                        StartLightFlareSwell_BeginCeiling();
                    }
                    else
                    {
                        busyFlare = false;
                        aud_lightFlareEvent_end = true;
                        lightComponent.enabled = false;
                    }
                }
            }
        }
    }

    void FlareHandling_LowerAndRougher()
    {
        // for LowerAndRougher, event beginnings and proceedings are all handled by a GrownAndFade in this method
        // ( event beginnings are not determined by geyser collisions ) 
        // growAndFade values are for ampLocal; scale light intensity from that - whereas above its the opposite

        // growAndFade must run constantly
        aud_ampLocal = lowerAndRougher_growAndFade.ReturnCurrentValue( flare_lowerAndRougher_growAndFade_initStayPut, false, mx_flare_lowerAndRougher_growAndFade_targetGrowRange, mx_flare_lowerAndRougher_growAndFade_targetFadeRange, mx_flare_lowerAndRougher_growAndFade_durToGrowRange, mx_flare_lowerAndRougher_growAndFade_durToFadeRange, mx_flare_lowerAndRougher_growAndFade_durToStayGrownRange, mx_flare_lowerAndRougher_growAndFade_durToStayFadedRange );

        // DESIGN prevent CPU overload...
        // only start flare if numBusyFlares is below the max num allowed
        if( lowerAndRougher_growAndFade._fadedAndStayingPutClick == true )
        {
            if( lightDiscs_numBusyFlare_script.lightDiscs_numBusyFlare < mx_flare_lowerAndRougher_maxNumFlares )
            {
                StartLightFlareSwell_LowerAndRougher();
                busyFlare = true;
            }
            else
            {
                // mute poly
                aud_lightFlareEvent_end = true;
                busyFlare = false;
            }
        }

        if( busyFlare == true )
        {
            lightComponent.intensity = Scale( aud_ampLocal, 0f, 1f, lightFlareEvent_intensityRange_lowerAndRougher[0], lightFlareEvent_intensityRange_lowerAndRougher[1] );
        }
    }

    void BusySpin()
    {
        if( aud_lightSpinEvent_start == true )
        {
            lightSpinEvent_spinDelta = .1f;
            lightSpin_frameCounter = lightSpin_frameStepSize;
        }

        if( lightSpin_frameCounter == lightSpin_frameStepSize )
        {
            if( aud_lightSpinEvent_start == false )
            {
                lightSpinEvent_spinDelta = Mathf.Abs( this.transform.rotation.y - transform_rotationPrev.y );
            }

            aud_ampLocal = Scale( lightSpinEvent_spinDelta, 0f, .5f, 0f, 1f );
            lightSpin_frameCounter = 0;
            transform_rotationPrev = this.transform.rotation;

            if( lightSpinEvent_spinDelta > lightSpinEvent_spinDeltaStopThresh )
            {
                lightComponent.intensity = Scale( lightSpinEvent_spinDelta, 0, .5f, lightSpinEvent_intensityRange[0], lightSpinEvent_intensityRange[1] );
            }
            else
            {
                this.transform.rotation = transform_rotationPrev;
                busySpin = false;
                aud_lightSpinEvent_end = true;
                lightComponent.enabled = false;
            }

            Beatings_SpinEvent();
        }
        else
        {
            lightSpin_frameCounter++;
        }
    }

    void SelectDiscModel_BeginCeiling()
    {
        discsModel_indexRandom = Random.Range( 0, makeDiscsModel_script.discsModel.Count );
        discModel_selected = makeDiscsModel_script.discsModel[ discsModel_indexRandom ];
        modelFreqs                    = discModel_selected.freqs;
        modelAmps                     = discModel_selected.amps;
        modelDecays                   = discModel_selected.decays;
        // for these states, ampGlobal set in the EvolutionParams()
    }

    void SelectDiscModel_LowerAndRougher()
    {
        discsModel_indexRandom = Random.Range( 0, makeDiscsModel_script.discsModel.Count );
        discModel_selected = makeDiscsModel_script.discsModel[discsModel_indexRandom];
        modelFreqs                    = discModel_selected.freqs;
        modelAmps                     = discModel_selected.amps;
        modelDecays                   = discModel_selected.decays;
            // now use the transp and ampGlobal from the ScoreDiscs
        modelTransposition            = discModel_selected.modelTransposition;
        aud_lightFlareEvent_ampGlobal = discModel_selected.ampGlobal * mx_lightFlareEvent_ampGlobal_scaleValuesInScore_lowerAndRougher;
        aud_lightSpinEvent_resonance_ampGlobal = discModel_selected.ampGlobal * mx_lightSpinEvent_ampGlobal_scaleValuesInScore_lowerAndRougher;
    }

    void Beatings_FlareEvent()
    {
        // we'll add extra tones to the model list, to create beatings / interest / shimmer around each fundamental and partial
        // **** we only generate the extra frequencies once, on event On - this isn't an evolving list of F, A, D ****

        lightFlareEvent_freqs = new List<float>( modelFreqs ); // <-- for troubleshooting
        InstantiateOscAddrs_FlareEvent();
        discModelParams = makeDiscsModel_script.discModelParams;

        sc_lightFlareEvent_shimmer_percentOfCriticalBandRange = discModelParams.lightFlareEvent_shimmer_percentOfCriticalBandRange;

        foreach( float freq in modelFreqs )   { osc_freqsFlare.Add( freq ); }
        foreach( float amp in modelAmps )     { osc_ampsFlare.Add( amp ); }
        foreach( float decay in modelDecays ) { osc_decaysFlare.Add( decay ); }

        for( int f = 0; f < modelFreqs.Count; f++ )
        {
            criticalBand = 24.7f * ( ( 0.00437f * modelFreqs[ f ] ) + 1 );
            half_criticalBand = criticalBand / 2;

            // fundamental - add three extra tones
            if( f == 0 )
            {
                for( int e = 0; e < 3; e++ )
                {
                    percentOfCriticalBand = Random.Range( sc_lightFlareEvent_shimmer_percentOfCriticalBandRange[0], sc_lightFlareEvent_shimmer_percentOfCriticalBandRange[1] );
                    percentOfCriticalBandFreq = modelFreqs[ f ] + ( percentOfCriticalBand * half_criticalBand );

                    osc_freqsFlare.Add( percentOfCriticalBandFreq );
                    osc_ampsFlare.Add( RandomAmp( "louder", aud_lightEventType ) );
                    osc_decaysFlare.Add( RandomDecay( "slower" ) );

                    // for debug:
                    lightFlareEvent_freqs.Add( percentOfCriticalBandFreq );
                    // lightFlareEvent_amps.Add( RandomAmp( "louder" ) );
                    // lightFlareEvent_decays.Add( RandomDecay( "slower" ) );
                }
            }
            // upper partials - add two extra tones each
            else
            {
                for( int e = 0; e < 2; e++ )
                {
                    percentOfCriticalBand = Random.Range( sc_lightFlareEvent_shimmer_percentOfCriticalBandRange[0], sc_lightFlareEvent_shimmer_percentOfCriticalBandRange[1] );
                    percentOfCriticalBandFreq = modelFreqs[ f ] + ( percentOfCriticalBand * half_criticalBand );

                    osc_freqsFlare.Add( percentOfCriticalBandFreq );
                    osc_ampsFlare.Add( RandomAmp( "softer", aud_lightEventType ) );
                    osc_decaysFlare.Add( RandomDecay( "faster" ) );

                    lightFlareEvent_freqs.Add(percentOfCriticalBandFreq);
                }
            }
        }
    }

    void Beatings_SpinEvent()
    {
        // we'll add extra tones to the model list, to create beatings / spin sound
        // **** we generate the extra frequencies constantly, scaled to the rate of spin so it will sound like its spinning ****
        // **** an evolving list of F, A, D ****

        lightSpinEvent_freqs = new List<float>( modelFreqs );
        // lightSpinEvent_amps = new List<float>( modelAmps );
        // lightSpinEvent_decays = new List<float>( modelDecays );
        InstantiateOscAddrs_SpinEvent();
        foreach( float freq in modelFreqs )   { osc_freqsSpin.Add( freq ); }
        foreach( float amp in modelAmps )     { osc_ampsSpin.Add( amp ); }
        foreach( float decay in modelDecays ) { osc_decaysSpin.Add( decay ); }

        for( int f = 0; f < modelFreqs.Count; f++ )
        {
            // fundamental
            if( f == 0 )
            {
                criticalBand = 24.7f * ( ( 0.00437f * modelFreqs[ f ] ) + 1 );
                half_criticalBand = criticalBand / 2;
                float lightSpinEvent_spinDeltaClamped = Mathf.Clamp( lightSpinEvent_spinDelta, 0f, .1f );
                percentOfCriticalBand = Scale( lightSpinEvent_spinDeltaClamped, 0.0f, .1f, mx_lightSpinEvent_fund_percentOfCriticalBandRange[0], mx_lightSpinEvent_fund_percentOfCriticalBandRange[1] );
                percentOfCriticalBandFreq = modelFreqs[ f ] + ( percentOfCriticalBand * half_criticalBand );

                osc_freqsSpin.Add( percentOfCriticalBandFreq );
                osc_ampsSpin.Add( RandomAmp( "louder", aud_lightEventType ) );
                osc_decaysSpin.Add( RandomDecay( "slower" ) );

                // for debug:
                lightSpinEvent_freqs.Add( percentOfCriticalBandFreq );
                // lightSpinEvent_amps.Add( RandomAmp( "louder" ) );
                // lightSpinEvent_decays.Add( RandomDecay( "slower" ) );
            }

            /*
            // upper partials
            // too noisy - for now
            else
            {
                criticalBand = 24.7f * ((0.00437f * modelFreqs[f]) + 1);
                half_criticalBand = criticalBand / 2;
                float lightSpinEvent_spinDeltaClamped = Mathf.Clamp(lightSpinEvent_spinDelta, 0f, .1f);
                percentOfCriticalBand = Scale(lightSpinEvent_spinDeltaClamped, 0.0f, .1f, mx_lightSpinEvent_fund_percentOfCriticalBandRange[0], mx_lightSpinEvent_fund_percentOfCriticalBandRange[1]);
                percentOfCriticalBandFreq = modelFreqs[f] + (percentOfCriticalBand * half_criticalBand);

                osc_freqsSpin.Add(percentOfCriticalBandFreq);
                osc_ampsSpin.Add(RandomAmp("softer", aud_lightEventType));
                osc_decaysSpin.Add(RandomDecay("faster"));

                lightSpinEvent_freqs.Add(percentOfCriticalBandFreq);
            }*/
        }

        /*
        if (selectedGO != null)
        {
            if (selectedGO.name == this.transform.name)
            { 
                if( osc_freqsSpin != null )
                {
                    Debug.Log("not null");
                }
            }
        }*/
    }

    float RandomAmp( string softer_or_louder, string lightEventType )
    {
        mx_flare_randAmp_softerRange = new Vector2( .001f, .01f );
        mx_flare_randAmp_louderRange = new Vector2( .6f, 1.0f );
        float randomAmp = 0;
        if( softer_or_louder == "softer" )
        {
            if( lightEventType == "flare" )
            {
                randomAmp = Random.Range( mx_flare_randAmp_softerRange[0], mx_flare_randAmp_softerRange[1] );
            }
            else if( lightEventType == "spin" )
            {
                randomAmp = Random.Range( mx_spin_randAmp_softerRange[0], mx_spin_randAmp_softerRange[1] );
            }
        }
        else if( softer_or_louder == "louder" )
        {
            if( lightEventType == "flare" )
            {
                randomAmp = Random.Range( mx_flare_randAmp_louderRange[0], mx_flare_randAmp_louderRange[1] );
            }
            else if( lightEventType == "spin" )
            {
                randomAmp = Random.Range( mx_spin_randAmp_louderRange[0], mx_spin_randAmp_louderRange[1] );
            }
        }

        return randomAmp;
    }

    float RandomDecay( string faster_or_slower )
    {
        mx_randDecay_fasterRange = new Vector2( 2.0f, 6.0f );
        mx_randDecay_slowerRange = new Vector2( .2f, 1.3f );
        float randomDecay = 0;
        if( faster_or_slower == "faster" )
        {
            randomDecay = Random.Range( mx_randDecay_fasterRange[0], mx_randDecay_fasterRange[1] );
        }
        else if( faster_or_slower == "slower" )
        {
            randomDecay = Random.Range( mx_randDecay_slowerRange[0], mx_randDecay_slowerRange[1] );
        }

        return randomDecay;
    }

    void DetermineSpinAttack()
    {
        // float normSpeed = Scale( geyserSpeed, geyser_startSpeedRange[0], geyser_startSpeedRange[1], 0f, 1f );
        float randomValue = Random.Range( 0f, 1f );
        if( randomValue <= mx_lightSpinEvent_attack_prob )
        {
            aud_attack = true;
        }
        else
        {
            aud_attack = false;
        }
    }

    // ><>  ><>  ><>  ><>
    void EvolutionParams()
    {
        if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.begin )
        {
            del_geyserCollision = GeyserCollision_BeginCeiling;
            del_flareHandling = FlareHandling_BeginCeiling;
            lightSpinEvent_prob = mx_lightSpinEvent_prob_begin;
            lightFlareEvent_durationRangePerSwell = mx_lightFlareEvent_durationRangePerSwell_begin;
            aud_lightFlareEvent_ampGlobal = mx_lightFlareEvent_ampGlobal_begin;
            lightFlareEvent_transposeDownAnOctave = false;
        }
        else if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.beginCeiling )
        {
            del_geyserCollision = GeyserCollision_BeginCeiling;
            del_flareHandling = FlareHandling_BeginCeiling;
            lightFlareEvent_transposeDownAnOctave = Random.Range( 0, 2 ) == 1;
        }
        else if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.tallerTubes )
        {
            del_geyserCollision = GeyserCollision_BeginCeiling;
            del_flareHandling = FlareHandling_BeginCeiling;
            lightFlareEvent_transposeDownAnOctave = Random.Range( 0, 2 ) == 1;
        }
        else if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.beginSpin )
        {
            del_geyserCollision = GeyserCollision_BeginCeiling;
            del_flareHandling = FlareHandling_BeginCeiling;
            lightSpinEvent_prob = mx_lightSpinEvent_prob_beginSpin;
            aud_lightFlareEvent_ampGlobal = mx_lightFlareEvent_ampGlobal_beginSpin;
            aud_lightSpinEvent_resonance_ampGlobal = mx_lightSpinEvent_resonance_ampGlobal;
            lightFlareEvent_transposeDownAnOctave = Random.Range( 0, 2 ) == 1;
        }
        else if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.lowerAndRougher )
        {
            if( globalEvolutionStatePrev != GlobalEvolution.GlobalEvolutionState.lowerAndRougher )
            {
                // now we need a GrowAndFade() to govern the flareEvents instead of the geyser collisions
                lowerAndRougher_growAndFade = new GrowAndFade();
                flare_lowerAndRougher_growAndFade_initStayPut = Random.Range( 0f, 1f ) >= mx_flare_lowerAndRougher_growAndFade_probInitStayPut;
                lightComponent.enabled = true;
            }
            del_geyserCollision = GeyserCollision_LowerAndRougher;
            del_flareHandling = FlareHandling_LowerAndRougher;
            // transposition and ampGlobal now come from ScoreDiscs and are set inside the SelectDiscModel delegate for this state
        }

        if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.lowerAndRougher )
        {
            del_selectDiscModel = SelectDiscModel_LowerAndRougher;
        }
        else
        {
            del_selectDiscModel = SelectDiscModel_BeginCeiling;
        }
    }

    float Scale( float oldValue, float oldMin, float oldMax, float newMin, float newMax )
    {

        float oldRange = oldMax - oldMin;
        float newRange = newMax - newMin;
        float newValue = (((oldValue - oldMin) * newRange) / oldRange) + newMin;

        return newValue;
    }

    void InstantiateOscAddrs_FlareEvent()
    {
        osc_freqsFlare =  new OscMessage( "/disc/" + ( self_id + 1 ) + "/model/freqs" );
        osc_ampsFlare =   new OscMessage( "/disc/" + ( self_id + 1 ) + "/model/amps" );
        osc_decaysFlare = new OscMessage( "/disc/" + ( self_id + 1 ) + "/model/decays" );
    }

    void InstantiateOscAddrs_SpinEvent()
    {
        osc_freqsSpin =  new OscMessage( "/disc/" + ( self_id + 1 ) + "/model/freqs" );
        osc_ampsSpin =   new OscMessage( "/disc/" + ( self_id + 1 ) + "/model/amps" );
        osc_decaysSpin = new OscMessage( "/disc/" + ( self_id + 1 ) + "/model/decays" );
    }

    void ReportOsc()
    {
        if( busyFlare == true )
        {
            oscOutScript.Send( "/disc/" + ( self_id + 1 ) + "/ampLocal",            aud_ampLocal );
            oscOutScript.Send( "/disc/" + ( self_id + 1 ) + "/lightEventType",      aud_lightEventType );
        }
        else if( busySpin == true )
        {
            if( osc_freqsSpin != null )
            {
                oscOutScript.Send(                                                  osc_freqsSpin );
                oscOutScript.Send(                                                  osc_ampsSpin );
                oscOutScript.Send(                                                  osc_decaysSpin );
                oscOutScript.Send( "/disc/" + ( self_id + 1 ) + "/ampLocal",        aud_ampLocal );
                oscOutScript.Send( "/disc/" + ( self_id + 1 ) + "/lightEventType",  aud_lightEventType );
            }
        }

        // noteOn event to be accompanied by the first model values?
        if( aud_lightFlareEvent_start == true )
        {
            oscOutScript.Send(                                                      osc_freqsFlare );
            oscOutScript.Send(                                                      osc_ampsFlare );
            oscOutScript.Send(                                                      osc_decaysFlare );
            oscOutScript.Send( "/disc/" + ( self_id + 1 ) + "/noteOn",              aud_lightFlareEvent_start );
            oscOutScript.Send( "/disc/" + ( self_id + 1 ) + "/ampGlobal/resonance", aud_lightFlareEvent_ampGlobal );
            oscOutScript.Send( "/disc/" + ( self_id + 1 ) + "/attack",              0 );
            aud_lightFlareEvent_start = false;
        }
        else if( aud_lightFlareEvent_end == true )
        {
            oscOutScript.Send( "/disc/" + ( self_id + 1 ) + "/noteOff",             aud_lightFlareEvent_end );
            aud_lightFlareEvent_end = false;
        }
        else if( aud_lightSpinEvent_start == true )
        {
            oscOutScript.Send( "/disc/" + ( self_id + 1 ) + "/noteOn",              aud_lightSpinEvent_start );
            oscOutScript.Send( "/disc/" + ( self_id + 1 ) + "/ampGlobal/resonance", aud_lightSpinEvent_resonance_ampGlobal );
            oscOutScript.Send( "/disc/" + ( self_id + 1 ) + "/ampGlobal/attack",    mx_lightSpinEvent_attack_ampGlobal );
            oscOutScript.Send( "/disc/" + ( self_id + 1 ) + "/attack",              aud_attack );
            aud_lightSpinEvent_start = false;
        }
        else if( aud_lightSpinEvent_end == true )
        {
            oscOutScript.Send( "/disc/" + ( self_id + 1 ) + "/noteOff",             aud_lightSpinEvent_end );
            aud_lightSpinEvent_end = false;
        }
    }

    void MixerValues_Init()
    {
        mx_lightFlareEvent_durationRangePerSwell_begin                  = mixer.disc_lightFlareEvent_durationRangePerSwell_begin;
        mx_lightSpinEvent_prob_begin                                    = mixer.disc_lightSpinEvent_prob_begin;
        mx_lightSpinEvent_prob_beginSpin                                = mixer.disc_lightSpinEvent_prob_beginSpin;
        mx_lightFlareEvent_ampGlobal_begin                              = mixer.disc_lightFlareEvent_ampGlobal_begin;
        mx_lightFlareEvent_ampGlobal_beginSpin                          = mixer.disc_lightFlareEvent_ampGlobal_beginSpin;
        mx_lightFlareEvent_ampGlobal_scaleValuesInScore_lowerAndRougher = mixer.disc_lightFlareEvent_ampGlobal_scaleValuesInScore_lowerAndRougher;
        mx_lightSpinEvent_ampGlobal_scaleValuesInScore_lowerAndRougher  = mixer.disc_lightSpinEvent_ampGlobal_scaleValuesInScore_lowerAndRougher;
        mx_lightSpinEvent_resonance_ampGlobal                           = mixer.disc_lightSpinEvent_resonance_ampGlobal;
        mx_lightSpinEvent_attack_ampGlobal                              = mixer.disc_lightSpinEvent_attack_ampGlobal;
        mx_lightSpinEvent_attack_prob                                   = mixer.disc_lightSpinEvent_attack_prob;
        mx_lightSpinEvent_fund_percentOfCriticalBandRange               = mixer.disc_lightSpinEvent_fund_percentOfCriticalBandRange;
        mx_flare_randAmp_softerRange                                    = mixer.disc_flare_randAmp_softerRange;
        mx_flare_randAmp_louderRange                                    = mixer.disc_flare_randAmp_louderRange;
        mx_spin_randAmp_softerRange                                     = mixer.disc_spin_randAmp_softerRange;
        mx_spin_randAmp_louderRange                                     = mixer.disc_spin_randAmp_louderRange;
        mx_randDecay_fasterRange                                        = mixer.disc_randDecay_fasterRange;
        mx_randDecay_slowerRange                                        = mixer.disc_randDecay_slowerRange;
        mx_flare_lowerAndRougher_maxNumFlares                           = mixer.disc_flare_lowerAndRougher_maxNumFlares;
        mx_flare_lowerAndRougher_growAndFade_probInitStayPut            = mixer.disc_flare_lowerAndRougher_growAndFade_probInitStayPut;
        mx_flare_lowerAndRougher_growAndFade_targetFadeRange            = mixer.disc_flare_lowerAndRougher_growAndFade_targetFadeRange;
        mx_flare_lowerAndRougher_growAndFade_targetGrowRange            = mixer.disc_flare_lowerAndRougher_growAndFade_targetGrowRange;
        mx_flare_lowerAndRougher_growAndFade_durToFadeRange             = mixer.disc_flare_lowerAndRougher_growAndFade_durToFadeRange;
        mx_flare_lowerAndRougher_growAndFade_durToGrowRange             = mixer.disc_flare_lowerAndRougher_growAndFade_durToGrowRange;
        mx_flare_lowerAndRougher_growAndFade_durToStayFadedRange        = mixer.disc_flare_lowerAndRougher_growAndFade_durToStayFadedRange;
        mx_flare_lowerAndRougher_growAndFade_durToStayGrownRange        = mixer.disc_flare_lowerAndRougher_growAndFade_durToStayGrownRange;
    }


    void OnDrawGizmos()
    {
        if(globalEvolutionState == GlobalEvolution.GlobalEvolutionState.lowerAndRougher)
        {
            Gizmos.color = Color.yellow;
            if (lowerAndRougher_growAndFade._fadedAndStayingPutClick == true)
            {
                Gizmos.DrawSphere(this.transform.position, .4f);
            }
            Gizmos.color = Color.red;
            if (busyFlare == true)
            {
                Gizmos.DrawSphere(this.transform.position, .2f);
            }
        }

        /*
        if (selectedGO != null)
        {
            if (selectedGO.name == this.transform.name)
            {
                if(busy == false)
                {
                    Gizmos.color = Color.green;
                }
                else
                {
                    Gizmos.color = Color.magenta;
                }
                Gizmos.DrawSphere(this.transform.position, .2f);

                if(lightSpinEvent_spinDelta <= lightSpinEvent_spinDeltaStopThresh)
                {
                    Gizmos.color = Color.cyan;
                }
                else
                {
                    Gizmos.color = Color.red;
                }
                Gizmos.DrawSphere(new Vector3(this.transform.position.x, this.transform.position.y - 1.0f, this.transform.position.z ), .3f);
            }
        }*/
    }
}
