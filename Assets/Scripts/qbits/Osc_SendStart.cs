using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Osc_SendStart : MonoBehaviour 
{
    OscOut oscOutScript;
    bool start = true;

    // NOTE: this osc assets package makes it so In must be in Enable() 
    //       and Out must be in Update(); this also won't work in OnApplicationQuit() unfortunately

	void Update() 
    {
        if( start == true )
        {
            oscOutScript = GameObject.Find("osc").GetComponent<OscOut>();
            oscOutScript.Send( "/unityStart", 1 );
            start = false;
        }
    }
}
