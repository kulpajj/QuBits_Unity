using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightDiscs_NumBusyFlare : MonoBehaviour 
{
    // DESIGN
    // this script exists for the lowerAndRougher state and to prevent CPU overload of too many lightDiscs playing at once
    // each lightDiscMovement script comes here to find out how many other lightDiscMovement scripts are currently busyFlare == true
    // each lightDiscMovement script won't play if the current lightDiscs_numBusyFlare is at the max number allowed, this max num being set in the mixer script

    // global evolution
    GlobalEvolution globalEvolution_script;
    GlobalEvolution.GlobalEvolutionState globalEvolutionState;
    List<LightDiscMovement> lightDiscScripts = new List<LightDiscMovement>();

    bool       firstFrame = true;
    public int lightDiscs_numBusyFlare;

    void Start() 
    {
        globalEvolution_script = GameObject.Find( "globalEvolution" ).GetComponent<GlobalEvolution>();
    }
	
	void Update() 
    {

        if( firstFrame == true )
        {
            GameObject[] lightDiscGOs = GameObject.FindGameObjectsWithTag( "disc" );
            foreach( GameObject lightDiscGO in lightDiscGOs )
            {
                lightDiscScripts.Add(lightDiscGO.GetComponent<LightDiscMovement>());
            }

            firstFrame = false;
        }

        globalEvolutionState = globalEvolution_script.globalEvolutionState;
        if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.lowerAndRougher )
        {
            lightDiscs_numBusyFlare = 0;
            foreach( LightDiscMovement lightDiscMovement in lightDiscScripts )
            {
                if( lightDiscMovement.busyFlare == true )
                {
                    lightDiscs_numBusyFlare++;
                }
            }
            // Debug.Log("numbusy " + lightDiscs_numBusyFlare);
        }
    }
}
