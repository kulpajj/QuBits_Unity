using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QbitUnusedMethods : MonoBehaviour {

    public GameObject[] qbitsAll;
    private Vector3 position;
    public int self_id;

    // useful shit, but not yet used in the project
    void FindNearestQbits()
    {
        GameObject closestID1 = null;
        GameObject closestID2 = null;
        float distanceSmallest1 = Mathf.Infinity;
        float distanceSmallest2 = Mathf.Infinity;
        position = this.transform.position;
        string selfObject = "qbit_" + self_id;
        foreach (GameObject qbit in qbitsAll)
        {
            if (qbit.name != selfObject)
            {
                Vector3 diff = position - qbit.transform.position;
                float thisDistance = diff.sqrMagnitude;
                if (thisDistance < distanceSmallest1)
                {
                    distanceSmallest2 = distanceSmallest1;
                    closestID2 = closestID1;

                    distanceSmallest1 = thisDistance;
                    closestID1 = qbit;
                }
                else if (thisDistance < distanceSmallest2)
                {
                    closestID2 = qbit;
                    distanceSmallest2 = thisDistance;
                }
            }
        }
    }

}
