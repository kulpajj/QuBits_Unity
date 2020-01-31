using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidsAllInfo : MonoBehaviour 
{
    private GameObject[] voidGOsAll;
    public List<Void_Cn> voidsAllInfo;

    private GameObject selectedGO;
    private GameObject[] selectedGOs;

    void Update()
    {
        voidGOsAll = GameObject.FindGameObjectsWithTag( "void" );
        voidsAllInfo = new List<Void_Cn>();

        foreach( GameObject voidGO in voidGOsAll )
        {
            VoidMesh voidMeshScript = voidGO.GetComponent<VoidMesh>();
            voidsAllInfo.Add( voidMeshScript.self_voidAllInfo );
        }
    }
}
