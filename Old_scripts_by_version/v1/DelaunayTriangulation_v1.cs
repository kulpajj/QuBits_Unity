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
    private List< DelaunayQbitAllInfo > delaunayQbitsAllInfoCurr;
    private List< DelaunayQbitAllInfo > delaunayQbitsAllInfoPrev;
    private Vector2 posXZ_curr;
    private Vector2 posXZ_prev;
    private Vector2 delaunay_qbit_posXZ_weighted;
    private List<Vector2> delaunay_all_qbits_posXZ_weighted;
    private bool was_delaunay_qbit_previously;
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
    public List<VoidAllInfo> voidsAllInfo;
    private List<VoidAllInfo> voidsAllInfoPrev;

    private List<Triangle> testVoidTriangles;
    private Triangle voidLargestTrianglePartner;
    private List<Vector2> voidLargestTrianglePartner_longestEdgePts;

    private float voidSumCoordsX;
    private float voidSumCoordsZ;
    public Vector3 thisVoidCentroid;
    public float voidTrackingAreaThresh = 4.0f;
    public float voidXfadeAreaThresh = 4.5f;
    private float voidTriangleEdge_longestAllowed = 3.0f;
    public float testVoidArea;
    private int thisVoidId;
    private float voidIDCentroidThreshold = 1.5f;
    int numPointsInCommon;
    bool voidAlreadyExists;
    bool id_alreadyExists;
    int id_assignmentCounter;
    List<int> previousVoidIds;
    List<int> currentVoidEntryIds;
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

    public GameObject voidMeshPrefab;

    void Start()
    {
        floorWidth  = GameObject.Find( "floor" ).GetComponent<Renderer>().bounds.size.x;
        floorHeight = GameObject.Find( "floor" ).GetComponent<Renderer>().bounds.size.z;
        voidsAllInfoPrev = null;
    }

    void Update()
    {
        //troubleshoot infinite draw
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
            qbitsAll_obj = GameObject.FindGameObjectsWithTag( "qbit" );
            getAllQbits_obj = false;
        }

        // always clear the old lists, or else they grow forever:
        delaunayQbitsAllInfoCurr = new List<DelaunayQbitAllInfo>();
        if( delaunayQbitsAllInfoPrev == null )
        {
            delaunayQbitsAllInfoPrev = new List<DelaunayQbitAllInfo>();
        }
        delaunay_all_qbits_posXZ_weighted = new List<Vector2>();
        colors = new List< uint >();

        foreach( GameObject obj in qbitsAll_obj )
        {
            int this_qtype = obj.GetComponent<QbitMovement>().qtype;
            int this_id = obj.GetComponent<QbitMovement>().self_id;

            if( this_qtype == 0 || this_qtype == 1 )
            {
                colors.Add( 0 );

                if( this_qtype == 0 )
                {
                    Vector3 centerPosition = obj.GetComponent<QbitMovement>().centerPosition;
                    posXZ_curr = new Vector2( centerPosition.x, centerPosition.z );
                }
                else
                {
                    posXZ_curr = new Vector2(obj.transform.position.x, obj.transform.position.z);
                }

                foreach( DelaunayQbitAllInfo qbit in delaunayQbitsAllInfoPrev )
                {
                    if( this_id == qbit.id )
                    {
                        was_delaunay_qbit_previously = true;
                        posXZ_prev = qbit.posXZ;
                        break;
                    }
                    else
                    {
                       was_delaunay_qbit_previously = false;
                    }
                }

                if( was_delaunay_qbit_previously == false )
                {
                    delaunay_all_qbits_posXZ_weighted.Add( posXZ_curr );
                    delaunayQbitsAllInfoCurr.Add(new DelaunayQbitAllInfo { id = this_id, posXZ = posXZ_curr });
                }
                else
                {
                    delaunay_qbit_posXZ_weighted = 1.0f * posXZ_curr + 0.0f * posXZ_prev;

                    delaunay_all_qbits_posXZ_weighted.Add( delaunay_qbit_posXZ_weighted );
                    delaunayQbitsAllInfoCurr.Add( new DelaunayQbitAllInfo { id = this_id, posXZ = delaunay_qbit_posXZ_weighted } );
                }
            }
        }

        delaunayQbitsAllInfoPrev = new List<DelaunayQbitAllInfo>( delaunayQbitsAllInfoCurr );
    }

    void RunDelaunay()
    {
        Delaunay.Voronoi voronoi = new Delaunay.Voronoi( delaunay_all_qbits_posXZ_weighted, colors, new Rect( 0, 0, floorWidth, floorHeight ) );
        delaunayTriangulation = voronoi.DelaunayTriangulation();
        triangles = voronoi.Triangles();
        convexHull = voronoi.Hull();
    }

    //________________________________________________________________________________________
    // analyze the delaunay analysis, find voids
    void FindVoidTriangles()
    {
        voidsAllInfo = new List<VoidAllInfo>();
        if( voidsAllInfoPrev == null )
        {
            voidsAllInfoPrev = new List<VoidAllInfo>();
        }
        float longestEdgesSum = new float();
        voidLargestTrianglePartner_ofSelectedGroup = null;


        // CROSSCHECK       SYNC CHECK - remove from voidsAllInfo if I am still there but there is to corresponding GO
        // there is another SYNC CHECK in the VoidMesh script/gameobject

        if (voidsAllInfo.Count > 0)
        {
            voidsAll_obj = GameObject.FindGameObjectsWithTag("void");
            if (currentVoidGoIds.Count > 0)
            {
                for( int e = voidsAllInfo.Count - 1; e >= 0; e++ )
                {
                    foreach (GameObject goVoid in voidsAll_obj)
                    {
                        entry_existsAs_go = voidsAllInfo[e].id == goVoid.GetComponent<VoidMesh>().self_id;
                        if( entry_existsAs_go == true )
                        {
                            break;
                        }
                    }

                    if( entry_existsAs_go == false )
                    {
                        voidsAllInfo.RemoveAt( e );
                    }
                }
            }
        }

        if ( voidsAllInfo.Count > 0 )
        {
            voidsAll_obj = GameObject.FindGameObjectsWithTag("void");
            foreach (GameObject goVoid in voidsAll_obj)
            {
                currentVoidGoIds.Add( goVoid.GetComponent<VoidMesh>().self_id );
            }


            foreach (VoidAllInfo voidEntry in voidsAllInfo)
            {
                currentVoidEntryIds.Add(voidEntry.id);
            }
        }

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
                        else                  { Debug.Log("iWasAdded - missing from testVoids" + " whichAdjacent " + whichAdjacent + " loop " + t); }
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
                    UpdateAllInfoListWithThisVoid();

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
            foreach( VoidAllInfo voidEntry in voidsAllInfo)
            {
                if(goPointTest == 3) { break; }
                for (int tri = 0; tri < voidEntry.triangles.Count; tri++ )
                {
                    if(goPointTest == 3) { break; }
                    goPointTest = 0;
                    for (int pt = 0; pt < 3; pt++)
                    {
                        Vector2 voidTriangle_XZ = voidEntry.triangles[tri].sites[pt].Coord;
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

        voidsAllInfoPrev = new List<VoidAllInfo>( voidsAllInfo );
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
    // update voidsAllInfo list with thisVoid and spawn new voids
    void UpdateAllInfoListWithThisVoid()
    {
        // ---bounding coords
        foreach( Triangle triangle in testVoidTriangles )
        {
            GetVoidBoundingCoords(triangle);
        }

        // ---centroid
        voidSumCoordsX = 0.0f;
        voidSumCoordsZ = 0.0f;
        foreach( Vector3 coord in voidBoundingCoords )
        {
            voidSumCoordsX += coord.x;
            voidSumCoordsZ += coord.z;
        }
        thisVoidCentroid = new Vector3( voidSumCoordsX / voidBoundingCoords.Count, 0.0f, voidSumCoordsZ / voidBoundingCoords.Count );

        // ---assign id
        AssignId_and_Spawn();

        // ---add
        if( voidsAllInfo.Count == 0 )
        {
            voidsAllInfo.Add(new VoidAllInfo { id = thisVoidId, triangles = testVoidTriangles, boundingCoords = voidBoundingCoords, centroid = thisVoidCentroid, area = testVoidArea });
        }
        else
        {
            /* CROSSCHECK cuz somehow a group of the same triangles 
             ( or a similar group of triangles? ) could otherwise get added twice as 
             the same ID - this is the entire reason for the existence of currentvoidEntryIds */
            foreach (VoidAllInfo voidEntry in voidsAllInfo)
            {
                currentVoidEntryIds.Add(voidEntry.id);
            }
            if (!currentVoidEntryIds.Contains(thisVoidId))
            {
                voidsAllInfo.Add(new VoidAllInfo { id = thisVoidId, triangles = testVoidTriangles, boundingCoords = voidBoundingCoords, centroid = thisVoidCentroid, area = testVoidArea });
            }
        }

    }

    void GetVoidBoundingCoords(Triangle triangle)
    {
        voidBoundingCoords.Add(new Vector3(triangle.sites[0].Coord.x, 0.0f, triangle.sites[0].Coord.y));
        voidBoundingCoords.Add(new Vector3(triangle.sites[1].Coord.x, 0.0f, triangle.sites[1].Coord.y));
        voidBoundingCoords.Add(new Vector3(triangle.sites[2].Coord.x, 0.0f, triangle.sites[2].Coord.y));
    }

    void AssignId_and_Spawn()
    {
        // Debug.Log("assign");
        if( voidsAllInfoPrev == null )
        {
            // nothing, just need for init
        }
        else if( voidsAllInfoPrev.Count == 0 )
        {
            // Debug.Log("new 0");
            thisVoidId = 0;
            GameObject goVoid = Instantiate(voidMeshPrefab, new Vector3(0, .01f, 0), Quaternion.identity);
            goVoid.name = "void_0";
        }
        else
        {
            // voids are tracked by their centroids - check this void against the previous list of voids
            // see if void already exists ( defined by threshold to previous centroid )

            // re-assign void IDs that are still active:
            voidAlreadyExists = false;
            previousVoidIds = new List<int>();
            currentVoidEntryIds = new List<int>();

            foreach( VoidAllInfo voidEntryPrev in voidsAllInfoPrev )
            {
                // Debug.Log("dist from prev voidID " + voidEntryPrev.id + " = " + Vector3.Distance(thisVoidCentroid, voidEntryPrev.centroid));
                if ( Vector3.Distance(thisVoidCentroid, voidEntryPrev.centroid) <= voidIDCentroidThreshold )
                {
                    thisVoidId = voidEntryPrev.id;
                    // Debug.Log("same void " + thisVoidId);
                    voidAlreadyExists = true;
                    break;
                }
            }

            // assign new void ids, spawn new void:
            if( voidAlreadyExists == false )
            {
                id_alreadyExists = true;
                id_assignmentCounter = 0;

                foreach( VoidAllInfo voidEntryPrev in voidsAllInfoPrev )
                {
                    previousVoidIds.Add( voidEntryPrev.id );
                }

                while( id_alreadyExists == true )
                {
                    id_alreadyExists = previousVoidIds.Contains( id_assignmentCounter );

                    if( id_alreadyExists == true )
                    {
                        id_assignmentCounter++;
                    }
                }
                thisVoidId = id_assignmentCounter;
                // Debug.Log("new void " + thisVoidId);
                GameObject goVoid = Instantiate(voidMeshPrefab, new Vector3(0, .01f, 0), Quaternion.identity);
                goVoid.name = "void_" + thisVoidId;
            }
        }
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

        if( voidsAllInfo != null )
        {
            foreach( VoidAllInfo voidEntry in voidsAllInfo )
            {
                for (int i = 0; i < voidEntry.triangles.Count; i++)
                {
                    if(voidEntry.triangles[i].isAVoidLargestTriangle == true)
                    {
                        Gizmos.color = Color.yellow;
                    }
                    else if( voidEntry.triangles[i].isAVoidSmallerTriangle == true)
                    {
                        Gizmos.color = Color.clear;
                    }
                    Vector2 pt0 = voidEntry.triangles[i].sites[0].Coord;
                    Vector2 pt1 = voidEntry.triangles[i].sites[1].Coord;
                    Vector2 pt2 = voidEntry.triangles[i].sites[2].Coord;

                    Vector3 point0 = new Vector3(pt0.x, 0, pt0.y);
                    Vector3 point1 = new Vector3(pt1.x, 0, pt1.y);
                    Vector3 point2 = new Vector3(pt2.x, 0, pt2.y);
                    Gizmos.DrawLine(point0, point1);
                    Gizmos.DrawLine(point1, point2);
                    Gizmos.DrawLine(point2, point0);
                }
            }
        }

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
        }

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
            Gizmos.color = Color.blue;

            Vector2 pt0 = voidLargestTrianglePartner_ofSelectedGroup.sites[0].Coord;
            Vector2 pt1 = voidLargestTrianglePartner_ofSelectedGroup.sites[1].Coord;
            Vector2 pt2 = voidLargestTrianglePartner_ofSelectedGroup.sites[2].Coord;

            Vector3 point0 = new Vector3(pt0.x, 0, pt0.y);
            Vector3 point1 = new Vector3(pt1.x, 0, pt1.y);
            Vector3 point2 = new Vector3(pt2.x, 0, pt2.y);
            Gizmos.DrawLine(point0, point1);
            Gizmos.DrawLine(point1, point2);
            Gizmos.DrawLine(point2, point0);
        }
    }

}
