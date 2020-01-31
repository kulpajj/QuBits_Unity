using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class Qbit_InsideVoid_Time
{
    float[] frozen_ampLouds  = new float[6] { .1f, .2f, .3f, .4f, .7f, 1 };
    float[] frozen_ampQuiets = new float[5] { .002f, .005f, .007f, .008f, .009f };
    int     frozen_ampIndex;
    float   frozen_ampLerpStart;
    float   frozen_ampLerpDestination;
    public float frozen_ampCurrent;

    string frozen_direction;
    int    frozen_changeDirection;
    float  frozen_phraseDuration;
    float  frozen_phraseStartTime;
    float  frozen_phraseCurrTime;
    float  frozen_phase;

    // some folders are sounds that are too high, so type the acceptable ones into a list here
    List<int> frozen_folderChoices = new List<int> { 2, 3, 4, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 21, 22, 23 };
    int    frozen_folderIndex;
    int    frozen_folderIndexPrev = -1;
    int    frozen_folderNum;
    int    frozen_numSamplesInFolder;
    int    frozen_sampleNum;
    int    frozen_sampleNumPrev = -1;
    int    frozen_sampleRepeatCounter;
    int    frozen_sampleRepeatTotalTimes;
    public string frozen_sampleFilename;
    public float  frozen_sampleTransposition;
    Vector2       frozen_sampleTranspositionRange = new Vector2( .55f, .8f );
    public bool   frozen_sendNewSample;

    public string igniterEvent_sampleFilename;
    public float  igniterEvent_sampleDuration;

    public void Frozen( bool beginQtype3, JSONNode sampleInfo_JSONParsed )
    {
        if( beginQtype3 == true )
        {
            FrozenWood_NewSample();

            frozen_ampCurrent = 1;
            frozen_direction = "get quiet";
            Frozen_NewQuietAmp();
        }

        frozen_phraseCurrTime = Time.time - frozen_phraseStartTime;

        // phrase is still going...lerp amp
        if( frozen_phraseCurrTime < frozen_phraseDuration )
        {
            frozen_phase = frozen_phraseCurrTime / frozen_phraseDuration;
            frozen_ampCurrent = Mathf.Lerp( frozen_ampLerpStart, frozen_ampLerpDestination, frozen_phase );
        }
        // phrase is over; determine where we are going next and assign new values
        else
        {
            // the prev direction assignment
            if( frozen_direction == "get loud" )
            {
                // always get quiet if was just loud
                frozen_direction = "get quiet";
                Frozen_NewQuietAmp();
            }
            else if( frozen_direction == "get quiet" )
            {
                // weighted = usually stay quiet
                frozen_changeDirection = Random.Range( 1, 6 );
                if( frozen_changeDirection <= 2 ){ frozen_direction = "get loud"; }
                else{ frozen_direction = "get quiet"; }
            }

            // and now what to do with the new direction assignment
            if( frozen_direction == "get loud" )
            {
                Frozen_NewLoudAmp();
                FrozenWood_DetermineIfNewSample();
                frozen_sampleRepeatCounter++;
            }
            else if( frozen_direction == "get quiet" )
            {
                Frozen_NewQuietAmp();
            }
        }
    }

    void Frozen_NewLoudAmp()
    {
        frozen_ampLerpStart = frozen_ampCurrent;
        frozen_ampIndex = Random.Range( 0, frozen_ampLouds.Length );
        frozen_ampLerpDestination = frozen_ampLouds[ frozen_ampIndex ];
        frozen_phraseDuration = Random.Range( .7f, 4.0f );

        frozen_phraseStartTime = Time.time;
    }

    void Frozen_NewQuietAmp()
    {
        frozen_ampLerpStart = frozen_ampCurrent;
        frozen_ampIndex = Random.Range( 0, frozen_ampQuiets.Length );
        frozen_ampLerpDestination = frozen_ampQuiets[ frozen_ampIndex ];
        frozen_phraseDuration = Random.Range( 1.0f, 3.0f );

        frozen_phraseStartTime = Time.time;
    }

    void FrozenWood_DetermineIfNewSample()
    {
        if( frozen_sampleRepeatCounter >= frozen_sampleRepeatTotalTimes )
        {
            FrozenWood_NewSample();
        }
    }

    void FrozenWood_NewSample()
    {
        frozen_sampleNum = Random.Range( 1, 99 );
        if( frozen_sampleNumPrev != -1 )
        {
            if( frozen_sampleNum == frozen_sampleNumPrev )
            {
                frozen_sampleNum = ( frozen_sampleNum % 98 ) + 1;
            }
        }
        frozen_sampleFilename = "wood_dark_legato." + frozen_sampleNum;
        frozen_sampleTransposition = Random.Range( frozen_sampleTranspositionRange[0], frozen_sampleTranspositionRange[1] );

        frozen_sampleNumPrev = frozen_sampleNum;
        frozen_sendNewSample = true;
    }

    public string IgniterEvent_GetFileName()
    {
        // for every frozen sample, there is a corresponding freeze_rhythm_freeze sample
        // e.g. frozen14.5 corresponds to the frozen part of freeze_rhythm_freeze14.5 - it will morph to and from and sound smooth
        igniterEvent_sampleFilename = "freeze_rhythm_freeze" + frozen_folderNum + "." + frozen_sampleNum;
        return igniterEvent_sampleFilename;
    }

    public float IgniterEvent_GetFileDuration( JSONNode sampleInfo_JSONParsed )
    {
        // duration comes from Max
        igniterEvent_sampleDuration = sampleInfo_JSONParsed[ "freeze_rhythm_freeze/" + frozen_folderNum + "." + frozen_sampleNum + "/duration" ];
        return igniterEvent_sampleDuration;
    }

    public string IgniterEventEnding_GetFrozenFileName()
    {
        return frozen_sampleFilename;
    }
}
