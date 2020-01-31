using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Delaunay;
using Delaunay.Geo;

public class DelaunayTriangulation : MonoBehaviour 
{
    // editing tools for multiple prefabs:
    private GameObject[]    selectedGOs;
    private List< Vector2 > selectedGOs_XZ;
    private bool iAmTheSelectedTriangle;
    private bool iAmTheSelectedTriangle_andWasAdded;

    private List<Triangle> longestTriangles;
    private bool iWasAddedToTestVoidsOnThisLoop;

    // init
    public GameObject[] qbitsAll_obj;
    private GameObject[] voidsAll_obj;
    private List< DelaunayQbit_Cn > delaunayQbitsAllInfo;
    private Vector2 posXZ;
    private List<Vector2> delaunay_all_qbits_posXZ;
    private List< uint > colors;
    private bool getAllQbits_obj = true;

    public float floorWidth;
    public float floorHeight;
    private List< LineSegment > edges = null;
    private List< LineSegment > convexHull;
    private List< LineSegment > delaunayTriangulation;

    private List< Triangle > triangles;
    public List< Triangle > voidTriangles;
    public HashSet< Vector3 > voidBoundingCoords;
    // to create a matrix, use a list of custom classes!!!! ( here, my class VoidAllInfo stores my parameters and values )

    // legacy all info list...can delete someday - we now write self_voidsAllInfo into the mesh on instantiation
    public List<Void_Cn> delaunayVoidsAllInfo;

    private List<Triangle> testVoidTriangles;
    private Triangle voidLargestTrianglePartner;
    private List<Vector2> voidLargestTrianglePartner_longestEdgePts;

    private float voidSumCoordsX;
    private float voidSumCoordsZ;
    public Vector3 thisTestVoidCentroid;
    public float voidTrackingAreaThresh = 4.5f;
    public float voidXfadeAreaThresh = 5.0f;
    private float voidIDCentroidThreshold = 3.0f;
    private float voidTriangleEdge_longestAllowed = 3.0f;
    public float testVoidArea;
    private int newVoidId;
    int numPointsInCommon;
    bool voidAlreadyExists;
    bool id_alreadyExists;
    int id_assignmentCounter;
    GameObject[] currentVoidGOs;
    bool foundABoundingQbit;
    public List<BoundingQbit_AllInfo> qbitsBoundingMe;
    int foundBoundingQbitsCounter;

    List<int> currentVoidGoIds;
    bool entry_existsAs_go;
    int testSameEdgeCounter;
    int testNumLargest;
    int testNumSmaller;
    int testNumSmallerPrev;
    int testNumConvexHullLargest;
    int testNumConvexHullSmaller;
    int test_is2;
    int test_is3;
    int goPointTest;
    int addSelectedTo_testVoidTriangles;
    string whichAdjacent;
    Triangle voidLargestTrianglePartner_ofSelectedGroup;
    int test_numPtsInCommon_LargestPartner;
    int test_numTrianglesAfterAddPartner;

    public Transform voidContainer;
    public GameObject voidMeshPrefab;

    void Start()
    {
        floorWidth  = GameObject.Find( "floor" ).GetComponent<Renderer>().bounds.size.x;
        floorHeight = GameObject.Find( "floor" ).GetComponent<Renderer>().bounds.size.z;
    }

    void Update()
    {
        GetDelaunayQbitsPositions();
        RunDelaunay();
        FindVoidTriangles();
    }


    //________________________________________________________________________________________
    // positions and delaunay
    void GetDelaunayQbitsPositions()
    {
        // only do once on the first update():
        if( getAllQbits_obj == true )
        {
            // todo ONLY NEED TO REF ONCE?
            qbitsAll_obj = GameObject.FindGameObjectsWithTag( "qbit" );
            getAllQbits_obj = false;
        }

        // always clear the old lists, or else they grow forever:
        delaunayQbitsAllInfo = new List<DelaunayQbit_Cn>();
        delaunay_all_qbits_posXZ = new List<Vector2>();
        colors = new List< uint >();

        foreach( GameObject obj in qbitsAll_obj )
        {
            QbitMovement qbitMovement_script = obj.GetComponent<QbitMovement>();
            int this_qtype = qbitMovement_script.qtype;
            int this_id = qbitMovement_script.self_id;

            // so thus far, this if() is completely unnecessary but...keep it for now...
            if( this_qtype == 0 || this_qtype == 1 )
            {
                colors.Add( 0 );

                if( this_qtype == 0 )
                {
                    Vector3 centerPosition = qbitMovement_script.jittery_centerPosition;
                    posXZ = new Vector2( centerPosition.x, centerPosition.z );
                }
                else if( this_qtype == 1 && qbitMovement_script.brownian_moving == true )
                {
                    Vector3 centerPosition = qbitMovement_script.brownian_centerPosition;
                    posXZ = new Vector2( centerPosition.x, centerPosition.z );
                }
                else
                {
                    posXZ = new Vector2( obj.transform.position.x, obj.transform.position.z );
                }

                delaunayQbitsAllInfo.Add( new DelaunayQbit_Cn { id = this_id, posXZ = posXZ } );
                delaunay_all_qbits_posXZ.Add( posXZ );
            }
        }
    }

    void RunDelaunay()
    {
        Delaunay.Voronoi voronoi = new Delaunay.Voronoi( delaunay_all_qbits_posXZ, colors, new Rect( 0, 0, floorWidth, floorHeight ) );
        delaunayTriangulation = voronoi.DelaunayTriangulation();
        triangles = voronoi.Triangles();
        convexHull = voronoi.Hull();
    }

    //________________________________________________________________________________________
    // analyze the delaunay analysis, find voids
    void FindVoidTriangles()
    {
        delaunayVoidsAllInfo = new List<Void_Cn>();
        float longestEdgesSum = new float();
        voidLargestTrianglePartner_ofSelectedGroup = null;


        // CROSSCHECK       SYNC CHECK - remove from voidsAllInfo if I am still there but there is to corresponding GO
        // there is another SYNC CHECK in the VoidMesh script/gameobject


        // sort triangles by longest edge
        triangles.Sort( 
                            delegate( Triangle triangle1, Triangle triangle2 ) 
                            {
                                return triangle1.edgeLongest_length.CompareTo( triangle2.edgeLongest_length );
                            }
                      );

        // tag triangles on the convex hull ( can't be central largest triangles - but can be smaller triangles part of void )
        for( int i = triangles.Count - 1; i >= 0; i-- )
        {
            foreach( LineSegment hullLineSegment in convexHull )
            {
                if( triangles[i].sites[0].Coord == hullLineSegment.p0 ||
                    triangles[i].sites[0].Coord == hullLineSegment.p1 ||
                    triangles[i].sites[1].Coord == hullLineSegment.p0 ||
                    triangles[i].sites[1].Coord == hullLineSegment.p1 ||
                    triangles[i].sites[2].Coord == hullLineSegment.p0 ||
                    triangles[i].sites[2].Coord == hullLineSegment.p1 
                  )
                {
                    triangles[i].isAConvexHullTriangle = true;
                    break;
                }
                else
                {
                    longestEdgesSum += triangles[i].edgeLongest_length;
                }
            }
        }

        // **************************************************************************
        // the main script for finding voids:
        // **************************************************************************
        // test every triangle, and then remove it from the triangles list; 
        // on each loop, then, t then is always the longest edged triangle remaining to examine

        // figure out all triangles part of the void
        // see On Finding Large Polygonal Voids Using Delaunay Triangulation: The Case of Planar Point Sets
        // Carlos Herv´ıas, Nancy Hitschfeld-Kahler, Luis E. Campusano, and Giselle Font
        // pg 280

        // troubleshoot - selected XZ
        selectedGOs = UnityEditor.Selection.gameObjects;
        if (selectedGOs != null)
        {
            selectedGOs_XZ = new List<Vector2>();
            foreach ( GameObject GO in selectedGOs )
            {
                selectedGOs_XZ.Add( new Vector2( GO.transform.position.x, GO.transform.position.z ));
            }
        }

        // troubleshoot - draw longest
        longestTriangles = new List<Triangle>();

        if (selectedGOs.Length == 3)
        {
            Debug.Log("start frame");
        }

        // Debug.Log("start frame");

        for ( int t = triangles.Count - 1; t >= 0; t-- )
        {
            iWasAddedToTestVoidsOnThisLoop = false;
            test_numTrianglesAfterAddPartner = 0;

            // stop when there aren't enough triangles left in the triangles list to even form a void
            if ( t < 5 /* || triangles[t].edgeLongest <= voidLongestEdgeThreshold */ )
            {
                break;
            }
            else if( triangles[t].isAVoidSmallerTriangle == true || triangles[t].isAVoidLargestTriangle == true /* || triangles[t].isAConvexHullTriangle == true triangles[t].edgeLongest_length >= voidTriangleEdge_longestAllowed */ )
            {
                // do nothing - this needs to be here or runs super slow
                // triangles.RemoveAt(t);
            }
            else
            {
                testVoidTriangles  = new List<Triangle>();
                voidBoundingCoords = new HashSet<Vector3>();
                testVoidArea = 0.0f;
                voidLargestTrianglePartner = null;

                // the first central largest triangle:
                // CROSSCHECK I think only partner needs to be in the if() inside FindLargestTrianglePartner - maybe remove this if():
                if ( triangles[ t ].isAVoidSmallerTriangle == false /* && triangles[ t ].isAConvexHullTriangle == false */ )
                {
                    triangles[t].isAVoidLargestTriangle = true;
                    triangles[t].isAVoidSmallerTriangle = false;
                    testVoidTriangles.Add( triangles[ t ] );
                    testVoidArea += TriangleArea( triangles[ t ] );
                }

                FindLargestTrianglePartner( triangles[ t ], t );

                test_numTrianglesAfterAddPartner = testVoidTriangles.Count;

                whichAdjacent = "largest";
                FindSmallerTrianglesAdjacentTo( triangles[ t ], t );

                if (voidLargestTrianglePartner != null)
                {
                    whichAdjacent = "partner";
                    FindSmallerTrianglesAdjacentTo(voidLargestTrianglePartner, t);
                }

                if( selectedGOs.Length == 3)
                {
                    // troubleshoot - %%%% iter 2 %%%%
                    if (iAmTheSelectedTriangle_andWasAdded == true)
                    {
                        goPointTest = 0;
                        foreach (Triangle voidTriangle in testVoidTriangles)
                        {
                            if (goPointTest == 3) { break; }
                            goPointTest = 0;
                            for (int q = 0; q < 3; q++)
                            {
                                Vector2 voidTriangle_XZ = voidTriangle.sites[q].Coord;
                                foreach (Vector2 selectedGO_XZ in selectedGOs_XZ)
                                {
                                    if (voidTriangle_XZ == selectedGO_XZ)
                                    {
                                        goPointTest++;
                                        break;
                                    }
                                }
                            }
                        }

                        if (goPointTest == 3) 
                        { 
                            Debug.Log("iWasAdded - still in testVoids" + " whichAdjacent " + whichAdjacent + " loop " + t);
                            Debug.Log("#tris after addPartner() " + test_numTrianglesAfterAddPartner);
                        }
                        else { Debug.Log("iWasAdded - missing from testVoids" + " whichAdjacent " + whichAdjacent + " loop " + t); }
                    }


                    // troubleshoot - %%%% iter 3 %%%%
                    if (testVoidArea >= voidTrackingAreaThresh)
                    {
                        goPointTest = 0;
                        foreach (Triangle voidTriangle in testVoidTriangles)
                        {
                            if (goPointTest == 3) { break; }
                            goPointTest = 0;
                            for (int q = 0; q < 3; q++)
                            {
                                Vector2 voidTriangle_XZ = voidTriangle.sites[q].Coord;
                                foreach (Vector2 selectedGO_XZ in selectedGOs_XZ)
                                {
                                    if (voidTriangle_XZ == selectedGO_XZ)
                                    {
                                        goPointTest++;
                                        break;
                                    }
                                }
                            }
                        }

                        if (goPointTest == 3) { Debug.Log("AreaThresh reached - still in testVoids" + " whichAdjacent " + whichAdjacent + " loop " + t); }
                        else                  { Debug.Log("AreaThresh reached - missing from testVoids" + " whichAdjacent " + whichAdjacent + " loop " + t); }
                    }

                    // troubleshoot - partner pts
                    if (iAmTheSelectedTriangle_andWasAdded == true)
                    {
                        for (int q = 0; q < 3; q++)
                        {
                            Debug.Log("partnerPt " + voidLargestTrianglePartner.sites[q].Coord);
                        }
                    }
                }
 

                // triangles.RemoveAt( t );

                // **************************************************
                // add the group of triangles to the voidsAllInfo list
                if ( testVoidArea >= voidTrackingAreaThresh )
                {
                    CheckShitOutWithThisVoid();

                    // troubleshoot #small vs large
                    if (selectedGOs.Length == 3)
                    {
                        testNumSmaller = 0;
                        testNumLargest = 0;
                        foreach (Triangle triangle in testVoidTriangles)
                        {
                            if (triangle.isAVoidSmallerTriangle == true)
                            {
                                testNumSmaller++;
                            }
                            if (triangle.isAVoidLargestTriangle == true)
                            {
                                testNumLargest++;
                            }
                        }
                        if (testNumSmaller == 3)
                        {
                            Debug.Log("this void problem - largest " + testNumLargest + " smaller " + testNumSmaller);
                        }
                    }
                }

                // troubleshoot
                if (iWasAddedToTestVoidsOnThisLoop == true && selectedGOs.Length == 3)
                {
                    longestTriangles = new List<Triangle>(testVoidTriangles);
                    testNumConvexHullLargest = 0;
                    testNumConvexHullSmaller = 0;
                    testNumLargest = 0;
                    testNumSmaller = 0;
                    foreach (Triangle triangle in testVoidTriangles)
                    {
                        if (triangle.isAConvexHullTriangle == true && triangle.isAVoidLargestTriangle == true)
                        {
                            testNumConvexHullLargest++;
                        }
                        if (triangle.isAConvexHullTriangle == true && triangle.isAVoidSmallerTriangle == true)
                        {
                            testNumConvexHullSmaller++;
                        }
                        if (triangle.isAVoidLargestTriangle == true)
                        {
                            testNumLargest++;
                        }
                        if (triangle.isAVoidSmallerTriangle == true)
                        {
                            testNumSmaller++;
                        }
                    }
                    Debug.Log("#ptsInCommon_LargestPartner " + test_numPtsInCommon_LargestPartner);
                    Debug.Log("#largest " + testNumLargest + " #smaller " + testNumSmaller +  " #convexLargest " + testNumConvexHullLargest + " #convexSmaller " + testNumConvexHullSmaller);

                    voidLargestTrianglePartner_ofSelectedGroup = voidLargestTrianglePartner;
                }
            }
        }

        // troubleshoot - %%%% iter 4 %%%%
        goPointTest = 0;
        if (selectedGOs.Length == 3)
        {
            foreach( Void_Cn voidEntry in delaunayVoidsAllInfo)
            {
                if(goPointTest == 3) { break; }
                for (int tri = 0; tri < voidEntry.initialTrianglesFromDelaunay.Count; tri++ )
                {
                    if(goPointTest == 3) { break; }
                    goPointTest = 0;
                    for (int pt = 0; pt < 3; pt++)
                    {
                        Vector2 voidTriangle_XZ = voidEntry.initialTrianglesFromDelaunay[tri].sites[pt].Coord;
                        foreach (Vector2 selectedGO_XZ in selectedGOs_XZ)
                        {
                            if (voidTriangle_XZ == selectedGO_XZ)
                            {
                                goPointTest++;
                                break;
                            }
                        }
                    }
                }
            }

            if( goPointTest == 3 ){Debug.Log("exists in AllInfo");}
            else                  {Debug.Log("missing from Allinfo");}
        }

        // Debug.Log("#entries " + voidsAllInfo.Count);

        if( selectedGOs.Length == 3 )
        {
            Debug.Log( "#triangles " + triangles.Count );
            Debug.Log( "end frame" );
        }

        // Debug.Log("end frame");
    }

    public float TriangleArea(Triangle triangle)
    {
        Vector2 a = triangle.sites[0].Coord;
        Vector2 b = triangle.sites[1].Coord;
        Vector2 c = triangle.sites[2].Coord;

        float area = (a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y)) / 2;
        return Mathf.Abs( area );
    }

    void FindLargestTrianglePartner( Triangle toThisTriangle, int loopnum )
    {
        // to optimize: break entirely out of method when find two triangles
        // look at every other triangle...
        for( int i = triangles.Count - 1; i >= 0; i-- )
        {
            if (numPointsInCommon == 2) { break; }

            bool pointTest = false;
            numPointsInCommon = 0;
            Triangle otherTriangle = triangles[i];
            test_numPtsInCommon_LargestPartner = 0;

            // CROSSCHECK ...again, hashset? but again we need these checks in the if()
            if ( otherTriangle.isAVoidSmallerTriangle == false && otherTriangle.isAVoidLargestTriangle == false )
            {
                // test each point of this triangle's longest edge...
                for( int toThisTrianglePt = 0; toThisTrianglePt < 2; toThisTrianglePt++ )
                {
                    if (numPointsInCommon == 2) { break; }
                    // ...to every point of other triangle
                    for ( int otherTrianglePt = 0; otherTrianglePt < 3; otherTrianglePt++ )
                    {
                        pointTest = toThisTriangle.edgeLongest_coords[toThisTrianglePt].Coord == otherTriangle.sites[otherTrianglePt].Coord;
                        if( pointTest == true )
                        {
                            numPointsInCommon++;
                            if( numPointsInCommon == 2 )
                            {
                                triangles[i].isAVoidLargestTriangle = true;
                                triangles[i].isAVoidSmallerTriangle = false;

                                testVoidArea += TriangleArea(triangles[i]);
                                testVoidTriangles.Add(triangles[i]);

                                voidLargestTrianglePartner = triangles[i];
                                test_numPtsInCommon_LargestPartner = 2;
                            }
                            break;
                        }
                    }
                }
            }
        }
    }

    void FindSmallerTrianglesAdjacentTo( Triangle toThisLargestTriangle, int loopnum )
    {
        int numTrianglesFound = 0;
        iAmTheSelectedTriangle_andWasAdded = false;

        // look at every other triangle...
        for ( int i = triangles.Count - 1; i >= 0; i-- )
        {
            addSelectedTo_testVoidTriangles = 0;
            bool pointTest = false;
            numPointsInCommon = 0;
            Triangle otherTriangle = triangles[ i ];

            if( numTrianglesFound == 2 )
            {
                break;
            }

            // troubleshoot - i am the selected
            if(selectedGOs.Length == 3)
            {
                int numPointsInCommonWithSelected = 0;
                foreach ( Vector2 selectedGO_XZ in selectedGOs_XZ )
                {
                    if( numPointsInCommonWithSelected == 3 ){ break; }
                    foreach( Site otherSite in otherTriangle.sites )
                    {
                        if( selectedGO_XZ == otherSite.Coord )
                        {
                            numPointsInCommonWithSelected++;
                            break;
                        }
                    }
                }

                if(numPointsInCommonWithSelected == 3)
                {
                    iAmTheSelectedTriangle = true;
                }
                else
                {
                    iAmTheSelectedTriangle = false;
                }
            }

            // CROSSCHECK ...again, hashset? but again we need these checks in the if()
            if( otherTriangle.isAVoidSmallerTriangle == false && otherTriangle.isAVoidLargestTriangle == false )
            {
                // test every point of this triangle...
                for( int toThisLargestTrianglePt = 0; toThisLargestTrianglePt < 3; toThisLargestTrianglePt++ )
                {
                    if( numPointsInCommon == 2 ){ break; }
                    // ...to every point of other triangle
                    for( int otherTrianglePt = 0; otherTrianglePt < 3; otherTrianglePt++ )
                    {
                        pointTest = toThisLargestTriangle.sites[toThisLargestTrianglePt].Coord == otherTriangle.sites[otherTrianglePt].Coord;
                        if( pointTest == true )
                        {
                            numPointsInCommon++;
                            if( numPointsInCommon == 2 )
                            {
                                triangles[i].isAVoidSmallerTriangle = true;
                                triangles[i].isAVoidLargestTriangle = false;

                                testVoidArea += TriangleArea(triangles[i]);
                                testVoidTriangles.Add(triangles[i]);

                                addSelectedTo_testVoidTriangles = 1;

                                // troubleshoot - %%%% iter 1 %%%%
                                goPointTest = 0;
                                if( iAmTheSelectedTriangle == true)
                                {
                                    iAmTheSelectedTriangle_andWasAdded = true;
                                    foreach( Triangle voidTriangle in testVoidTriangles )
                                    {
                                        if( goPointTest == 3 ){break;}
                                        goPointTest = 0;
                                        for( int q = 0; q < 3; q++ )
                                        {
                                            Vector2 voidTriangle_XZ = voidTriangle.sites[q].Coord;
                                            foreach( Vector2 selectedGO_XZ in selectedGOs_XZ )
                                            {
                                                if( voidTriangle_XZ == selectedGO_XZ )
                                                {
                                                    goPointTest++;
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    if (goPointTest == 3) { Debug.Log("selected added to testVoids"); }
                                    else                  { Debug.Log("selected missing from testVoids"); }
                                }

                                numTrianglesFound++;
                            }
                            break;
                        }
                    }
                }
            }
        }

        // troubleshoot - iWasAdded in this loop of t
        if (iAmTheSelectedTriangle_andWasAdded == true)
        {
            iWasAddedToTestVoidsOnThisLoop = true;
        }
    }

    //______________________________________________________________________________________
    // spawn a new void or not
    // ******************************************************************************
    // self_voidAllInfo.triangles and id are first set by the DelaunayTriangulation script on spawning this mesh;
    // each mesh then contributes self_VoidAllInfo.area and centroid;
    // the VoidsAllInfo script aggragates all the self_voidAllInfo methods from all meshes;
    // the Qbit movement scripts check in with the VoidAllInfo script to find out if they are a bounding qbit;
    // ******************************************************************************
    void CheckShitOutWithThisVoid()
    {
        // get centroid
        currentVoidGOs = GameObject.FindGameObjectsWithTag("void");
        voidSumCoordsX = 0.0f;
        voidSumCoordsZ = 0.0f;
        GetVoidBoundingCoords(testVoidTriangles);
        foreach (Vector3 coord in voidBoundingCoords)
        {
            voidSumCoordsX += coord.x;
            voidSumCoordsZ += coord.z;
        }
        thisTestVoidCentroid = new Vector3(voidSumCoordsX / voidBoundingCoords.Count, 0.0f, voidSumCoordsZ / voidBoundingCoords.Count);


        if ( currentVoidGOs.Length == 0 )
        {
            SpawnVoid( 0 );
        }
        else
        {
            // voids are tracked by their centroids - check testVoid against the game object meshes already spawned
            // see if void already exists ( defined by threshold to GO centroid that already exists )
            currentVoidGoIds = new List<int>();
            voidAlreadyExists = false;

            foreach( GameObject go in currentVoidGOs )
            {
                int thisVoidGoId = go.GetComponent<VoidMesh>().self_id;
                currentVoidGoIds.Add(thisVoidGoId);

                if ( Vector3.Distance( thisTestVoidCentroid, go.GetComponent<VoidMesh>().meshCentroid ) <= voidIDCentroidThreshold )
                {
                    voidAlreadyExists = true;
                    break;
                    // ...update nothing, do nothing; the mesh takes care of itself
                }
            }

            // assign new void ids, spawn new void:
            if( voidAlreadyExists == false )
            {
                id_alreadyExists = true;
                id_assignmentCounter = 0;

                while( id_alreadyExists == true )
                {
                    id_alreadyExists = currentVoidGoIds.Contains( id_assignmentCounter );

                    if( id_alreadyExists == true )
                    {
                        id_assignmentCounter++;
                    }
                }

                SpawnVoid( id_assignmentCounter );
            }
        }
    }

    void GetVoidBoundingCoords( List<Triangle> triangles )
    {
        // ---bounding coords
        foreach(Triangle triangle in triangles)
        {
            voidBoundingCoords.Add(new Vector3(triangle.sites[0].Coord.x, 0.0f, triangle.sites[0].Coord.y));
            voidBoundingCoords.Add(new Vector3(triangle.sites[1].Coord.x, 0.0f, triangle.sites[1].Coord.y));
            voidBoundingCoords.Add(new Vector3(triangle.sites[2].Coord.x, 0.0f, triangle.sites[2].Coord.y));
        }
    }

    void SpawnVoid( int newVoidId )
    {
        qbitsBoundingMe = new List<BoundingQbit_AllInfo>();
        foundBoundingQbitsCounter = 0;

        foreach(GameObject qbit in qbitsAll_obj)
        {
            if( foundBoundingQbitsCounter == 8 ) { break; }
            foundABoundingQbit = false;
            Vector2 thisQbitPosition = new Vector2(qbit.transform.position.x, qbit.transform.position.z);

            for (int t = 0; t < testVoidTriangles.Count; t++)
            {
                if (foundABoundingQbit == true) { break; }
                for (int p = 0; p < 3; p++)
                {
                    if (thisQbitPosition == testVoidTriangles[t].sites[p].Coord)
                    {
                        bool qbitAddedAlready = false;
                        QbitMovement qbitMovementScript = qbit.GetComponent<QbitMovement>();
                        int thisQbitId = qbitMovementScript.self_id;
                        foreach( BoundingQbit_AllInfo qbitEntry in qbitsBoundingMe )
                        {
                            if( qbitEntry.id == thisQbitId )
                            {
                                qbitAddedAlready = true;
                                break;
                            }
                        }
                        if( qbitAddedAlready == false )
                        {
                            qbitsBoundingMe.Add( new BoundingQbit_AllInfo { id = thisQbitId, transform = qbit.transform, qbitMovementScript = qbitMovementScript } );
                            foundABoundingQbit = true;
                            foundBoundingQbitsCounter++;
                            break;
                        }
                    }
                }
            }
        }

        // spawn and set self_voidAllInfo of the spawned GO
        GameObject goVoid = Instantiate( voidMeshPrefab, new Vector3(0, .01f, 0), Quaternion.identity );
        goVoid.transform.SetParent( voidContainer, true );
        goVoid.name = "void_" + newVoidId;
        VoidMesh voidMeshScript = goVoid.GetComponent<VoidMesh>();
        voidMeshScript.self_voidAllInfo = new Void_Cn { id = newVoidId, initialTrianglesFromDelaunay = testVoidTriangles, boundingQbits_allInfo = qbitsBoundingMe, area = testVoidArea, centroid = thisTestVoidCentroid };
    }

    //________________________________________________________________________________________
    // gizmos draw

    void OnDrawGizmos()
    {
        // delaunay triangles' line segments
       if ( delaunayTriangulation != null )
       {
            Gizmos.color = Color.gray;
            for( int i = 0; i < delaunayTriangulation.Count; i++ )
            {
               Vector2 point0 = (Vector2)delaunayTriangulation[i].p0;
               Vector2 point1 = (Vector2)delaunayTriangulation[i].p1;

               //rotate so the analysis is now in the X and Z dimensions:
               Vector3 left = new Vector3( point0.x, 0.0f, point0.y );
               Vector3 right = new Vector3( point1.x, 0.0f, point1.y );
               Gizmos.DrawLine( left, right );
            }
       }

       /*
        if( delaunayVoidsAllInfo != null )
        {
            foreach( Void_Cn voidEntry in delaunayVoidsAllInfo )
            {
                for (int i = 0; i < voidEntry.initialTrianglesFromDelaunay.Count; i++)
                {
                    if(voidEntry.initialTrianglesFromDelaunay[i].isAVoidLargestTriangle == true)
                    {
                        Gizmos.color = Color.yellow;
                    }
                    else if( voidEntry.initialTrianglesFromDelaunay[i].isAVoidSmallerTriangle == true)
                    {
                        Gizmos.color = Color.clear;
                    }
                    Vector2 pt0 = voidEntry.initialTrianglesFromDelaunay[i].sites[0].Coord;
                    Vector2 pt1 = voidEntry.initialTrianglesFromDelaunay[i].sites[1].Coord;
                    Vector2 pt2 = voidEntry.initialTrianglesFromDelaunay[i].sites[2].Coord;

                    Vector3 point0 = new Vector3(pt0.x, 0, pt0.y);
                    Vector3 point1 = new Vector3(pt1.x, 0, pt1.y);
                    Vector3 point2 = new Vector3(pt2.x, 0, pt2.y);
                    Gizmos.DrawLine(point0, point1);
                    Gizmos.DrawLine(point1, point2);
                    Gizmos.DrawLine(point2, point0);

                    
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(point0, .1f);
                    Gizmos.DrawSphere(point1, .1f);
                    Gizmos.DrawSphere(point2, .1f);
                                      
                }
            }
        }*/


        /*
        // convex hull
        if (convexHull != null)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < convexHull.Count; i++)
            {
                Vector2 point0 = (Vector2)convexHull[i].p0;
                Vector2 point1 = (Vector2)convexHull[i].p1;

                //rotate so the analysis is now in the X and Z dimensions:
                Vector3 left = new Vector3(point0.x, 0.0f, point0.y);
                Vector3 right = new Vector3(point1.x, 0.0f, point1.y);
                Gizmos.DrawLine(left, right);
            }
        }*/

        /*
        if (longestTriangles != null)
        {
            for (int i = 0; i < longestTriangles.Count; i++)
            {
                if (longestTriangles[i].isAVoidSmallerTriangle == true)
                {
                    Gizmos.color = Color.green;
                    Vector2 pt0 = longestTriangles[i].sites[0].Coord;
                    Vector2 pt1 = longestTriangles[i].sites[1].Coord;
                    Vector2 pt2 = longestTriangles[i].sites[2].Coord;

                    Vector3 point0 = new Vector3(pt0.x, 0, pt0.y);
                    Vector3 point1 = new Vector3(pt1.x, 0, pt1.y);
                    Vector3 point2 = new Vector3(pt2.x, 0, pt2.y);
                    Gizmos.DrawLine(point0, point1);
                    Gizmos.DrawLine(point1, point2);
                    Gizmos.DrawLine(point2, point0);
                }
            }
        }

        if (longestTriangles != null)
        {
            for (int i = 0; i < longestTriangles.Count; i++)
            {
                if( longestTriangles[i].isAVoidLargestTriangle == true )
                {
                    Gizmos.color = Color.red;
                    Vector2 pt0 = longestTriangles[i].sites[0].Coord;
                    Vector2 pt1 = longestTriangles[i].sites[1].Coord;
                    Vector2 pt2 = longestTriangles[i].sites[2].Coord;

                    Vector3 point0 = new Vector3(pt0.x, 0, pt0.y);
                    Vector3 point1 = new Vector3(pt1.x, 0, pt1.y);
                    Vector3 point2 = new Vector3(pt2.x, 0, pt2.y);
                    Gizmos.DrawLine(point0, point1);
                    Gizmos.DrawLine(point1, point2);
                    Gizmos.DrawLine(point2, point0);
                }
            }
        }

        if(voidLargestTrianglePartner_ofSelectedGroup != null)
        {
            Vector2 pt0 = voidLargestTrianglePartner_ofSelectedGroup.sites[0].Coord;
            Vector2 pt1 = voidLargestTrianglePartner_ofSelectedGroup.sites[1].Coord;
            Vector2 pt2 = voidLargestTrianglePartner_ofSelectedGroup.sites[2].Coord;

            Vector3 point0 = new Vector3(pt0.x, 0, pt0.y);
            Vector3 point1 = new Vector3(pt1.x, 0, pt1.y);
            Vector3 point2 = new Vector3(pt2.x, 0, pt2.y);
            Gizmos.DrawLine(point0, point1);
            Gizmos.DrawLine(point1, point2);
            Gizmos.DrawLine(point2, point0);
        }*/
    }

}
