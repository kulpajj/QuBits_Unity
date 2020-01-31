using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalLightMovement : MonoBehaviour 
{
    // osc
    OscOut oscOut_script;

    // mixer
    Mixer mixer;

    bool         rotating;
    float        rotation_x;
    float        rotation_x_euler_prev;
    public float rotation_x_euler;
    // **** 
    // these Quaternions determine the x-axis rotation angle range, but so that we can report that to other scripts, figure it out 
    // and enter it as the euler range
    Quaternion rotationPole_brightest = new Quaternion( -.6f, .3f, -.6f, -.3f );
    Quaternion rotationPole_darkest   = new Quaternion( -.7f, 0f, -.7f, 0f );
    // **** ....so enter the angle range here... 
    public Vector2 aud_rotationChange_eulerRange = new Vector2( 0f, 54f );
    bool           rotationChange_begin_toDarkness;
    // this bool controls whether to lerp all the way to brightest or darkest, else stop somewhere sooner:
    bool           rotationChange_stopB4Pole;
    Vector2        rotationChange_stopB4Pole_phaseToStopRange_toBrightness = new Vector2( .05f, .2f );
    Vector2        rotationChange_stopB4Pole_phaseToStopRange_toDarkness   = new Vector2( .93f, 1.2f );
    Vector2        rotationChange_gotoPole_phaseToStopRange                = new Vector2( .93f, 1.2f );
    float          rotationChange_phaseToStop;
    Quaternion     rotationChange_targetRotation;
    // note: you can't say if Quaternion == Quaternion like you can with Vector3 == Vector3
    bool           rotationChange_targetDark;
    Vector2        rotationChange_durationRange = new Vector2( 4f, 6f );
    float          rotationChange_duration;
    float          rotationChange_startTime;
    Quaternion     rotationChange_initialRotation;
    float          rotationChange_phase;
    float          rotationChange_deltaTime;

    Vector2 stayAtRotation_durationRange_pole = new Vector2( 6f, 14f );
    Vector2 stayAtRotation_durationRange_stopB4Pole = new Vector2( 3f, 4f );
    float   stayAtRotation_duration;
    float   stayAtRotation_startTime;
    float   stayAtRotation_deltaTime;




    void Start()
    {
        oscOut_script = GameObject.Find( "osc" ).GetComponent<OscOut>();
        rotation_x_euler = transform.eulerAngles.x;
        rotation_x_euler_prev = rotation_x_euler;
        rotationChange_targetRotation = rotationPole_brightest;
        rotationChange_targetDark = true;

        mixer = new Mixer();
        MixerValues_Init();

        rotationChange_targetDark = true;
        rotationChange_stopB4Pole = false;
        New_StayAtRotation();
        // Debug.Log(stayAtRotation_duration);

        ReportOscStart();
        ReportOscUpdate();
    }
	
	void Update() 
    {
        rotation_x = this.transform.rotation.x;
        rotation_x_euler = transform.eulerAngles.x;

        // rotating
        if( rotating == true )
        {
            rotationChange_deltaTime = Time.time - rotationChange_startTime;
            rotationChange_phase = rotationChange_deltaTime / rotationChange_duration;
            if( rotationChange_phase <= rotationChange_phaseToStop )
            {
                this.transform.rotation = Quaternion.Lerp( rotationChange_initialRotation, rotationChange_targetRotation, rotationChange_phase );
            }
            else
            {
                New_StayAtRotation();
            }

            if( rotation_x_euler != rotation_x_euler_prev )
            {
                ReportOscUpdate();
            }
        }
        // stay put
        else
        {
            stayAtRotation_deltaTime = Time.time - stayAtRotation_startTime;
            if( stayAtRotation_deltaTime > stayAtRotation_duration )
            {
                New_RotationChange();
            }
        }

        rotation_x_euler_prev = rotation_x_euler;
	}

    void New_StayAtRotation()
    {
        rotating = false;
        stayAtRotation_startTime = Time.time;

        if( rotationChange_stopB4Pole == false )
        {
            stayAtRotation_duration = Random.Range( stayAtRotation_durationRange_pole[0], stayAtRotation_durationRange_pole[1] );
        }
        else
        {
            stayAtRotation_duration = Random.Range( stayAtRotation_durationRange_stopB4Pole[0], stayAtRotation_durationRange_stopB4Pole[1]);
        }
    }

    void New_RotationChange()
    {
        rotating = true;
        rotationChange_startTime = Time.time;
        rotationChange_initialRotation = this.transform.rotation;
        rotationChange_duration = Random.Range( rotationChange_durationRange[0], rotationChange_durationRange[1] );

        if( rotationChange_targetDark == true )
        {
            rotationChange_targetRotation = rotationPole_darkest;
            rotationChange_begin_toDarkness = true;
            // Debug.Log("toDark");
        }
        else
        {
            rotationChange_targetRotation = rotationPole_brightest;
        }

        rotationChange_stopB4Pole = Random.Range( 0, 2 ) == 1;
        if( rotationChange_stopB4Pole == true )
        {
            if( rotationChange_targetDark == true )
            {
                rotationChange_phaseToStop = Random.Range( rotationChange_stopB4Pole_phaseToStopRange_toDarkness[0], rotationChange_stopB4Pole_phaseToStopRange_toDarkness[1] );
            }
            else
            {
                rotationChange_phaseToStop = Random.Range( rotationChange_stopB4Pole_phaseToStopRange_toBrightness[0], rotationChange_stopB4Pole_phaseToStopRange_toBrightness[1] );
            }
        }
        else
        {
            // if not stopping before reach the pole, the destination phase is around 1:
            rotationChange_phaseToStop = Random.Range( rotationChange_gotoPole_phaseToStopRange[0], rotationChange_gotoPole_phaseToStopRange[1] );
        }

        // Debug.Log( "toDark " + rotationChange_targetDark + " phaseToStop " + rotationChange_phaseToStop );
        
        rotationChange_targetDark = !rotationChange_targetDark; //<--- turn false into true and vice versa for next time around
    }

    void ReportOscStart()
    {
        oscOut_script.Send( "/directionalLight/rotation/range", aud_rotationChange_eulerRange[0], aud_rotationChange_eulerRange[1] );
    }

    void ReportOscUpdate()
    {
        oscOut_script.Send( "/directionalLight/rotation/x", rotation_x_euler );
        // Debug.Log(rotation_x_euler);
        if( rotationChange_begin_toDarkness == true && rotation_x_euler >= 30 )
        {
            oscOut_script.Send( "/directionalLight/rotation/headToDarkness", 1 );
            rotationChange_begin_toDarkness = false;
        }
    }

    void MixerValues_Init()
    {

    }
}
