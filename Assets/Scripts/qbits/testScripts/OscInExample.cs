using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscInExample : MonoBehaviour 
{
    OscIn oscInScript;
    public GameObject voidStaticPrefab;
    int self_id;

    void OnEnable()
    {
        if (!oscInScript)
        {
            oscInScript = GameObject.Find("osc").GetComponent<OscIn>();
            oscInScript.Open(8000);
        }
        ReceiveOsc();
    }

    void Start()
    {
        self_id = 0;
    }

    void ReceiveOsc()
    {
        // send an address callexd /0/void/static from max; pack a metro and udpsend it on port 8000
        oscInScript.MapInt("/" + self_id + "/void/static", RandomStatic);
    }

    void RandomStatic( int bang )
    {
        if( bang == 1 )
        {
            Debug.Log( bang );
        }
    }
}
