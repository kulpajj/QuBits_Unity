using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Qbit : MonoBehaviour 
{
    GrowAndFade growAndFade = new GrowAndFade();
    float tentacle_height;

    // values for arguments
    bool    start_hold     = false;
    bool    start_grow     = true;
    Vector2 targetGrow     = new Vector2( .6f, 2.5f );
    Vector2 durToGrow      = new Vector2( 2, 6 );
    Vector2 targetFade     = new Vector2( 0f, .1f );
    Vector2 durToFade      = new Vector2( 3, 4 );
    Vector2 durToHoldGrown = new Vector2( 1, 3 );
    Vector2 durToHoldFaded = new Vector2( 2, 5 );

    // run once per frame
    void Update() 
    {
        tentacle_height = growAndFade.ReturnCurrentValue( start_hold, start_grow, targetGrow, durToGrow, 
                                                          targetFade, durToFade, durToHoldGrown, durToHoldFaded );  
	}
}
