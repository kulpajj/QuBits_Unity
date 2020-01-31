using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateQbits : MonoBehaviour {

    public GameObject prefabQbit;
    public int gridX = 11;
    public int gridZ = 11;
    public float spacing;
    // offset 0 is the middle of the floor
    // this puts the lower left corner qbit ( id 0 ) at a consistent place on the floor and builds from there
    public float offset = -4.6f;
    private float scale = .07f;
    public Transform qbitsContainer;
    private Rigidbody rigidbody;
    private SphereCollider self_collider;

    void Start()
    {
        spacing = GameObject.Find( "floor" ).GetComponent<Renderer>().bounds.size.x / gridX;
        for( int z = 0; z < gridZ; z++ )
        {
            for( int x = 0; x < gridX; x++ )
            {
                float id = ( z * gridZ ) + ( x % gridX );
                Vector3 position = new Vector3( x , 0, z ) * spacing;
                position = new Vector3( position.x + offset, 0, position.z + offset);
                var qbit = Instantiate( prefabQbit, position, Quaternion.identity );
                qbit.name = prefabQbit.name + "_" + id;
                qbit.transform.SetParent( qbitsContainer, true );
                qbit.transform.localScale = new Vector3( scale, scale, scale );

                rigidbody = qbit.GetComponent<Rigidbody>();
                rigidbody.isKinematic = false;

                self_collider = qbit.GetComponent<SphereCollider>();
                self_collider.enabled = true;
            }
        }
    }
}
