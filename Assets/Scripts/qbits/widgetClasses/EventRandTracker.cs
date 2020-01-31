using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventRandTracker 
{
    //********
    // -what-
    // schedules random events ( gives you bool eventStartClick = true ) provided random duration ranges; 
    //                           eventStartClick = true lasts for one frame, i.e. a data click

    // -from other scripts- 
    // in Start(), instantiate an instance of this script's class, EventRandTracker; there is no container class
    //             as this class only returns the bool eventStartClick;
    // Start()  : eventRandTracker = new EventRandTracker();
    // Update() : bool eventStartClick = eventRandTracker.Return_EventStartClick( Vector2 eventDurRange )
    //********

    bool init = true;

    // inits as off because of init if() below
    public bool _eventStartClick;

    float eventDur;
    float eventStartTime;
    float eventDeltaTime;

    // return type OnOff_Cn
    public bool Return_EventStartClick( Vector2 eventDurRange )
    {
        _eventStartClick = false;

        if( init == true )
        {
            New_Event( eventDurRange );
            init = false;
        }

        eventDeltaTime = Time.time - eventStartTime;

        if( eventDeltaTime > eventDur )
        {
            New_Event( eventDurRange );
        }

        return _eventStartClick;
    }

    void New_Event( Vector2 eventDurRange )
    {
        _eventStartClick = true;
        eventStartTime = Time.time;
        eventDur = Random.Range( eventDurRange[0], eventDurRange[1] );
    }
}
