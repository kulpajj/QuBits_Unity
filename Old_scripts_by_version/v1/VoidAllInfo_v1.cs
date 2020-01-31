using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Delaunay;

public class VoidAllInfo 
{
    public int id;
    public List<Triangle> triangles;
    public HashSet<Vector3> boundingCoords;
    public Vector3 centroid;
    public float area;
    public bool startOpening;
    public bool startClosing;
    public bool draw;
    public float xfadeStartTime;
    public float xfadeTime;
}
