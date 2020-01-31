using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SimpleJSON;

public class LoadJSON_FromMax : MonoBehaviour 
{
    private string filePath_samplesInfo = "/Users/jonathankulpa/sounds/samples_info.json";
    public string dataAsJSON;
    public JSONNode sampleInfo_JSONParsed;

    void Start ()
    {
        // File. stuff needs "using System.IO"
        if ( File.Exists( filePath_samplesInfo ) )
        {
            dataAsJSON = File.ReadAllText( filePath_samplesInfo );
            sampleInfo_JSONParsed = JSON.Parse( dataAsJSON );
        }
        else
        {
            Debug.Log( "there is no samples_info file!" );
        }
    }
}
