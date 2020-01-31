using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalEvolution : MonoBehaviour 
{
    // ><>  ><>  ><>  ><>
    // this script is responsible for when to switch the globalEvolutionState which corresponds to my sketch/plan
    // ususally things evolve based on how many qtype 3s are trapped inside the voids, and later by how many voids have collapsed
    // thus, this script needs a reference to VoidsAllInfo to count the num of qtype3s in the system
    // ><>  ><>  ><>  ><>

    public enum GlobalEvolutionState { begin, beginCeiling, tallerTubes, beginSpin, lowerAndRougher, final };
    public GlobalEvolutionState globalEvolutionState;
    VoidsAllInfo voidsAllInfo_script;
    int qtype3_num;

    void Start()
    {
        voidsAllInfo_script = GameObject.Find( "voidsAllInfo" ).GetComponent<VoidsAllInfo>();
        globalEvolutionState = GlobalEvolutionState.begin;
    }

    void Update()
    {
        if( voidsAllInfo_script.voidsAllInfo != null )
        {
            qtype3_num = 0;
            foreach( Void_Cn voidEntry in voidsAllInfo_script.voidsAllInfo )
            {
                foreach( InsideVoidQbit_Cn qbitEntry in voidEntry.insideVoidQbits_allInfo )
                {
                    if( qbitEntry != null )
                    {
                        if( qbitEntry.qtype == 3 )
                        {
                            qtype3_num++;
                        }
                    }
                }
            }

            // Debug.Log(qtype3_num);

            //**** design with c#: because csharp is stateful, use == N; even if there is a gap: == 3, == 5, == 4 will still be covered by == 3
            //****                 no need to, and do not, use >= 3 && <= 5 
            // NOTE: begin testing/scripting a change at 2 captured qtype3s - need the first captured to init EvolutionParams() while GlobalEvolutionState == begin, or don't get the begin values
            if( qtype3_num == 0 )
            {
                globalEvolutionState = GlobalEvolutionState.begin;
            }
            else if( qtype3_num == 2 )
            {
                globalEvolutionState = GlobalEvolutionState.beginCeiling;
            }
            else if( qtype3_num == 4 )
            {
                globalEvolutionState = GlobalEvolutionState.tallerTubes;
            }
            else if( qtype3_num == 6 )
            {
                globalEvolutionState = GlobalEvolutionState.beginSpin;
            }
            else if( qtype3_num >= 7 )
            {
                globalEvolutionState = GlobalEvolutionState.lowerAndRougher;
            }

            if( Input.GetKeyDown( KeyCode.E ) )
            {
                Debug.Log( globalEvolutionState + " q3_num " + qtype3_num );
            }
        }
        else
        {
            // set back to begin if all voids gone
            globalEvolutionState = GlobalEvolutionState.begin;
        }
    }

    /*
    void OnDrawGizmos()
    {
        if( voidsAllInfo_script.voidsAllInfo != null )
        {
            foreach (Void_Cn voidEntry in voidsAllInfo_script.voidsAllInfo)
            {
                foreach (InsideVoidQbit_Cn qbitEntry in voidEntry.insideVoidQbits_allInfo)
                {
                    if (qbitEntry != null)
                    {
                        if (qbitEntry.qtype == 3)
                        {
                            Gizmos.color = Color.yellow;
                            Gizmos.DrawSphere(qbitEntry.position, .1f);
                        }
                    }
                }
            }
        }
    }*/
}
