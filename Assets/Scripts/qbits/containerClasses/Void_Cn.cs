using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Delaunay;

public class Void_Cn 
{
    public int id;
    public List<Triangle> initialTrianglesFromDelaunay;
    public List<BoundingQbit_AllInfo> boundingQbits_allInfo;
    public BoundingQbit_ConvexHullOrder_AllInfo[] boundingQbits_convexHullOrder_allInfo;
    public Vector2[] boundingQbits_convexHullOrder_positions;
    public Vector3 centroid;
    public float area;
    public bool isOpening;
    public bool isOpen;

    public bool startOpening;
    public bool startClosing;
    public float xfadeStartTime;
    public float xfadeTime;

    // the only igniterType we'll use is staticOnly;
    // shakeIt is now responsible for sending the qbits into orbit
    public bool hitby_igniter;
    public enum HitByVoidIgniterType { staticOnly, staticAndQbit };
    public      HitByVoidIgniterType hitby_voidIgniterType;

    public bool shakeIt_displacingBegin; // <-- transToOrbiting begin
    public bool shakeIt_springingBegin; //<-- orbiting begin if displaced long enough

    // each qubit finds its own parent void and adds its own InsideVoidQit_Cn to the Void_Cn
    public List<InsideVoidQbit_Cn> insideVoidQbits_allInfo = new List<InsideVoidQbit_Cn>();

    // create ability to make a deep copy, so this class can be copied with values only, 
    // without cloning, i.e. a shallow copy; without this, could never track voidsAllInfoPrev
    public Void_Cn DeepCopy()
    {
        Void_Cn deepCopy = new Void_Cn();
        deepCopy.id = this.id;
        deepCopy.initialTrianglesFromDelaunay = this.initialTrianglesFromDelaunay;
        deepCopy.boundingQbits_allInfo = this.boundingQbits_allInfo;
        deepCopy.boundingQbits_convexHullOrder_allInfo = this.boundingQbits_convexHullOrder_allInfo;
        deepCopy.centroid = this.centroid;
        deepCopy.area = this.area;
        deepCopy.isOpening = this.isOpening;
        deepCopy.isOpen = this.isOpen;
        deepCopy.startOpening = this.startOpening;
        deepCopy.startClosing = this.startClosing;
        deepCopy.xfadeStartTime = this.xfadeStartTime;
        deepCopy.xfadeTime = this.xfadeTime;
        deepCopy.hitby_igniter = this.hitby_igniter;
        deepCopy.hitby_voidIgniterType = this.hitby_voidIgniterType;
        deepCopy.shakeIt_displacingBegin = this.shakeIt_displacingBegin;
        deepCopy.shakeIt_springingBegin  = this.shakeIt_springingBegin;
        deepCopy.insideVoidQbits_allInfo = this.insideVoidQbits_allInfo;

        return deepCopy;
    }
}
