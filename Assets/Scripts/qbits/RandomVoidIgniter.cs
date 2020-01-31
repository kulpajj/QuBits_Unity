using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomVoidIgniter : MonoBehaviour 
{
    // mixer
    Mixer mixer;

    // ****
    // mixer values
    float mx_igniter_prob = .0037f;

    // troubleshoot technique: instantiate GO with spacebar at mouse position
    private Ray troubleshootRay;
    private RaycastHit troubleshootHit;
    public Vector3 troubleshootHitpoint;
    public Camera troubleshootRayCamera;
    // then place all this in Update()
    /*
        if( Input.GetKeyDown(KeyCode.Space))
        {
            troubleshootRay = troubleshootRayCamera.ScreenPointToRay( Input.mousePosition );
            if( Physics.Raycast(troubleshootRay, out troubleshootHit))
            {
                if( troubleshootHit.collider.name == "floor" )
                {
                    troubleshootHitpoint = troubleshootHit.point;
                    Instantiate( prefab, troubleshootHitpoint, Quaternion.identity);
                }
            }
        } 
     */

    VoidsAllInfo voidsAllInfo_script;
    float randomValue;
    GameObject[] activeIgniterGOs;
    List<int>    activeIgniterIDs;
    bool igniterID_foundNew;
    int  igniterID_new;
    GameObject igniterGO_new;
    public GameObject igniterPrefab;
    public Transform igniterContainer;
    GameObject ceiling;
    float ceilingY;
    Vector3 ceilingMin;
    Vector3 ceilingMax;
    float ceilingOffset = 1.3f;
    Vector3 position;

    void Start()
    {
        voidsAllInfo_script = GameObject.Find( "voidsAllInfo" ).GetComponent<VoidsAllInfo>();
        ceiling = GameObject.Find( "ceiling" );
        ceilingY = ceiling.transform.position.y;
        ceilingMin = ceiling.GetComponent<Renderer>().bounds.min;
        ceilingMax = ceiling.GetComponent<Renderer>().bounds.max;
        voidsAllInfo_script = GameObject.Find( "voidsAllInfo" ).GetComponent<VoidsAllInfo>();

        troubleshootRayCamera = GameObject.Find( "camera_explorer" ).GetComponent<Camera>();

        mixer = new Mixer();
        MixerValues_Init();
    }

    void Update()
    {
        randomValue = Random.Range( 0.0f, 1.0f );
        if( randomValue <= mx_igniter_prob )
        {
            GetNewID();

            position = new Vector3( Random.Range( ceilingMin.x + ceilingOffset, ceilingMax.x - ceilingOffset ), ceilingY, Random.Range( ceilingMin.z + ceilingOffset, ceilingMax.z - ceilingOffset ) );
            igniterGO_new = Instantiate( igniterPrefab, position, Quaternion.identity );
            igniterGO_new.transform.SetParent( igniterContainer );
            igniterGO_new.GetComponent<VoidIgniterMovement>().self_id = igniterID_new;
        }

        /*
        if( voidsAllInfo_script.voidsAllInfo != null )
        {
            if( voidsAllInfo_script.voidsAllInfo.Count >= 1 )
            {
                randomValue = Random.Range( 0.0f, 1.0f );
                if( randomValue <= igniter_randomWeighted )
                {
                    position = new Vector3( Random.Range( ceilingMin.x + ceilingOffset, ceilingMax.x - ceilingOffset), ceilingY, Random.Range( ceilingMin.z + ceilingOffset, ceilingMax.z - ceilingOffset ) );
                    igniterGO = Instantiate( igniterPrefab, position, Quaternion.identity );
                    igniterGO.transform.SetParent( igniterContainer );
                }
            }
        }*/

        if ( Input.GetKeyDown( KeyCode.Space ) )
        {
            troubleshootRay = troubleshootRayCamera.ScreenPointToRay( Input.mousePosition );
            if( Physics.Raycast( troubleshootRay, out troubleshootHit ) )
            {
                if( troubleshootHit.collider.name == "floor" )
                {
                    troubleshootHitpoint = troubleshootHit.point;
                    Instantiate( igniterPrefab, new Vector3( troubleshootHitpoint.x, ceilingY, troubleshootHitpoint.z ), Quaternion.identity );
                }
            }
        }
    }

    void GetNewID()
    {
        activeIgniterGOs = GameObject.FindGameObjectsWithTag( "voidIgniter" );

        if( activeIgniterGOs.Length == 0 )
        {
            igniterID_new = 1;
        }
        else
        {
            activeIgniterIDs = new List<int>();
            foreach( GameObject igniterGO in activeIgniterGOs )
            {
                activeIgniterIDs.Add( igniterGO.GetComponent<VoidIgniterMovement>().self_id );
            }

            igniterID_new = 1;
            igniterID_foundNew = false;
            while( igniterID_foundNew == false )
            {
                foreach( int id in activeIgniterIDs )
                {
                    if( igniterID_new != id )
                    {
                        igniterID_foundNew = true;
                    }
                    else
                    {
                        igniterID_foundNew = false;
                        break;
                    }
                }

                if( igniterID_foundNew == false )
                {
                    igniterID_new++;
                }
            }
        }
    }

    void MixerValues_Init()
    {
        mx_igniter_prob = mixer.igniter_prob;
    }
}
