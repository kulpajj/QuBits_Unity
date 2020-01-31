using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowAndFade  
{
    //********
    // -what-
    // grow ( crescendo ) to a target value for a duration ( both random ranges ), 
    // then stay there for a duration ( random range )
    // then diminish/fade ( decrescendo ) to a target value for a duration ( both random ranges ), 
    // then stay there for a duration ( random range )
    //     ...or first fade and then grow, depending on initial parameter values indicated
    // repeat as long as called in the Update()

    // -from other scripts- ( no container script )
    // Start()  : growAndFade = new GrowAndFade(); 
    // Update() : floatValue  = growAndFade.ReturnCurrentValue( ...params... )
    //********

    public float _currentValue;
    float _currentValuePrev;
    public bool _fadedAndStayingPutClick;

    bool init = true;

    bool fade;    // else grow
    bool stayPut; // else change
    float change_initialVal;
    float change_targetVal;
    float change_duration;
    float stayPut_duration;

    float lerp_startTime;
    float lerp_deltaTime;
    float lerp_phase;

    // needs initial settings: init_stayPut indicates stay or change; init_fade indicates to first target fade or grow
    // needs 6 random ranges:
    // the min and max ranges for the value you are growing/diminishing, and ranges for durations of growing, fading, and staying put
    public float ReturnCurrentValue( bool init_stayPut, bool init_fade, Vector2 targetGrow, Vector2 targetFade, Vector2 durToGrow, Vector2 durToFade, Vector2 durToStay_grown, Vector2 durToStay_faded )
    {
        _fadedAndStayingPutClick = false;

        if( init == true )
        {
            if( init_fade == false )
            {
                // init growing, we'll start from the smallest value:
                _currentValuePrev = targetFade[0];
            }
            else
            {
                // init fading, we'll start from a random large value:
                _currentValuePrev = Random.Range( targetGrow[0], targetGrow[1] );
            }

            stayPut = init_stayPut;
            // we do this like setting the init_fadePrev to the opposite of what we want; New_Change() will flip it back to the requested value
            fade = !init_fade; 
            if( stayPut == false )
            {
                New_Change( targetGrow, targetFade, durToGrow, durToFade );
            }
            else
            {
                New_StayPut( durToStay_grown, durToStay_faded );
            }
            init = false;
        }

        lerp_deltaTime = Time.time - lerp_startTime;

        if( stayPut == true )
        {
            lerp_phase = lerp_deltaTime / stayPut_duration;
            if( lerp_phase > 1f )
            {
                New_Change( targetGrow, targetFade, durToGrow, durToFade );
            }
        }
        else
        {
            lerp_phase = lerp_deltaTime / change_duration;
            if( lerp_phase > 1 )
            {
                New_StayPut( durToStay_grown, durToStay_faded );
            }
            else
            {
                _currentValue = Mathf.Lerp( change_initialVal, change_targetVal, lerp_phase );
            }
        }

        _currentValuePrev = _currentValue;

        return _currentValue;
    }

    void New_Change( Vector2 targetGrow, Vector2 targetFade, Vector2 durToGrow, Vector2 durToFade )
    {
        stayPut = false;
        fade = !fade;
        change_initialVal = _currentValuePrev;

        if( fade == false )
        {
            change_targetVal = Random.Range( targetGrow[0], targetGrow[1] );
            change_duration  = Random.Range( durToGrow[0], durToGrow[1] );
        }
        else
        {
            change_targetVal = Random.Range( targetFade[0], targetFade[1] );
            change_duration  = Random.Range( durToFade[0], durToFade[1] );
        }

        lerp_startTime = Time.time;
    }

    void New_StayPut( Vector2 durToStay_grown, Vector2 durToStay_faded )
    {
        stayPut = true;
        if( fade == true )
        {
            stayPut_duration = Random.Range( durToStay_faded[0], durToStay_faded[1] );
            _fadedAndStayingPutClick = true;
        }
        else
        {
            stayPut_duration = Random.Range( durToStay_grown[0], durToStay_grown[1] );
        }

        lerp_startTime = Time.time;
    }
}
