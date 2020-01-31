using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JitteryMovement : MonoBehaviour 
{
    public int jittering = 0;
    private string grow_or_shrink = "grow";
    public Vector3 centerPosition;
    public float distFromCenter = 0.0f;
    private float distIncrement = 0.0f;
    public float distMax = 1.0f;
    public int jitteryDirection;
    private float xTranslate;
    private float zTranslate;
    private Rigidbody self_rigidbody;
    private SphereCollider self_collider;


    // currentSign is responsible for the illusion of the particle "splitting into two"
    // every other rendering, it translates/flips the position around the center point
    private int currentSign = 1;
    private int frameCount = 0;


    void Start()
    {
        self_rigidbody = this.GetComponent<Rigidbody>();
        self_collider = this.GetComponent<SphereCollider>();
        JitteryRestart();
    }

    void Update()
    {
        Jittery();
    }

    void JitteryRestart()
    {
        centerPosition = new Vector3( this.transform.position.x, this.transform.position.y, this.transform.position.z );
        jittering = 0;
        grow_or_shrink = "grow";
        distMax = Random.Range( .3f, 1.0f );
        self_rigidbody.isKinematic = true;
        self_collider.enabled = false;

        jitteryDirection = Random.Range( 1, 5 );
    }

    void Jittery()
    {
        if( jittering == 1 )
        {
            //only render every 5 frames - 60 fps is way too fast for the desired look
            if( frameCount % 5 == 0 )
            {
                switch( jitteryDirection )
                {
                    case 1: xTranslate = currentSign * distFromCenter; zTranslate = currentSign * distFromCenter; break;
                    case 2: xTranslate = currentSign * distFromCenter; zTranslate = -1 * currentSign * distFromCenter; break;
                    case 3: xTranslate = currentSign * distFromCenter; zTranslate = 0; break;
                    case 4: xTranslate = 0; zTranslate = currentSign * distFromCenter; break;
                }
                this.transform.position = new Vector3( centerPosition.x + xTranslate, centerPosition.y, centerPosition.z + zTranslate );

                // new dist from center
                if( grow_or_shrink == "grow" )
                {
                    distIncrement = Random.Range( .005f, .08f );
                    distFromCenter += distIncrement;
                }
                else
                {
                    if( distFromCenter > .03 )
                    {
                        distIncrement = Random.Range( .01f, .03f );
                    }
                    else
                    {
                        distIncrement = Random.Range( .0005f, .0008f );
                    }
                    distFromCenter -= distIncrement;
                }

                // increase or decrease; stop when done
                currentSign *= -1;
                if( distFromCenter >= distMax )
                {
                    grow_or_shrink = "shrink";
                }

                if( grow_or_shrink == "shrink" && distFromCenter <= 0 )
                {
                    JitteryRestart();
                }

            }

            frameCount++;
        }
    }

}
