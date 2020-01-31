using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateLightDiscs : MonoBehaviour 
{
    public int gridX = 6;
    public int gridZ = 6;
    float spacing;
    // offset is whatever "random" world coordinates I need to add to get the grid 
    // correctly situated in my world - not sure exactly why I have to add this but do
    float offset = -4.6f;
    float padding = 1.0f;
    float ceilingHeight;
    GameObject ceiling;
    GameObject newLightDisc;
    public GameObject prefabLightDisc;
    LightDiscMovement lightDiscMovementScript;
    public Transform lightDiscContainer;
    Light lightComponent;
    int id;
    Vector3 position;

    void Start()
    {
        ceiling = GameObject.Find( "ceiling" );
        spacing = ( ceiling.GetComponent<Renderer>().bounds.size.x / gridX ) - 1.0f;
        ceilingHeight = ceiling.transform.position.y;
        for( int x = 0; x < gridX; x++ )
        {
            for( int z = 0; z < gridZ; z++ )
            {
                id = ( z * gridZ ) + ( x % gridX );
                position = new Vector3( x + ( x - 1 ) * spacing + padding + offset, ceilingHeight - .25f, z + ( z - 1 ) * spacing + padding + offset );
                newLightDisc = Instantiate( prefabLightDisc, position, Quaternion.identity );
                newLightDisc.transform.SetParent( lightDiscContainer, true );

                lightDiscMovementScript = newLightDisc.GetComponent<LightDiscMovement>();
                lightComponent = newLightDisc.GetComponent<Light>();

                lightComponent.type = LightType.Point;
                lightComponent.enabled = false;
                newLightDisc.name = "lightDisc_" + id;
                lightDiscMovementScript.self_id = id;
            }
        }
    }
}
