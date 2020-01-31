using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomJitteryId : MonoBehaviour 
{
    private int frameCounter;
    private int frameCounterMax;
    private int sameFrequencyCounter;
    private int sameFrequencyCounterMax;
    public int jitteryQbitId;
    public string jitteryFrequency;
    private InstantiateQbits instantiateQbitsScript;
    private int maxQbitId;

    void Start()
    {
        MakeNewJitteryID();
        WeightedRandFrequency();
        instantiateQbitsScript = GameObject.Find("instantiateQbits").GetComponent<InstantiateQbits>();
        maxQbitId = instantiateQbitsScript.gridX * instantiateQbitsScript.gridZ - 1;
    }

    void Update()
    {
        // report framerate:
        // Debug.Log( 1 / Time.deltaTime );

        // each Jittery event:
        if( frameCounter <= frameCounterMax )
        {
            frameCounter++;
            jitteryQbitId = -1;
        }
        else
        {
            // select a new frequency of jittery events ( infrequent, medium, or frequent ) 
            if (sameFrequencyCounter <= sameFrequencyCounterMax)
            {
                sameFrequencyCounter++;
            }
            else
            {
                WeightedRandFrequency();
            }

            MakeNewJitteryID();
        }

    }

    public void MakeNewJitteryID()
    {
        frameCounter = 0;
        jitteryQbitId = Random.Range( 0, maxQbitId );
        switch (jitteryFrequency)
        {
            case "infrequent":
                frameCounterMax = Random.Range(100, 720); break;
            case "medium":
                frameCounterMax = Random.Range(80, 450); break;
            case "frequent":
                frameCounterMax = Random.Range(30, 100); break;
        }
    }

    public void WeightedRandFrequency()
    {
        int prob_of_infrequent= 40;
        int prob_of_medium = 20;
        int prob_of_frequent = 40;

        int sum_of_probs = prob_of_infrequent + prob_of_medium + prob_of_frequent;

        int rand = Random.Range( 0, sum_of_probs );
        if( ( rand -= prob_of_infrequent ) < 0 )
        {
            jitteryFrequency = "infrequent";
            // number of times to use the same frequency before calling
            // this Method again, randomly selecting another frequency
            sameFrequencyCounterMax = Random.Range( 3, 5 );
        }
        else if( ( rand -= prob_of_medium ) < 0 )
        {
            jitteryFrequency = "medium";
            sameFrequencyCounterMax = Random.Range( 4, 6 );
        }
        else if ( ( rand -= prob_of_frequent ) < 0 )
        {
            jitteryFrequency = "frequent";
            sameFrequencyCounterMax = Random.Range( 5, 7 );
        }


        sameFrequencyCounter = 0;
        // Debug.Log(jitteryFrequency);
    }
}
