using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Geyser : MonoBehaviour 
{
    GlobalEvolution.GlobalEvolutionState globalEvolutionState;

    float mx_dissolveProbability_begin;
    float mx_dissolveProbability_beginCeiling;
    Vector2 mx_speedRange_begin;
    Vector2 mx_speedRange_beginCeiling;

    bool dissolveB4Ceiling;
    float dissolveProbability;
    Vector2 speedRange;
    float speed;

    void Start() 
    {
        EvolutionParams();

        dissolveB4Ceiling = Random.Range( 0f, 1f ) <= dissolveProbability;
        speed             = Random.Range( speedRange[0], speedRange[1] );
    }

    void EvolutionParams()
    {
        if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.begin )
        {
            dissolveProbability = 1;
            speedRange          = new Vector2( 2, 10 );
        }
        else if( globalEvolutionState == GlobalEvolution.GlobalEvolutionState.beginCeiling )  
        {
            dissolveProbability = .4f;
            speedRange          = new Vector2( 5, 11 );
        }
    }
}
