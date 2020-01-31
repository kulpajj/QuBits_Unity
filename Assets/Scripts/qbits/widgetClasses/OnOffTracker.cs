using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnOffTracker
{
    //********
    // -what-
    // schedules events on and off given provided random duration ranges; 
    // also provides fadeOut float ( 0 to 1 ) prior to off, given provided fadeOut duration

    // -from other scripts- 
    // in Start(), need to instantiate both an instance of the container class, OnOff_Cn, and this script's class, OnOffTracker
    // Start()  : onOff_cn = new OnOff_Cn(); onOffTracker = new OnOffTracker();
    // Update() : onOff_cn = onOffTracker.Return_OnOff_Cn( ...params... ) <---returns the OnOff_Cn Type, which contains all the vars: on and fadeOutPhase, etc
    //********

    bool init = true;
    bool init_on;

    // inits as off because of init if() below
    bool  _on;
    // 0. - 1., 0. = begin fadeOut; 1. = end
    float _fadeOutPhase;
    // only for reporting:
    float _onDur;
    float _offDur;

    OnOff_Cn onOff_cn = new OnOff_Cn();

    float eventDur;
    float eventStartTime;
    float eventDeltaTime;

    // return type OnOff_Cn
    public OnOff_Cn Return_OnOff_Cn( bool init_on, Vector2 onDurRange, Vector2 offDurRange, float fadeDur )
    {
        if( init == true )
        {
            New_Off( offDurRange );
            init = false;
        }

        eventDeltaTime = Time.time - eventStartTime;

        if( eventDeltaTime > eventDur )
        {
            {
                if( _on == true )
                {
                    New_Off( offDurRange );
                }
                else
                {
                    New_On( onDurRange );
                }
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

        onOff_cn = new OnOff_Cn{ on = _on, fadeOutPhase = _fadeOutPhase, onDur = _onDur, offDur = _offDur };

        return onOff_cn;
    }

    void New_On( Vector2 onDurRange )
    {
        _on = true;
        eventStartTime = Time.time;
        eventDur = Random.Range( onDurRange[0], onDurRange[1] );
        _onDur = eventDur;
        // Debug.Log("tracker on " + _on + " dur " + eventDur);
    }

    void New_Off( Vector2 offDurRange )
    {
        _on = false;
        eventStartTime = Time.time;
        eventDur = Random.Range( offDurRange[0], offDurRange[1] );
        _offDur = eventDur;
        // Debug.Log("tracker on " + _on + " dur " + eventDur);
    }
}
