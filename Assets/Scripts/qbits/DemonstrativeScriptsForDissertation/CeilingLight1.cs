using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CeilingLight1 : MonoBehaviour 
{
    // **** declare variable types
    GlobalEvolution.GlobalEvolutionState globalEvolution;
	
    // **** run once per frame
	void Update() 
    {
        if( globalEvolution == GlobalEvolution.GlobalEvolutionState.beginCeiling ) 
        {
            CheckGlow1();
        }
        else if (globalEvolution == GlobalEvolution.GlobalEvolutionState.final ) 
        {
            CheckGlow2();
        }
    }

    // **** define methods
    void CheckGlow1()
    {
        // code to see if hit by geyser
    }

    void CheckGlow2()
    {
        // code to glow by self
    }
}
