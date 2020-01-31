using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowAndFadeTest : MonoBehaviour 
{
    GrowAndFade growAndFade;
    float currentValue;
    Vector2 targetGrowRange = new Vector2(3.2f, 4.4f);
    Vector2 targetFadeRange = new Vector2(.1f, .3f);
    Vector2 durToGrowRange = new Vector2(5f, 7f);
    Vector2 durToFadeRange = new Vector2(3f, 5f);
    Vector2 durToStayGrownRange = new Vector2(1f, 2f);
    Vector2 durToStayFadedRange = new Vector2(4f, 5f);

    void Start () 
    {
        growAndFade = new GrowAndFade();
    }
	
	void Update () 
    {
        // init: grow, don't stay put
        currentValue = growAndFade.ReturnCurrentValue( false, false, targetGrowRange, targetFadeRange, durToGrowRange, durToFadeRange, durToStayGrownRange, durToStayFadedRange );
        Debug.Log( currentValue );
	}
}
