using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidStatic : MonoBehaviour 
{
    float lifetime;
    float timeOfBirth;
    float timeDelta;
    Vector3 currentPosition;
    int randomSignX;
    int randomSignZ;
    Vector3 randomForce;

    void Start () 
    {
        // maybe give it a leftime range - map lifetime and randomForce to whether delay added
        lifetime = .5f;
        timeOfBirth = Time.time;
        randomSignX = Random.Range( 0, 2 ) * 2 - 1;
        randomSignZ = Random.Range( 0, 2 ) * 2 - 1;
        randomForce = new Vector3( Random.Range( 100f, 200f ) * randomSignX, Random.Range( 500.0f, 1000.0f ), Random.Range( 100f, 200f ) * randomSignZ );
        GetComponent<Rigidbody>().AddForce( randomForce, ForceMode.Force );
	}
	
	void Update () 
    {
        timeDelta = Time.time - timeOfBirth;
        if( timeDelta >= lifetime )
        {
            Destroy( gameObject );
        }
    }
}
