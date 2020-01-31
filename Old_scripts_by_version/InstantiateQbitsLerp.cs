using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateQbitsLerp : MonoBehaviour 
{
    public GameObject prefabQbit;
    public float gridX = 10.0f;
    public float gridY = 10.0f;
    public float spacing = 1.0f;
    public float offset = -4.5f;

    void Start()
    {
        for (int z = 0; z < gridY; z++)
        {
            for (int x = 0; x < gridX; x++)
            {
                float id = ( z * gridY ) + (x % gridX);
                Vector3 position = new Vector3( x + offset, 0, z + offset ) * spacing;
                var qbit = Instantiate( prefabQbit, position, Quaternion.identity );
                qbit.name = prefabQbit.name + "_" + id;
            }
        }
    }
}
