using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnOffTrackerWeighted 
{
    //********
    // -what-
    // schedules events on and off given provided - random duration ranges - longer and shorter durations for on and off;
    //                                            - probabilities for selecting longer and shorter durations ( 0f - 1f ) <--- note range
    // also provides fadeOut float ( 0 to 1 ) prior to off, given provided fadeOut duration

    // -from other scripts- 
    // in Start(), need to instantiate both an instance of the container class, OnOff_Cn, and this script's class, OnOffTrackerWeighted
    // Start()  : onOff_cn = new OnOff_Cn(); onOffTrackerWeighted = new OnOffTrackerWeighted();
    // Update() : onOff_cn = onOffTracker.Return_OnOff_Cn( ...params... ) <---returns the OnOff_Cn Type, which contains all the vars
    //********

    bool init = true;
    bool init_on;

    // inits as off because of init if() below
    // see OnOff_Cn for definition of variables 
    bool _on;
    bool _onClick;
    bool _offClick;
    // 0. - 1., 0. = begin fadeOut; 1. = end
    float _fadeOutPhase;
    // only for reporting:
    float _onDur;
    float _offDur;

    OnOff_Cn onOff_cn = new OnOff_Cn();

    float eventDur;
    float eventStartTime;
    float eventDeltaTime;
    float randValue;

    Vector2 offDurRange;
    Vector2 onDurRange;

    // return type OnOff_Cn
    public OnOff_Cn Return_OnOff_Cn( bool init_on, Vector2 onDurRange_shorter, Vector2 onDurRange_longer, float prob_onLonger, Vector2 offDurRange_shorter, Vector2 offDurRange_longer, float prob_offLonger, float fadeDur )
    {
        if( _onClick  == true ) { _onClick  = false; }
        if( _offClick == true ) { _offClick = false; }

        if( init == true )
        {
            New_OffDurRange( prob_offLonger, offDurRange_longer, offDurRange_shorter );
            New_Off( offDurRange );
            init = false;
        }

        eventDeltaTime = Time.time - eventStartTime;

        if( eventDeltaTime > eventDur )
        {
            if( _on == true )
            {
                New_OffDurRange( prob_offLonger, offDurRange_longer, offDurRange_shorter );
                New_Off( offDurRange );
            }
            else
            {
                New_OnDurRange( prob_onLonger, onDurRange_longer, onDurRange_shorter );
                New_On( onDurRange );
            }
        }

        if( eventDeltaTime >= eventDur - fadeDur && _on == true )
        {
            float timeRemaining = eventDur - eventDeltaTime;
            float timeRemainingProgress = fadeDur - timeRemaining;
            _fadeOutPhase = timeRemainingProgress / fadeDur;
        }
        else
        {
            _fadeOutPhase = 0f;
        }

        onOff_cn = new OnOff_Cn { on = _on, onClick = _onClick, offClick = _offClick, fadeOutPhase = _fadeOutPhase, onDur = _onDur, offDur = _offDur };

        return onOff_cn;
    }

    void New_OnDurRange( float prob_onLonger, Vector2 onDurRange_longer, Vector2 onDurRange_shorter )
    {
        randValue = Random.Range( 0f, 1f );
        if( randValue <= prob_onLonger )
        {
            onDurRange = onDurRange_longer;
        }
        else
        {
            onDurRange = onDurRange_shorter;
        }
    }

    void New_OffDurRange( float prob_offLonger, Vector2 offDurRange_longer, Vector2 offDurRange_shorter )
    {
        randValue = Random.Range( 0f, 1f );
        if( randValue <= prob_offLonger )
        {
            offDurRange = offDurRange_longer;
        }
        else
        {
            offDurRange = offDurRange_shorter;
        }
    }

    void New_On( Vector2 onDurRange )
    {
        _on = true;
        _onClick = true;
        eventStartTime = Time.time;
        eventDur = Random.Range( onDurRange[0], onDurRange[1] );
        _onDur = eventDur;
        // Debug.Log("tracker on " + _on + " dur " + eventDur);
    }

    void New_Off( Vector2 offDurRange )
    {
        _on = false;
        _offClick = true;
        eventStartTime = Time.time;
        eventDur = Random.Range( offDurRange[0], offDurRange[1] );
        _offDur = eventDur;
        // Debug.Log("tracker on " + _on + " dur " + eventDur);
    }
}
