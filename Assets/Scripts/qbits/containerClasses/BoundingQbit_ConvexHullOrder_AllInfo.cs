using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundingQbit_ConvexHullOrder_AllInfo 
{
    // the Method in QbitMovement.cs that detects qbits inside voids needs the pts as an array
    // we also use these arrays to detect whether qbits are convex in the VoidMesh script
    public int id;
    public Vector2 position;
    public bool expandToMakeConvex;
    public float iAmFormingThisVertexAngle;
}
