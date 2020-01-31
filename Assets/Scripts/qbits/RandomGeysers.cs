using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomGeysers : MonoBehaviour 
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

    VoidMesh voidMeshScript;
    VoidsAllInfo voidsAllInfo_script;

    // global evolution
    GlobalEvolution globalEvolution_script;
    GlobalEvolution.GlobalEvolutionState globalEvolutionState;

    // OSC
    OscOut oscOutScript;

    // mixer
    Mixer mixer;

    // ****
    // mixer values
    float   mx_ampGlobal;
    Vector2 mx_geyserDuration_toAddMoreParticles_range;
        // see mixer for explantation of the following 3 params that work in conjunction
    Vector2 mx_durationBetweenGeysers_range_1void;
    float   mx_durationBetweenGeysers_secAddedToMin_perAdditionalVoid;
    float   mx_durationBetweenGeysers_secAddedToMax_perAdditionalVoid;
    Vector2 mx_particlesRateRange;
    Vector2 mx_lfoDynamicsRange;
    Vector2 mx_stopB4Ceiling_durationUntilStopRange;
    float   mx_stopB4CeilingProb_begin;
    float   mx_stopB4CeilingProb_beginCeiling;
    Vector2 mx_startSpeedRange_begin;
    Vector2 mx_startSpeedRange_beginCeiling;

    // ****
    // additional values to audio/Max
    int   aud_currentGeyser_delayMode;
    float aud_currentGeyser_lfoDynamics;
    float aud_geyserFading_globalAmp;
    bool  aud_geyserOnEvent;
    bool  aud_geyserOffEvent;

    float self_id;

    ParticleSystem particleSystem1;
    ParticleSystem particleSystem2;
    ParticleSystem.ShapeModule shapeModule;
    ParticleSystem.MainModule mainModule;
    ParticleSystem.EmissionModule emissionModule;
    ParticleSystemRenderer rendererModule;
    Vector3 voidCentroid;

    bool aGeyserIsActive;

    int numParticles;
    int numParticlesPrev;
    bool geyserFading_begin;
    bool geyserFading;
    // for fading before reaching ceiling:
    float geyserFading_startTime;
    float geyserFading_deltaTime;
    // for fading after reaching ceiling: 
    int geyserFading_numParticlesAtBegin;
    // for both:
    float geyserFading_phase;

    bool  firstGeyserEvent = true;
    bool  firstGeyserEvent_recordStartTime = true;
    float firstGeyserEvent_postponeDuration;
    float firstGeyserEvent_postponeStartTime;

    public Vector2 startSpeedRange = new Vector2( 1.0f, 2.0f );
    float currentGeyserDuration_toAddMoreParticles;
    float currentGeyserDuration_untilStop;
    bool  currentGeyser_stopB4Ceiling;
    float currentGeyser_particlesRate;
    float currentGeyser_startSpeed;
    float currentGeyser_startTime;
    float currentGeyser_deltaTime;
    float durationBetweenGeysers;
    int   voidsNumOpen;

    void Start()
    {
        voidMeshScript = GetComponent<VoidMesh>();
        particleSystem1 = GetComponent<ParticleSystem>();
        shapeModule = particleSystem1.shape;
        mainModule  = particleSystem1.main;
        emissionModule = particleSystem1.emission;
        rendererModule = GetComponent<ParticleSystemRenderer>();

        self_id = voidMeshScript.self_id;

        globalEvolution_script = GameObject.Find( "globalEvolution" ).GetComponent<GlobalEvolution>();
        voidsAllInfo_script = GameObject.Find( "voidsAllInfo" ).GetComponent<VoidsAllInfo>();

        oscOutScript = GameObject.Find( "osc" ).GetComponent<OscOut>();

        mixer = new Mixer();
        MixerValues_Init();
    }

    void Update() 
    {
        if( voidMeshScript.iAmOpen == true )
        {
            globalEvolutionState = globalEvolution_script.globalEvolutionState;

            voidsNumOpen = 0;
            foreach( Void_Cn voidEntry in voidsAllInfo_script.voidsAllInfo )
            {
                voidsNumOpen++;
            }

            shapeModule.position = voidMeshScript.self_voidAllInfo.centroid;
            numParticles = particleSystem1.particleCount;

            if( firstGeyserEvent == true )
            {
                if( firstGeyserEvent_recordStartTime == true )
                {
                    firstGeyserEvent_recordStartTime = false;
                    firstGeyserEvent_postponeStartTime = Time.time;
                    firstGeyserEvent_postponeDuration = Random.Range( 2.0f, 4.0f );
                }
                else
                {
                    if( Time.time - firstGeyserEvent_postponeStartTime >= firstGeyserEvent_postponeDuration )
                    {
                        NewGeyserEvent();
                        firstGeyserEvent = false;
                    }
                }
            }
            else 
            {
                // the following ways, we only report 1 value for each of geyser on and off
                if( currentGeyser_deltaTime >= durationBetweenGeysers )
                {
                    NewGeyserEvent();
                }

                currentGeyser_deltaTime = Time.time - currentGeyser_startTime;

                if( aGeyserIsActive == true )
                {
                    // stop before ceiling:
                    if( currentGeyser_stopB4Ceiling == true )
                    {
                        CheckFading_stopAfterDur();
                    }
                    // hits ceiling:
                    else
                    {
                        CheckFading_hitsCeiling();
                    }

                    ReportOsc();
                    if( aud_geyserOnEvent == true ) { aud_geyserOnEvent = false; }
                    if( aud_geyserOffEvent == true) { aud_geyserOffEvent = false; }
                }
            }

            numParticlesPrev = numParticles;
        }
    }

    void NewGeyserEvent()
    {
        // ><>  ><>  ><>  ><>
        EvolutionParams_NewGeyser();
        // ><>  ><>  ><>  ><>
        currentGeyser_startSpeed = Random.Range( startSpeedRange[0], startSpeedRange[1] );
        mainModule.startSpeed    = currentGeyser_startSpeed;

        if ( currentGeyser_stopB4Ceiling == true )
        {
            currentGeyserDuration_untilStop = Random.Range( mx_stopB4Ceiling_durationUntilStopRange[0], mx_stopB4Ceiling_durationUntilStopRange[1] );
        }

        aud_geyserOnEvent = true;
        aGeyserIsActive = true;
        currentGeyserDuration_toAddMoreParticles = Random.Range( mx_geyserDuration_toAddMoreParticles_range[0], mx_geyserDuration_toAddMoreParticles_range[1] );
        durationBetweenGeysers = Random.Range( mx_durationBetweenGeysers_range_1void[0] + ( ( voidsNumOpen - 1 ) * mx_durationBetweenGeysers_secAddedToMin_perAdditionalVoid ), mx_durationBetweenGeysers_range_1void[1] + ((voidsNumOpen - 1) * mx_durationBetweenGeysers_secAddedToMax_perAdditionalVoid ) );
        mainModule.duration = currentGeyserDuration_toAddMoreParticles;
        rendererModule.material.color = new Color( rendererModule.material.color.r, rendererModule.material.color.g, rendererModule.material.color.b, 1.0f );

        currentGeyser_startTime = Time.time;
        // shapeModule.rotation = new Vector3( 0, Random.Range( 0, 359 ), 0 );
        currentGeyser_particlesRate = Random.Range( mx_particlesRateRange[0], mx_particlesRateRange[1] );
        emissionModule.rateOverTime = currentGeyser_particlesRate;
        // todo evolve over time to be a wider angle to play more of the ceiling...
        shapeModule.angle = Random.Range( 4.6f, 25.0f );
        // shapeModule.angle = 3.6f;
        aud_currentGeyser_lfoDynamics = Scale( currentGeyser_particlesRate, mx_particlesRateRange[0], mx_particlesRateRange[1], mx_lfoDynamicsRange[0], mx_lfoDynamicsRange[1] );
        int randomValue = Random.Range( 0, 2 );
        // in max, we want 0 or 2 for the delay engines to use
        if( randomValue == 1 ){ randomValue = 2; }
        aud_currentGeyser_delayMode = randomValue;
        geyserFading_begin = true;

        particleSystem1.Play();
    }

    void CheckFading_stopAfterDur()
    {
        if( currentGeyserDuration_untilStop - currentGeyser_deltaTime <= 1.0 )
        {
            // needed for osc reporting condition:
            geyserFading = true;

            aud_geyserFading_globalAmp = currentGeyserDuration_untilStop - currentGeyser_deltaTime;
            rendererModule.material.color = new Color( rendererModule.material.color.r, rendererModule.material.color.g, rendererModule.material.color.b, aud_geyserFading_globalAmp );
            // Debug.Log("amp stopAfterDur " + geyserFading_globalAmp);
            // Debug.Log("durUntilStop " + currentGeyserDuration_untilStop + " deltaTime " + currentGeyserDeltaTime);
            // Debug.Log(currentGeyserDuration_untilStop - currentGeyserDeltaTime);

            if( currentGeyserDuration_untilStop - currentGeyser_deltaTime <= 0.0f )
            {
                aud_geyserOffEvent = true;
                aGeyserIsActive = false;
                geyserFading = false;
                geyserFading_begin = true;

                particleSystem1.Stop( true, ParticleSystemStopBehavior.StopEmittingAndClear );
                currentGeyser_stopB4Ceiling = false;
            }
        }
    }

    void CheckFading_hitsCeiling()
    {
        if( numParticles < numParticlesPrev )
        {
            // needed for osc reporting condition:
            geyserFading = true;

            if( geyserFading_begin == true )
            {
                geyserFading_numParticlesAtBegin = numParticles;
                geyserFading_begin = false;
            }

            geyserFading_phase = ( geyserFading_numParticlesAtBegin - (float)numParticles ) / geyserFading_numParticlesAtBegin;
            aud_geyserFading_globalAmp = Mathf.Lerp( 1.0f, 0.0f, geyserFading_phase );
            // Debug.Log("amp hitCeiling " + geyserFading_globalAmp);

            if( numParticles == 0 )
            {
                aud_geyserOffEvent = true;
                aGeyserIsActive = false;
                geyserFading = false;
                geyserFading_begin = true;
            }
        }
    }

    // ><>  ><>  ><>  ><>
    void EvolutionParams_NewGeyser()
    {
        if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.begin )
        {
            // 100% chance of stopB4ceiling geysers = combiination of killiung particles after dur, and limiting speed so doesn't reach ceiling before dying
            currentGeyser_stopB4Ceiling = ( Random.Range( 0f, 1f ) <= mx_stopB4CeilingProb_begin ); // <-- cool way to get a random true or false ( and its weighted )
            startSpeedRange = mx_startSpeedRange_begin;
        }
        else if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.beginCeiling )
        {
            // now we allow geysers that don't stop before ceiling - play lightDiscs
            currentGeyser_stopB4Ceiling = ( Random.Range( 0f, 1f ) <= mx_stopB4CeilingProb_beginCeiling );
            startSpeedRange = mx_startSpeedRange_beginCeiling;
        }
    }

    void ReportOsc()
    {
        // in the voids DSP, /id comes after the specific /void/polyname/id; this is because there are independent 
        // polys per independent void sound, and wanted to mute the processing on each of these independently
        // and that's how the routing works then
        if( aud_geyserOnEvent == true )
        {
            oscOutScript.Send( "/void/geyser/" + ( self_id + 1 ) + "/on",           aud_geyserOnEvent );
            // oscOutScript.Send( "/void/geyser/" + ( self_id + 1 ) + "/rate", particlesRate );
            oscOutScript.Send( "/void/geyser/" + ( self_id + 1 ) + "/lfo/dynamics", aud_currentGeyser_lfoDynamics );
            oscOutScript.Send( "/void/geyser/" + ( self_id + 1 ) + "/delay",        aud_currentGeyser_delayMode );
            oscOutScript.Send( "/void/geyser/" + ( self_id + 1 ) + "/amp/global",   mx_ampGlobal );
        }
        if( geyserFading == true )
        {
            oscOutScript.Send( "/void/geyser/" + ( self_id + 1 ) + "/amp/fading",   aud_geyserFading_globalAmp );
        }
        if( aud_geyserOffEvent == true )
        {
            oscOutScript.Send( "/void/geyser/" + ( self_id + 1 ) + "/off",          aud_geyserOffEvent );
        }
    }

    // mixer
    void MixerValues_Init()
    {
        mx_ampGlobal                                              = mixer.geyser_ampGlobal;
        mx_startSpeedRange_begin                                  = mixer.geyser_startSpeedRange_begin;
        mx_startSpeedRange_beginCeiling                           = mixer.geyser_startSpeedRange_beginCeiling;
        mx_geyserDuration_toAddMoreParticles_range                = mixer.geyser_geyserDuration_toAddMoreParticles_range;
        mx_durationBetweenGeysers_range_1void                     = mixer.geyser_durationBetweenGeysers_range_1void;
        mx_durationBetweenGeysers_secAddedToMin_perAdditionalVoid = mixer.geyser_durationBetweenGeysers_secAddedToMin_perAdditionalVoid;
        mx_durationBetweenGeysers_secAddedToMax_perAdditionalVoid = mixer.geyser_durationBetweenGeysers_secAddedToMax_perAdditionalVoid;
        mx_particlesRateRange                                     = mixer.geyser_particlesRateRange;
        mx_lfoDynamicsRange                                       = mixer.geyser_lfoDynamicsRange;
        mx_stopB4Ceiling_durationUntilStopRange                   = mixer.geyser_stopB4Ceiling_durationUntilStopRange;
        mx_stopB4CeilingProb_begin                                = mixer.geyser_stopB4CeilingProb_begin;
        mx_stopB4CeilingProb_beginCeiling                         = mixer.geyser_stopB4CeilingProb_beginCeiling;
    }

    //****************************************
    // etc
    public float Scale(float oldValue, float oldMin, float oldMax, float newMin, float newMax)
    {

        float oldRange = oldMax - oldMin;
        float newRange = newMax - newMin;
        float newValue = (((oldValue - oldMin) * newRange) / oldRange) + newMin;

        return newValue;
    }
}
