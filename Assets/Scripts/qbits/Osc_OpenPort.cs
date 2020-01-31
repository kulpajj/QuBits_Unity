using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Osc_OpenPort : MonoBehaviour 
{
    OscOut oscOut;
    OscIn oscIn;

    void Start()
    {
        GameObject osc = GameObject.Find( "osc" );
        oscOut = osc.GetComponent<OscOut>();
        oscIn = osc.GetComponent<OscIn>();

        oscOut.Open( 7000 );
        oscIn.Open( 8000 );

        // Debug.Log( "!!! Make sure Max udpsend is set to IPAddress " + OscIn.ipAddress );
        Debug.Log( "!!! If orbits aren't working, in Max, try banging 'init_sound_libraries' and then restart the Scene" );
    }
}
