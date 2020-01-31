using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsideVoidQbit_Cn 
{
    // this class is bundled into the Void_Cn; however, each qubit finds its own parent and adds its own InsideVoidQit_Cn to the Void_Cn

    public int id;
        // globalEvolution script needs to know if what's inside is just a qtype1 being bounced out, or a true qtype3 that evolves the space forward
    public int qtype;
        // position added just to troubleshoot qtype3s with gizmos in globalEvolution script
    public Vector3 position; 
    public bool orbiting;
}
