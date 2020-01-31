using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmOrbitingEvents : MonoBehaviour 
{
    VoidsAllInfo voidsAllInfo_script;
    List<Void_Cn> voidsAllInfo;
    List<InsideVoidQbit_Cn> insideVoidQbits_allInfo;
    bool qbitCurrentlyOrbiting;
    bool qbitCurrentlyOrbitingPrev;
    bool makeNewGlobalAmp = true;
    bool firstIgniterEvent = true;

    public float orbitingGlobalAmp;
    public Vector2 orbitingGlobalAmpRange = new Vector2( .08f, .4f );

    public float orbitingLocalAmp;
    public float orbitingLocalAmp_min;
    public float orbitingLocalAmp_max;
    string dynamicDirection;
    string dynamicDirectionPrev;
    float  dynamicDirection_startAmp;
    float  dynamicDirection_targetAmp;
    float  dynamicDirection_duration;
    float  dynamicDirection_startTime;
    float  dynamicDirection_deltaTime;
    float  dynamicDirection_phase;

    // each qbitMovement script reports its orbiting status to VoidsAllInfo.insideVoidQbits_allInfo
    // this script then makes a new global dynamic when there are currently no orbiting qbits in any void
    // this script also makes a local dynamic for crescendos and decrescendos which globally applies to all orbiting qbits in any void

    void Start()
    {
        voidsAllInfo_script = GameObject.Find( "voidsAllInfo" ).GetComponent<VoidsAllInfo>();
    }

    void Update()
    {
        voidsAllInfo = voidsAllInfo_script.voidsAllInfo;
        if( voidsAllInfo != null )
        {
            // is anyone orbiting?
            qbitCurrentlyOrbiting = false;
            foreach( Void_Cn voidEntry in voidsAllInfo )
            {
                if( qbitCurrentlyOrbiting == true ) { break; }
                insideVoidQbits_allInfo = voidEntry.insideVoidQbits_allInfo;
                if( insideVoidQbits_allInfo != null )
                {
                    foreach( InsideVoidQbit_Cn qbitsEntry in insideVoidQbits_allInfo )
                    {
                        if( qbitsEntry.orbiting == true )
                        {
                            qbitCurrentlyOrbiting = true;
                            break;
                        }
                    }
                }
            }

            // globalAmp
            if( qbitCurrentlyOrbiting == false && makeNewGlobalAmp == true )
            {
                if( firstIgniterEvent == true )
                {
                    orbitingGlobalAmp = .1f;
                    firstIgniterEvent = false;
                }
                else
                {
                    orbitingGlobalAmp = Random.Range( orbitingGlobalAmpRange[0], orbitingGlobalAmpRange[1] );
                }
                // if a loud global amp, lower the local decrescendos more...
                orbitingLocalAmp_max = Scale( orbitingGlobalAmp, orbitingGlobalAmpRange[0], orbitingGlobalAmpRange[1], .5f, .2f );
                // if a quiet global amp, boost the local crescendos more...
                orbitingLocalAmp_max = Scale( orbitingGlobalAmp, orbitingGlobalAmpRange[0], orbitingGlobalAmpRange[1], 10f, 0f );
                makeNewGlobalAmp = false;
            }

            // localAmp
            if( qbitCurrentlyOrbiting == true )
            {
                // first instance when go from no one orbiting to someone orbiting:
                if( qbitCurrentlyOrbitingPrev == false )
                {
                    FirstDynamicDirection();
                }

                if( dynamicDirection_deltaTime > dynamicDirection_duration )
                {
                    NewDynamicDirection();
                }

                dynamicDirection_deltaTime = Time.time - dynamicDirection_startTime;
                dynamicDirection_phase = dynamicDirection_deltaTime / dynamicDirection_duration;
                orbitingLocalAmp = Mathf.Lerp( dynamicDirection_startAmp, dynamicDirection_targetAmp, dynamicDirection_phase );

                // Debug.Log("duration " + dynamicDirection_duration + " delta " + dynamicDirection_deltaTime + " phase " + dynamicDirection_phase + " dir " + dynamicDirection );
            }

            if( qbitCurrentlyOrbiting == true && makeNewGlobalAmp == false )
            {
                makeNewGlobalAmp = true;
            }

            qbitCurrentlyOrbitingPrev = qbitCurrentlyOrbiting;
            dynamicDirectionPrev = dynamicDirection;
        }
    }

    void FirstDynamicDirection()
    {
        dynamicDirection_startTime = Time.time;

        if( orbitingGlobalAmp >= orbitingGlobalAmpRange[1] * .5f )
        {
            dynamicDirection = "decresc";
        }
        else
        {
            dynamicDirection = "cresc";
        }

        dynamicDirection_startAmp = orbitingLocalAmp;

        if( dynamicDirection == "cresc" )
        {
            dynamicDirection_targetAmp = orbitingLocalAmp_max;
            dynamicDirection_duration = Random.Range( 5f, 6f );
        }
        else
        {
            // so actually, stay the same if you started off a loud phrase:
            dynamicDirection_targetAmp = orbitingLocalAmp;
            dynamicDirection_duration = Random.Range( 5f, 6f );
        }
    }

    void NewDynamicDirection()
    {
        dynamicDirection_startTime = Time.time;

        if( dynamicDirectionPrev == "cresc" )
        {
            dynamicDirection = "decresc";
        }
        else
        {
            dynamicDirection = "cresc";
        }

        dynamicDirection_startAmp = orbitingLocalAmp;

        if( dynamicDirection == "cresc" )
        {
            dynamicDirection_targetAmp = orbitingLocalAmp_max;
            dynamicDirection_duration = Random.Range( 1f, 5f );
        }
        else
        {
            dynamicDirection_targetAmp = orbitingLocalAmp_min;
            dynamicDirection_duration = Random.Range( 1f, 2f );
        }
    }

    public float Scale( float oldValue, float oldMin, float oldMax, float newMin, float newMax )
    {

        float oldRange = oldMax - oldMin;
        float newRange = newMax - newMin;
        float newValue = (((oldValue - oldMin) * newRange) / oldRange) + newMin;

        return newValue;
    }
}
