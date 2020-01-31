using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour 
{
    Material skybox;

    // osc
    OscOut oscOut_script;

    // global evolution
    GlobalEvolution globalEvolution_script;
    GlobalEvolution.GlobalEvolutionState globalEvolutionState;

    // mixer
    Mixer mixer;

    // mixer values
    Vector2 mx_softerEvents_exposureRange_grow;
    Vector2 mx_softerEvents_exposureRange_fade;
    Vector2 mx_change_durationRange_grow;
    Vector2 mx_change_durationRange_fade;
    Vector2 mx_stayPut_durationRange_grown;
    Vector2 mx_stayPut_durationRange_faded;
    float   mx_tubes_softerEvents_ampGlobal;
    float   mx_noise_softerEvents_ampGlobal;
    float   mx_louderEventProb_begin;
    float   mx_louderEventProb_beginCeiling;
    Vector2 mx_louderEvents_ampGlobalRange_begin;
    Vector2 mx_louderEvents_ampGlobalRange_beginSpin;

    // other audio values to max
    float aud_ampLocal;
    float aud_tubes_ampGlobal;
    float aud_noise_ampGlobal;
    bool  aud_change_growBegin;

    float exposure;
    bool  stayPut;     // else change
    bool  fade;        // else grow
    float change_exposureInitial;
    float change_exposureTarget;
    float change_duration;
    float louderEventProb;
    bool  louderEvent; // else softerEvent
    Vector2 louderEvents_ampGlobalRange;

    float stayPut_duration;

    float lerp_startTime;
    float lerp_deltaTime;
    float lerp_phase;

    void Start() 
    {
        skybox = RenderSettings.skybox;

        oscOut_script = GameObject.Find( "osc" ).GetComponent<OscOut>();
        globalEvolution_script = GameObject.Find( "globalEvolution" ).GetComponent<GlobalEvolution>();

        mixer = new Mixer();
        MixerValues_Init();
        skybox.SetFloat( "_Exposure", 0 );
        fade = true;

        New_SofterFade();
        New_StayPut();
        ReportOscStart();
    }
	
	void Update() 
    {
        globalEvolutionState = globalEvolution_script.globalEvolutionState;

        EvolutionParams();

        lerp_deltaTime = Time.time - lerp_startTime;

        if( stayPut == true )
        {
            lerp_phase = lerp_deltaTime / stayPut_duration;
            if( lerp_phase > 1f )
            {
                New_Change();
            }
        }
        else
        {
            lerp_phase = lerp_deltaTime / change_duration;
            if( lerp_phase > 1 )
            {
                New_StayPut();
            }
            else
            {
                exposure = Mathf.Lerp( change_exposureInitial, change_exposureTarget, lerp_phase );
                skybox.SetFloat( "_Exposure", exposure );
                aud_ampLocal = Scale( exposure, 0f, 8f, 0f, 1f );
            }
        }

        ReportOscUpdate();
	}

    //*********************
    // change or stay put
    void New_Change()
    {
        stayPut = false;
        fade = !fade;
        change_exposureInitial = skybox.GetFloat( "_Exposure" );

        if( fade == false )
        {
            change_duration = Random.Range( mx_change_durationRange_grow[0], mx_change_durationRange_grow[1] );

            if( louderEvent == true )
            {
                New_LouderGrow();
            }
            else
            {
                New_SofterGrow();
            }

            aud_change_growBegin = true;
        }
        else
        {
            change_duration = Random.Range( mx_change_durationRange_fade[0], mx_change_durationRange_fade[1] );

            // *******
            // determine if louderEvent HERE, so on the fade it is guaranteed to go to 0., then gets louder
            // *******
            float rand = Random.Range( 0f, 1f );
            if( rand <= louderEventProb )
            {
                louderEvent = true;
                New_LouderFade();
            }
            else
            {
                louderEvent = false;
                New_SofterFade();
            }
        }

        lerp_startTime = Time.time;
    }

    void New_StayPut()
    {
        stayPut = true;
        if( fade == true )
        {
            stayPut_duration = Random.Range( mx_stayPut_durationRange_faded[0], mx_stayPut_durationRange_faded[1] );
        }
        else
        {
            stayPut_duration = Random.Range( mx_stayPut_durationRange_grown[0], mx_stayPut_durationRange_grown[1] );
        }

        lerp_startTime = Time.time;
    }

    //*********************
    // specifics of softerEvent or louderEvent
    void New_SofterGrow()
    {
        aud_tubes_ampGlobal = mx_tubes_softerEvents_ampGlobal;
        aud_noise_ampGlobal = mx_noise_softerEvents_ampGlobal;
        change_exposureTarget = Random.Range( mx_softerEvents_exposureRange_grow[0], mx_softerEvents_exposureRange_grow[1] );
    }

    void New_SofterFade()
    {
        aud_tubes_ampGlobal = mx_tubes_softerEvents_ampGlobal;
        aud_noise_ampGlobal = mx_noise_softerEvents_ampGlobal;
        change_exposureTarget = Random.Range( mx_softerEvents_exposureRange_fade[0], mx_softerEvents_exposureRange_fade[1] );
    }

    void New_LouderGrow()
    {
        float ampGlobal = Random.Range( louderEvents_ampGlobalRange[0], louderEvents_ampGlobalRange[1] );
        aud_tubes_ampGlobal = ampGlobal;
        aud_noise_ampGlobal = ampGlobal;
        change_exposureTarget = 8; // <---always go to max ampLocal - ampGlobal responsible for loudness of grow
        // Debug.Log( "louder tubeAmp " + aud_tubes_ampGlobal + " noiseAmp " + aud_noise_ampGlobal );
    }

    void New_LouderFade()
    {
        // ampGlobal only gets set in LouderGrow
        change_exposureTarget = 0; //<--- guarantees to go to ampLocal 0.
    }

    // osc
    void ReportOscStart()
    {
        oscOut_script.Send( "/background/tubes/amp/global",     aud_tubes_ampGlobal );
        oscOut_script.Send( "/background/noise/amp/global",     aud_noise_ampGlobal );
    }

    void ReportOscUpdate()
    {
        oscOut_script.Send( "/background/amp/local",            aud_ampLocal );

        if( aud_change_growBegin == true )
        {
            oscOut_script.Send( "/background/growBegin",        aud_change_growBegin );
            oscOut_script.Send( "/background/tubes/amp/global", aud_tubes_ampGlobal );
            oscOut_script.Send( "/background/noise/amp/global", aud_noise_ampGlobal );
            aud_change_growBegin = false;
        }
    }

    // mixer init
    void MixerValues_Init()
    {
        mx_softerEvents_exposureRange_grow       = mixer.bg_softerEvents_exposureRange_grow;
        mx_softerEvents_exposureRange_fade       = mixer.bg_softerEvents_exposureRange_fade;
        mx_change_durationRange_grow             = mixer.bg_change_durationRange_grow;
        mx_change_durationRange_fade             = mixer.bg_change_durationRange_fade;
        mx_stayPut_durationRange_grown           = mixer.bg_stayPut_durationRange_grown;
        mx_stayPut_durationRange_faded           = mixer.bg_stayPut_durationRange_faded;
        mx_tubes_softerEvents_ampGlobal          = mixer.bg_tubes_softerEvents_ampGlobal;
        mx_noise_softerEvents_ampGlobal          = mixer.bg_noise_softerEvents_ampGlobal;
        mx_louderEventProb_begin                 = mixer.bg_louderEventProb_begin;
        mx_louderEventProb_beginCeiling          = mixer.bg_louderEventProb_beginCeiling;
        mx_louderEvents_ampGlobalRange_begin     = mixer.bg_louderEvents_ampGlobalRange_begin;
        mx_louderEvents_ampGlobalRange_beginSpin = mixer.bg_louderEvents_ampGlobalRange_beginSpin;
    }

    //****************************************
    // ><>  ><>  ><>  ><>
    void EvolutionParams()
    {
        if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.begin )
        {
            louderEventProb = mx_louderEventProb_begin;
            louderEvents_ampGlobalRange = mx_louderEvents_ampGlobalRange_begin;
        }
        else if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.beginCeiling )
        {
            louderEventProb = mx_louderEventProb_beginCeiling;
        }
        else if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.beginSpin )
        {
            louderEvents_ampGlobalRange = mx_louderEvents_ampGlobalRange_beginSpin;
        }
    }

    // etc
    float Scale(float oldValue, float oldMin, float oldMax, float newMin, float newMax)
    {

        float oldRange = oldMax - oldMin;
        float newRange = newMax - newMin;
        float newValue = (((oldValue - oldMin) * newRange) / oldRange) + newMin;

        return newValue;
    }
}
