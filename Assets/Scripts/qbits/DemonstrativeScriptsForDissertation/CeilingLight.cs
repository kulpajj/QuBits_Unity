using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CeilingLight : MonoBehaviour 
{
    // **** declare variable types
    GlobalEvolution.GlobalEvolutionState globalEvolution;
    delegate void CheckGlowDelegate();
    CheckGlowDelegate CheckGlow;
	
    // **** run once per frame
	void Update () 
    {
        EvolutionParams();
        CheckGlow();
	}

    // **** set delegate to specific method 
    void EvolutionParams()
    {
        if( globalEvolution == GlobalEvolution.GlobalEvolutionState.beginCeiling )
        {
            CheckGlow = CheckGlow1;
        }
        else if( globalEvolution == GlobalEvolution.GlobalEvolutionState.final ) 
        {
            CheckGlow = CheckGlow2;
        }
    }

    // **** define methods
    void CheckGlow1()
    {
        // code to see if hit by geyser
    }

    void CheckGlow2()
    {
        // code to glow by self (random timer)
    }
}
