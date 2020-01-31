using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Delaunay;
using Delaunay.Geo;

public class VoidMesh : MonoBehaviour 
{
    // editing tools for multiple prefabs:
    public bool debug = false;
    private GameObject[] selectedGOs;
    // and then PLACE ALL THIS in a method:
    /*
        selectedGo = UnityEditor.Selection.activeGameObject;
        if( selectedGo != null)
        {
            if (selectedGo.name == this.transform.name)
            { debug = true; }
            else
            { debug = false; }
            if (debug == true)
            { Debug.Log(); }
        }
    */
    private List<Vector2> selectedGOs_XZ;
    int goPointTest;

    GameObject[] otherVoids;
    int otherVoidsSelfIdCounter;
    public DelaunayTriangulation delaunayScript;

    GameObject delaunayTriangulation;
    List<VoidAllInfo> voidsAllInfo;
    string parentName;
    public int self_id;
    VoidAllInfo self_info;
    bool self_still_exists;

    Mesh voidMesh;
    Material voidMaterial;
    private Vector3[] voidMesh_Vertices;
    private int[] voidMesh_Triangles;

    Color colorStart;
    Color colorEnd;
    Color colorClosed = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    Color colorOpen = new Color(0.0f, 0.0f, 0.0f, 1.0f);

    private float trackingAreaThresh;
    private float xfadeAreaThresh;
    private float xfadeMaxAreaThresh;
    private float xfadeTime;
    private float xfadeStartTime;
    private float xfadeTotalTime = 2.5f;
    private float voidXfadePhase = -1;
    private bool startOpening = true;
    private bool isOpening;
    private bool startClosing = false;
    private bool isClosing;
    private bool dieAPeacefulDeath = false;

    int testNumLargest;
    int testNumSmaller;

    void Awake()
    {
        voidMesh = this.GetComponent<MeshFilter>().mesh;
        voidMaterial = this.GetComponent<Renderer>().material;
        voidMaterial.color = colorClosed;
    }

    void Start()
    {
        delaunayTriangulation = GameObject.Find("delaunayTriangulation");
        delaunayScript = GameObject.Find("delaunayTriangulation").GetComponent<DelaunayTriangulation>();

        trackingAreaThresh = delaunayTriangulation.GetComponent<DelaunayTriangulation>().voidTrackingAreaThresh;
        xfadeAreaThresh = delaunayTriangulation.GetComponent<DelaunayTriangulation>().voidXfadeAreaThresh;

        GetSelfID();

        // CROSSCHECK destroy if go already exists
        otherVoidsSelfIdCounter = 0;
        otherVoids = GameObject.FindGameObjectsWithTag("void");
        foreach (GameObject otherVoid in otherVoids)
        {
            if( self_id == otherVoid.GetComponent<VoidMesh>().self_id )
            {
                otherVoidsSelfIdCounter++;
            }
            if( otherVoidsSelfIdCounter > 1)
            {
                Destroy(gameObject);
            }
        }
    }

	void Update () 
    {
        voidsAllInfo = delaunayTriangulation.GetComponent<DelaunayTriangulation>().voidsAllInfo;
        // voidsAllInfo = delaunayScript.voidsAllInfo;

        // CROSSCHECK       SYNC CHECK - destroy if I still exist, but there is no corresponding entry in voidsAllInfo
        // there is another SYNC CHECK in the Delaunay script
        foreach ( VoidAllInfo thisVoid in voidsAllInfo )
        {
            if( thisVoid.id == self_id )
            {
                self_info = thisVoid;
                self_still_exists = true;
                break;
            }
            else
            {
                self_still_exists = false;
            }
        }

        // CROSSCHECK destroy if I still exist, but list is empty
        if ( self_still_exists == false || voidsAllInfo.Count == 0 )
        {
            // Debug.Log(self_id + " has died");
            Destroy(gameObject);
        }

        // else still exists, and do shit...
        else
        {
            // opening and closing the void...
            // opening...
            if (self_info.area >= xfadeAreaThresh && startOpening == true)
            {
                startOpening = false;
                isOpening = true;
                voidXfadePhase = 0;
                xfadeStartTime = Time.time;

                colorStart = colorClosed;
                colorEnd = colorOpen;
            }

            // closing...
            if (self_info.area < xfadeAreaThresh && isOpening == false && startOpening == false)
            {
                startClosing = true;
            }

            if (startClosing == true)
            {
                isClosing = true;
                startClosing = false;
                startOpening = true;
                voidXfadePhase = 0;
                xfadeStartTime = Time.time;

                colorStart = colorOpen;
                colorEnd = colorClosed;
            }

            // xfading for opening and closing...
            if( voidXfadePhase != -1 )
            {
                if (isOpening == true)
                {
                    voidXfadePhase = (Time.time - xfadeStartTime) / xfadeTotalTime;
                    voidMaterial.color = Color.Lerp(colorStart, colorEnd, voidXfadePhase);
                    if (voidXfadePhase >= 1.0f)
                    {
                        isOpening = false;
                        voidXfadePhase = -1;
                    }
                }
                else if (isClosing == true)
                {
                    voidXfadePhase = (Time.time - xfadeStartTime) / xfadeTotalTime;
                    voidMaterial.color = Color.Lerp(colorStart, colorEnd, voidXfadePhase);
                    if (voidXfadePhase >= 1.0f)
                    {
                        isClosing = false;
                        dieAPeacefulDeath = true;
                    }
                }
            }

            /*
            else if( dieAPeacefulDeath == true )
            {
                Debug.Log(self_id + " has died cuz below area thresh ");
                Destroy(gameObject);
            }
            */

            // draw the mesh...
            voidMesh.Clear();

            voidMesh_Vertices = new Vector3[self_info.triangles.Count * 3];
            voidMesh_Triangles = new int[self_info.triangles.Count * 3];

            for (int i = 0; i < self_info.triangles.Count; i++)
            {
                int index1 =  i * 3;
                int index2 = (i * 3) + 1;
                int index3 = (i * 3) + 2;

                voidMesh_Vertices[index1] = new Vector3(self_info.triangles[i].sites[0].Coord.x, 0, self_info.triangles[i].sites[0].Coord.y);
                voidMesh_Vertices[index2] = new Vector3(self_info.triangles[i].sites[1].Coord.x, 0, self_info.triangles[i].sites[1].Coord.y);
                voidMesh_Vertices[index3] = new Vector3(self_info.triangles[i].sites[2].Coord.x, 0, self_info.triangles[i].sites[2].Coord.y);

                voidMesh_Triangles[index1] = index1;
                voidMesh_Triangles[index2] = index2;
                voidMesh_Triangles[index3] = index3;

            }

            voidMesh.vertices = voidMesh_Vertices;
            voidMesh.triangles = voidMesh_Triangles;
            voidMesh.RecalculateNormals();

            ScaleAround( this.gameObject, self_info.centroid, new Vector3(.8f, .8f, .8f) );
        }
    }

    void GetSelfID()
    {
        string[] splitName = new string[2];
        splitName = name.Split('_');
        string str_id = splitName[1];
        self_id = int.Parse(str_id);
    }

    public void ScaleAround(GameObject target, Vector3 pivot, Vector3 newScale)
    {
        Vector3 A = target.transform.localPosition;
        Vector3 B = pivot;

        Vector3 C = A - B; // diff from object pivot to desired pivot/origin

        float RS = newScale.x / target.transform.localScale.x; // relataive scale factor

        // calc final position post-scale
        Vector3 FP = B + C * RS;

        // finally, actually perform the scale/translation
        target.transform.localScale = newScale;
        target.transform.localPosition = FP;
    }
}
