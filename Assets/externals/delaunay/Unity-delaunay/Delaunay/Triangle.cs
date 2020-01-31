using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Delaunay.Utils;

namespace Delaunay
{
    
    public sealed class Triangle: IDisposable
    {
        private List<Site> _sites;
        public List<Site> sites {
            get { return this._sites; }
        }

        // ******ADDED******
        public List<float> edgeLengths;
        public float edgeLongest_length;
        public List<Site> edgeLongest_coords;
        public int voidId = -1;
        public bool isAVoidLargestTriangle;
        public bool isAVoidSmallerTriangle;
        public bool isAConvexHullTriangle;

        private int indexLinkLengthBackToCoords;

        public Triangle (Site a, Site b, Site c)
        {
            _sites = new List<Site> () { a, b, c };

            // ******ADDED******
            float length0 = Vector2.Distance( a.Coord, b.Coord );
            float length1 = Vector2.Distance( b.Coord, c.Coord );
            float length2 = Vector2.Distance( c.Coord, a.Coord );
            edgeLengths = new List<float>{ length0, length1, length2 };

            List<float> allEdges = new List<float>();
            allEdges.Add( edgeLengths[0] );
            allEdges.Add( edgeLengths[1] );
            allEdges.Add( edgeLengths[2] );
            edgeLongest_length = Mathf.Max( allEdges.ToArray() );

            for (int i = 0; i <= 2; i++)
            {
                if(edgeLengths[i] == edgeLongest_length)
                {
                    indexLinkLengthBackToCoords = i;
                }
            }

            if( indexLinkLengthBackToCoords == 0 )
            {
                edgeLongest_coords = new List<Site> { a, b };
            }
            else if (indexLinkLengthBackToCoords == 1)
            {
                edgeLongest_coords = new List<Site> { b, c };
            }
            else if (indexLinkLengthBackToCoords == 2)
            {
                edgeLongest_coords = new List<Site> { c, a };
            }

            isAVoidLargestTriangle = false;
            isAVoidSmallerTriangle = false;
            isAConvexHullTriangle = false;
        }
        
        public void Dispose ()
        {
            _sites.Clear ();
            _sites = null;
        }

    }
}