using System.Collections;

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleSlice : MonoBehaviour
{
    //This will handle the mesh generation of the inside of the destructible object. 

    private MeshFilter meshFilter;
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uv;
    private Vector2[] uv2;
    private Vector3[] normals;

    private Vector3 fwd;
    private Vector3 up;
    private Vector3 right;
    private Vector3 contact;

    public bool tested = false;

    public bool isConvex = false;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;
        vertices = mesh.vertices;
        triangles = mesh.triangles;
        uv = mesh.uv;
        uv2 = mesh.uv2;
        normals = mesh.normals;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (!tested)
        {
            //Get the current number of vertices, so we don't have to deal with comparing floats to zero
            fwd = transform.InverseTransformDirection(collision.transform.forward);
            up = transform.InverseTransformDirection(collision.transform.up);
            right = transform.InverseTransformDirection(collision.transform.right);
            contact = transform.InverseTransformPoint(collision.GetContact(0).point);

            int numVerts = vertices.Length;
            List<Vector3> NewPoints = Bisect(contact, right);
            int[] mask = GetMask(contact, right, numVerts);
            int numPointsAdded = NewPoints.Count;
            CloneAndCut(mask, NewPoints);
        }
    }

    private List<Vector3> Bisect(Vector3 pos, Vector3 normal)
    {
        HashSet<Tuple<int, int>> intersectingEdges = new HashSet<Tuple<int, int>>();
        Dictionary<int, List<Tuple<int, int>>> intersectingTriangles = new Dictionary<int, List<Tuple<int, int>>>(); //Associates intersected edges with the triangle;
        Dictionary<Tuple<int, int>, int> intersectingVertices = new Dictionary<Tuple<int, int>, int>(); //Associates the intersecting vertex with the edge

        Dictionary<int, int> bisectedTriangles = new Dictionary<int, int>(); //Stores triangles that are bisected by point & edge. Key is tri number, val is 0/1/2 to denote the point intersected

        List<Vector3> AddedVertices = new List<Vector3>();

        //Check which edges intersect the slice plane, add them to the appropriate data structures
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 pt;
            if (EdgeAndPointIntersection(pos, normal, i, out pt))
            {
                //Add stuff to a new container to process later
                int val = -1;
                if (pt == vertices[triangles[i]]) val = 0;
                else if (pt == vertices[triangles[i + 1]]) val = 1;
                else if (pt == vertices[triangles[i + 2]]) val = 2;
                else Debug.Log("Error! Bisected triangle error");
                Tuple<int, int> edge = SortedTuple(triangles[i + 1], triangles[i + 2]);
                intersectingEdges.Add(edge);
                bisectedTriangles.Add(i, val);
            }
            else
            {
                if (DoesEdgeIntersect(pos, normal, vertices[triangles[i]], vertices[triangles[i + 1]]))
                {
                    Tuple<int, int> edge = SortedTuple(triangles[i], triangles[i + 1]);
                    intersectingEdges.Add(edge);
                    intersectingTriangles.Add(i, new List<Tuple<int, int>> { edge });
                }
                if (DoesEdgeIntersect(pos, normal, vertices[triangles[i]], vertices[triangles[i + 2]]))
                {
                    Tuple<int, int> edge = SortedTuple(triangles[i], triangles[i + 2]);
                    intersectingEdges.Add(edge);

                    if (intersectingTriangles.ContainsKey(i))
                    {
                        intersectingTriangles[i].Add(edge);
                    }
                    else intersectingTriangles.Add(i, new List<Tuple<int, int>> { edge });
                }
                if (DoesEdgeIntersect(pos, normal, vertices[triangles[i + 1]], vertices[triangles[i + 2]]))
                {
                    Tuple<int, int> edge = SortedTuple(triangles[i + 1], triangles[i + 2]);
                    intersectingEdges.Add(edge);
                    intersectingTriangles[i].Add(edge);
                }
            }
        }
        //Containers for the new mesh data:
        List<Vector3> newVertices = new List<Vector3>(vertices);
        List<Vector2> newUv = new List<Vector2>(uv);
        List<Vector2> newUv2 = new List<Vector2>(uv2);
        List<Vector3> newNormals = new List<Vector3>(normals);

        int[] newTris = new int[triangles.Length + (intersectingTriangles.Count * 2 * 3) + (bisectedTriangles.Count * 3)];
        Array.Copy(triangles, newTris, triangles.Length);

        int j = vertices.Length;

        //Add a new vertex and associate it in the intersectingVertex dictionary
        foreach (Tuple<int, int> t in intersectingEdges)
        {
            //Add a new vertex and associate it in the intersectingVertex dictionary
            Vector3 newPoint = IntersectionPoint(pos, normal, vertices[t.Item1], vertices[t.Item2]);
            AddedVertices.Add(newPoint);
            newVertices.Add(newPoint);
            newUv.Add(CalculateUV(vertices[t.Item1], vertices[t.Item2], newVertices[j], uv[t.Item1], uv[t.Item2]));
            newUv2.Add(CalculateUV(vertices[t.Item1], vertices[t.Item2], newVertices[j], uv2[t.Item1], uv2[t.Item2]));
            newNormals.Add(CalculateNormal(vertices[t.Item1], vertices[t.Item2], newVertices[j], normals[t.Item1], normals[t.Item2]));

            intersectingVertices.Add(t, j);
            j++;
        }

        int count = triangles.Length;

        //Redraw the triangles
        foreach (KeyValuePair<int, List<Tuple<int, int>>> kvp in intersectingTriangles)
        {
            //Start with the first point in the triangle and go clockwise. If 1 > 2 is in the list, 
            if (kvp.Value.Contains(SortedTuple(triangles[kvp.Key], triangles[kvp.Key + 1])))
            {
                if (kvp.Value.Contains(SortedTuple(triangles[kvp.Key + 1], triangles[kvp.Key + 2])))
                {
                    //Numbers are refering to the index position in newVertices

                    int one = triangles[kvp.Key];
                    int two = triangles[kvp.Key + 1];
                    int three = triangles[kvp.Key + 2];
                    int four = intersectingVertices[SortedTuple(triangles[kvp.Key], triangles[kvp.Key + 1])];
                    int five = intersectingVertices[SortedTuple(triangles[kvp.Key + 1], triangles[kvp.Key + 2])];

                    newTris[kvp.Key] = four;
                    newTris[kvp.Key + 1] = two;
                    newTris[kvp.Key + 2] = five;

                    int[] tempArray = { one, four, three, three, four, five };
                    Array.Copy(tempArray, 0, newTris, count, 6);
                    count += 6;
                } //Draw triangles at 4, 2, 5; {1, 4, 3; 3, 4, 5}
                else
                {
                    int one = triangles[kvp.Key];
                    int two = triangles[kvp.Key + 1];
                    int three = triangles[kvp.Key + 2];

                    int four = intersectingVertices[SortedTuple(triangles[kvp.Key], triangles[kvp.Key + 1])];
                    int five = intersectingVertices[SortedTuple(triangles[kvp.Key], triangles[kvp.Key + 2])];

                    newTris[kvp.Key] = one;
                    newTris[kvp.Key + 1] = four;
                    newTris[kvp.Key + 2] = five;

                    int[] tempArray = { four, two, three, five, four, three };
                    Array.Copy(tempArray, 0, newTris, count, 6);
                    count += 6;
                } //Draw triangles at 1, 4, 5; { 4, 2, 3; 5, 4, 3}
            }
            else
            {

                int one = triangles[kvp.Key];
                int two = triangles[kvp.Key + 1];
                int three = triangles[kvp.Key + 2];
                int four = intersectingVertices[SortedTuple(triangles[kvp.Key + 1], triangles[kvp.Key + 2])];
                int five = intersectingVertices[SortedTuple(triangles[kvp.Key], triangles[kvp.Key + 2])];

                newTris[kvp.Key] = three;
                newTris[kvp.Key + 1] = five;
                newTris[kvp.Key + 2] = four;

                int[] tempArray = { one, two, four, one, four, five };
                Array.Copy(tempArray, 0, newTris, count, 6);
                count += 6;
            } //Draw triangles at 3, 5, 4; { 1, 2, 4; 1, 4, 5}
        }
        foreach (KeyValuePair<int, int> kvp in bisectedTriangles)
        {
            //Each entry in the dict represents a triangle that's bisected, and the point. Edge is always going to be the opposing edge to the point in the triangle
            //So what do we need to do. We have to draw two new triangles, and add a single new point. 

            int four;

            if (kvp.Value == 0)
            {
                four = intersectingVertices[SortedTuple(triangles[kvp.Key + 1], triangles[kvp.Key + 2])];

            }
            else if (kvp.Value == 1)
            {
                four = intersectingVertices[SortedTuple(triangles[kvp.Key], triangles[kvp.Key + 2])];
            }
            else if (kvp.Value == 2)
            {
                four = intersectingVertices[SortedTuple(triangles[kvp.Key], triangles[kvp.Key + 1])];
            }
            else
            {
                Debug.Log("Error: Value not set correctly!");
                four = -1;
            }

            int one = triangles[kvp.Key];
            int two = triangles[kvp.Key + 1];
            int three = triangles[kvp.Key + 2];

            switch (kvp.Value)
            {
                case 0:
                    //1,2,4; 1, 4, 3
                    newTris[kvp.Key + 2] = four;
                    int[] zeroCase = { one, four, three };
                    Array.Copy(zeroCase, 0, newTris, count, 3);
                    count += 3;
                    break;
                case 1:
                    //1, 2, 4; 4, 2, 3
                    newTris[kvp.Key + 2] = four;
                    int[] oneCase = { four, two, three };
                    Array.Copy(oneCase, 0, newTris, count, 3);
                    count += 3;
                    break;
                case 2:
                    //1, 4, 3; 4, 2, 3
                    newTris[kvp.Key + 1] = four;
                    newTris[kvp.Key + 2] = three;
                    int[] twoCase = { four, two, three };
                    Array.Copy(twoCase, 0, newTris, count, 3);
                    count += 3;
                    break;
            }
        }

        mesh.Clear();
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTris;
        mesh.uv = newUv.ToArray();
        mesh.uv2 = newUv2.ToArray();
        mesh.normals = newNormals.ToArray();

        RecalculateAndResetContainers();

        return AddedVertices;
    }

    private bool EdgeAndPointIntersection(Vector3 pos, Vector3 normal, int tri, out Vector3 pointIntersected)
    {
        //Check if a point is intersected, within the margin of error

        pointIntersected = Vector3.zero;
        if (PointInTriangle(out pointIntersected, pos, normal, tri))
        {
            if (IntersectingEdgeWithoutPoint(pos, normal, tri, pointIntersected))
            {
                return true;
            }
        }
        return false;

        //Check if an edge is intersected
    }

    private bool IntersectingEdgeWithoutPoint(Vector3 pos, Vector3 normal, int tri, Vector3 without)
    {
        if (vertices[triangles[tri]] == without)
        {
            if (DoesEdgeIntersect(pos, normal, vertices[triangles[tri + 1]], vertices[triangles[tri + 2]]))
            {
                return true;
            }
        }
        else if (vertices[triangles[tri + 1]] == without)
        {
            if (DoesEdgeIntersect(pos, normal, vertices[triangles[tri]], vertices[triangles[tri + 2]]))
            {
                return true;
            }
            return false;
        }
        else if (vertices[triangles[tri + 2]] == without)
        {
            if (DoesEdgeIntersect(pos, normal, vertices[triangles[tri + 1]], vertices[triangles[tri]]))
            {
                return true;
            }
            return false;
        }

        return false;
    }

    private bool PointInTriangle(out Vector3 v, Vector3 pos, Vector3 normal, int tri)
    {
        //Checks if the plane intersects any of the points, within a given margin of error. Returns true if found, and assigns the intersecting vertex to the out parameter.
        float marginOfError = 0.00001f;
        if (Mathf.Abs(DistanceFromPlane(vertices[triangles[tri]], pos, normal)) < marginOfError)
        {
            v = vertices[triangles[tri]];
            return true;
        }
        else if (Mathf.Abs(DistanceFromPlane(vertices[triangles[tri + 1]], pos, normal)) < marginOfError)
        {
            v = vertices[triangles[tri + 1]];
            return true;
        }
        else if (Mathf.Abs(DistanceFromPlane(vertices[triangles[tri + 2]], pos, normal)) < marginOfError)
        {
            v = vertices[triangles[tri + 2]];
            return true;
        }
        else
        {
            v = Vector3.zero;
            return false;
        }
    }

    private GameObject CloneAndCut(int[] mask, List<Vector3> AddedPoints)
    {
        GameObject clone = Instantiate(gameObject, transform.parent);
        DestructibleSlice cloneData = clone.GetComponent<DestructibleSlice>();

        Cut(mask, false);
        cloneData.Cut(mask, true);
        cloneData.tested = true;
        tested = true;
        Cap(AddedPoints);


        return clone;
    }


    private void Cap(List<Vector3> AddedPoints)
    {
        Vector3ApproximateComparer Approx = new Vector3ApproximateComparer();
        HashSet<Vector3> UniquePoints = new HashSet<Vector3>(AddedPoints, Approx);
        Dictionary<Vector3, Vector3> edgeList = new Dictionary<Vector3, Vector3>(Approx);

        //Creates the edge list defining the polygon from mesh triangles
        //There's redundant edges that are being added, which creates an infinite loop
        for (int i = 0; i < triangles.Length; i += 3)
        {
            if (UniquePoints.Contains(vertices[triangles[i]]) && UniquePoints.Contains(vertices[triangles[i + 1]]))
            {
                if (edgeList.ContainsKey(vertices[triangles[i + 1]]))
                {
                    Debug.Log("Found duplicate");
                }
                else if (vertices[triangles[i + 1]] == vertices[triangles[i]])
                {
                    //Debug.Log("Edge Loop");
                }
                else
                {
                    edgeList[vertices[triangles[i + 1]]] = vertices[triangles[i]];
                }
            }
            if (UniquePoints.Contains(vertices[triangles[i + 1]]) && UniquePoints.Contains(vertices[triangles[i + 2]]))
            {
                if (edgeList.ContainsKey(vertices[triangles[i + 2]]))
                {
                    Debug.Log("Found duplicate");
                }
                else if (vertices[triangles[i + 1]] == vertices[triangles[i + 2]])
                {
                    //Debug.Log("Edge Loop");
                }
                else
                {
                    edgeList[vertices[triangles[i + 2]]] = vertices[triangles[i + 1]];
                }
            }
            if (UniquePoints.Contains(vertices[triangles[i]]) && UniquePoints.Contains(vertices[triangles[i + 2]]))
            {
                if (edgeList.ContainsKey(vertices[triangles[i]]))
                {
                    Debug.Log("Found duplicate");
                }
                else if (vertices[triangles[i]] == vertices[triangles[i + 2]])
                {
                    //Debug.Log("Edge Loop");
                }
                else
                {
                    edgeList[vertices[triangles[i]]] = vertices[triangles[i + 2]];
                }
            }
        }


        /* The below is verification that we have a closed loop
        Debug.Log(edgeList.Count);
        int count = 0;
        Vector3 start = edgeList.Keys.First();
        Vector3 current = start;
        while (true)
        {
            count++;
            Vector3 next = edgeList[current];
            Debug.Log(current.ToString("F5") + " to " + next.ToString("F5"));
            Debug.Log("Count: " + count);
            if (next == start)
            {
                Debug.Log("Success!!");
                break;
            }
            else current = next;
            if (count > 500)
            {
                Debug.Log("Over 500!");
                break;
            }
        }

        Debug.Log("Edges traversed: " + count);
        */

        //Now we need to project the points onto the slice plane, to normalize the X/Y values 

        List<Vector3> newTriangles;
        if (isConvex)
        {
            newTriangles = Triangulate.TriangulateConvexPolygon(UniquePoints, edgeList, Approx);
        }
        else
        {
            Dictionary<Vector3, Vector3> Projection = new Dictionary<Vector3, Vector3>(Approx);
            HashSet<Vector3> ProjectedPoints = new HashSet<Vector3>(Approx);
            foreach (Vector3 pt in UniquePoints)
            {
                Vector3 temp = Project(pt, contact, right, up, fwd);
                ProjectedPoints.Add(temp);
                Projection[pt] = temp;

            }
            //Dictionary is created, now we need to translate the edge list 

            Dictionary<Vector3, Vector3> ProjectedEdges = new Dictionary<Vector3, Vector3>(Approx);
            foreach (KeyValuePair<Vector3, Vector3> kvp in edgeList)
            {
                ProjectedEdges[Projection[kvp.Key]] = Projection[kvp.Value];
            }
            //This will need to convert from projected points back to regular points using the above dictionary
            newTriangles = Triangulate.TriangulatePolygon(ProjectedPoints, ProjectedEdges, Approx);
        }

        //New triangles is a list of the vectors in the proper edge order. Iterate through the list of triangles and pair each one to the resulting index
        Dictionary<Vector3, int> indexDict = new Dictionary<Vector3, int>(Approx);
        List<Vector3> vertList = new List<Vector3>(vertices);
        List<Vector3> points = new List<Vector3>(UniquePoints);
        for (int i = 0; i < points.Count; i++)
        {
            indexDict[points[i]] = vertList.FindIndex(x => x == newTriangles[i]);
        }

        List<int> newTris = new List<int>(triangles);
        for (int i = 0; i < newTriangles.Count; i++)
        {
            newTris.Add(indexDict[newTriangles[i]]);
        }

        triangles = newTris.ToArray();
        mesh.triangles = triangles;

        RecalculateAndResetContainers();

    }


    private int SortByMagnitude(Vector3 a, Vector3 b)
    {
        if (a.magnitude > b.magnitude)
        {
            return 1;
        }
        else if (a.magnitude < b.magnitude)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }

    private void Cut(int[] mask, bool clone)
    {
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector2> newUv = new List<Vector2>();
        List<Vector2> newUv2 = new List<Vector2>();
        List<Vector3> newNormals = new List<Vector3>();
        Dictionary<int, int> vertexMap = new Dictionary<int, int>();

        if (clone)
        {
            for (int i = 0; i < vertices.Length; i++)
                if (mask[i] != 1)
                {
                    newVertices.Add(vertices[i]);
                    newUv.Add(uv[i]);
                    newUv2.Add(uv2[i]);
                    newNormals.Add(normals[i]);
                    vertexMap.Add(i, newVertices.Count - 1);
                }
        }
        else
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                if (mask[i] != -1)
                {
                    newVertices.Add(vertices[i]);
                    newUv.Add(uv[i]);
                    newUv2.Add(uv2[i]);
                    newNormals.Add(normals[i]);
                    vertexMap.Add(i, newVertices.Count - 1);
                }
            }
        }

        //Now we iterate through the triangles to redraw triangles;
        List<int> newTriangles = new List<int>();

        for (int i = 0; i < triangles.Length; i += 3)
        {
            if (vertexMap.ContainsKey(triangles[i]) && vertexMap.ContainsKey(triangles[i + 1]) && vertexMap.ContainsKey(triangles[i + 2]))
            {
                newTriangles.Add(vertexMap[triangles[i]]);
                newTriangles.Add(vertexMap[triangles[i + 1]]);
                newTriangles.Add(vertexMap[triangles[i + 2]]);
            }
        }

        mesh.Clear();

        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
        mesh.uv = newUv.ToArray();
        mesh.uv2 = newUv2.ToArray();
        mesh.normals = newNormals.ToArray();

        RecalculateAndResetContainers();
    }


    private int[] GetMask(Vector3 origin, Vector3 normal, int num)
    {
        //Returns an array mask noting which vertices are above, below, or on the slice plane
        int[] mask = new int[vertices.Length];

        for (int i = 0; i < num; i++)
        {
            if (DistanceFromPlane(vertices[i], origin, normal) > 0.00001f) 
            {
                mask[i] = 1;
            }
            else if (DistanceFromPlane(vertices[i], origin, normal) < -0.00001f)
            {
                mask[i] = -1;
            }
            else
            {
                mask[i] = 0;
            }
        }

        for (int j = num; j < vertices.Length; j++)
        {
            mask[j] = 0;
        }
        return mask;
    }

    public Vector2 CalculateUV(Vector3 a, Vector3 b, Vector3 c, Vector2 uva, Vector2 uvb)
    {
        float LerpPoint = Vector3.Distance(a,c)/Vector3.Distance(a,b);
        return Vector2.Lerp(uva, uvb, LerpPoint);
    }

    public Vector3 CalculateNormal(Vector3 a, Vector3 b, Vector3 c, Vector3 normalA, Vector3 normalB)
    {
        float LerpPoint = Vector3.Distance(a, c) / Vector3.Distance(a, b);
        return Vector3.Lerp(normalA, normalB, LerpPoint);
    }

    private Tuple<int,int> SortedTuple(int a, int b)
    {
        return a > b ? new Tuple<int, int>(b, a) : new Tuple<int, int>(a, b);
    }

    private Vector3 IntersectionPoint(Vector3 origin, Vector3 normal, Vector3 p0, Vector3 p1)
    {
        Vector3 u = p1 - p0;
        Vector3 w = p0 - origin;
        float s = Vector3.Dot(-normal, w) / Vector3.Dot(normal, u);
        return p0 + s * u;
    }

    private bool DoesEdgeIntersect(Vector3 origin, Vector3 normal, Vector3 a, Vector3 b)
    {
        //Takes two points and tests whether or not it intersects with a plane defined by an origin point and a normal
        if ( (DistanceFromPlane(a, origin, normal) * DistanceFromPlane(b, origin, normal)) > 0)
        {
            return false;
        }
        return true;
    } 

    private Vector3 Project(Vector3 point, Vector3 origin, Vector3 normal, Vector3 xAxis, Vector3 yAxis)
    {
        return new Vector3(Vector3.Dot(xAxis, point - origin), Vector3.Dot(yAxis, point - origin), Vector3.Dot(normal, point - origin));
    }

    private static float DistanceFromPlane(Vector3 point, Vector3 origin, Vector3 normal)
    {
        return Vector3.Dot(normal, point - origin);
    }
    
    private void RecalculateAndResetContainers()
    {
        vertices = mesh.vertices;
        triangles = mesh.triangles;
        uv = mesh.uv;
        uv2 = mesh.uv2;
        normals = mesh.normals;

        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
    }


    class Vector3ApproximateComparer : IEqualityComparer<Vector3>
    {
        public bool Equals(Vector3 a, Vector3 b)
        {
            if (a == b) return true;
            else return false;
        }
        public int GetHashCode(Vector3 a)
        {
            float hCode = Mathf.Pow(Mathf.Pow(a.x, a.y), a.z);
            return hCode.GetHashCode();
        }
    }
}


