using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangulate
{

    private static Vector3 LowerBound = new Vector3(Mathf.NegativeInfinity, Mathf.NegativeInfinity, Mathf.NegativeInfinity);

    enum VertexType
    {
        start,
        merge, 
        split,
        end,
        regular
    }

    public static List<Vector3> TriangulateConvexPolygon(HashSet<Vector3> points, Dictionary<Vector3, Vector3> next, IEqualityComparer<Vector3> equalityComparer)
    {
        List<Vector3> triangles = new List<Vector3>();
        Vector3 first = points.First();
        Vector3 current = next[first];


        while (true)
        {
            Debug.Log("*******");
            Debug.Log(first.ToString("F5"));
            Debug.Log(current.ToString("F5"));
            triangles.Add(first);
            triangles.Add(current);
            current = next[current];
            triangles.Add(current);
            Debug.Log(current.ToString("F5"));
            Debug.Log("________");

            if (current == first)
            {
                break;
            }
        }
        return triangles;
    }

    public static List<Vector3> TriangulatePolygon(HashSet<Vector3> points, Dictionary<Vector3, Vector3> next, IEqualityComparer<Vector3> equalityComparer)
    {
        //Takes a set of unique points, and a list of edges as a dictionary, and returns a dictionary of edges defining the triangulation of the polygon given

        //Using the algorithm from de Berg et al

        //Start vertex: Neighbors are below and interior angle is < pi
        //Split vertex: Neighbors are below and interior angle is > pi
        //Merge vertex: Neighbors are above and interior angle is > pi
        //End vertex: Neighbors are above and interior angle is < pi
        //Regular vertex: Neighbors are not homogeneously above or below

        //Sort the unique points 
        List<Vector3> PointsByY = new List<Vector3>(points);
        PointsByY.Sort(SortByY);

        //Pass it to MakeMonotone
        List<Tuple<Vector3, Vector3>> AddedDiagonals = MakeMonotone(PointsByY, next);

        for (int i = 0; i < AddedDiagonals.Count; i++)
        {
            Debug.Log("Added diagonal from " + AddedDiagonals[i].Item1 + " to " + AddedDiagonals[i].Item2);
        }

        return new List<Vector3>();
    }

    private static List<Tuple<Vector3, Vector3>> MakeMonotone(List<Vector3> points, Dictionary<Vector3, Vector3> next)
    {
        //V_i is points[i], E_i is also points[i]. E_i+1 is next[points[i]], E_i-1 is prev[points[i]]

        Dictionary<Vector3, Vector3> helper = new Dictionary<Vector3, Vector3>(next.Comparer);
        Dictionary<Vector3, Vector3> prev = new Dictionary<Vector3, Vector3> (next.ToDictionary(x => x.Value, x => x.Key), next.Comparer);

        List<Tuple<Vector3, Vector3>> AddedDiagonals = new List<Tuple<Vector3, Vector3>>();

        SortedSet<Vector3> CurrentEdges = new SortedSet<Vector3>(new CompareX());

        for (int i = 0; i < points.Count; i++)
        {
            Debug.Log("*** New Vertex ***");
            Debug.Log("Vertex: " + points[i].ToString("F5"));
            switch ( GetVertexType(points[i], prev[points[i]], next[points[i]]))
            {


                case VertexType.start:
                    Debug.Log("Start");
                    CurrentEdges.Add(points[i]);
                    helper[points[i]] = points[i];
                    break;

                case VertexType.end:
                    //if helper(e_(i-1)) is a merge vertex
                    Debug.Log("End");
                    Vector3 temp = helper[prev[points[i]]];
                    if (GetVertexType(temp, prev[temp], next[temp]) == VertexType.merge)
                    {
                        //Insert diagonal connecting v_i to temp 
                        AddedDiagonals.Add(new Tuple<Vector3,Vector3>(points[i], temp));
                    }
                    if (!CurrentEdges.Remove(prev[points[i]])) Debug.Log("Didn't remove an edge!");
                    break;

                case VertexType.split:
                    Debug.Log("Split");
                    Vector3 leftEdge = GetLeftEdge(points[i], CurrentEdges);
                    AddedDiagonals.Add(new Tuple<Vector3,Vector3>(points[i], helper[leftEdge]));
                    helper[leftEdge] = points[i];
                    helper[points[i]] = points[i];
                    CurrentEdges.Add(points[i]);
                    break;

                case VertexType.merge:
                    Debug.Log("Merge");
                    temp = helper[prev[points[i]]];
                    if (GetVertexType(temp, prev[temp], next[temp]) == VertexType.merge)
                    {
                        AddedDiagonals.Add(new Tuple<Vector3, Vector3> (points[i], helper[prev[points[i]]]));
                    }
                    CurrentEdges.Remove(prev[points[i]]);
                    leftEdge = GetLeftEdge(points[i], CurrentEdges);
                    temp = helper[leftEdge];
                    if (GetVertexType(temp, prev[temp], next[temp]) == VertexType.merge)
                    {
                        AddedDiagonals.Add(new Tuple<Vector3, Vector3> (points[i], temp));
                    }
                    helper[leftEdge] = points[i];
                    break;
                    
                case VertexType.regular:

                    Debug.Log("Regular");

                    if (IsLeftSide(points[i], prev[points[i]], next[points[i]]))
                    {
                        temp = helper[prev[points[i]]];
                        if(GetVertexType(temp, prev[temp], next[temp]) == VertexType.merge)
                        {
                            AddedDiagonals.Add(new Tuple<Vector3, Vector3>(points[i], temp));
                        }
                        CurrentEdges.Remove(prev[points[i]]);
                        CurrentEdges.Add(points[i]);
                        helper[points[i]] = points[i];
                         
                    }
                    else
                    {
                        Debug.Log("Not left side regular point!");
                        Debug.Log("Point: " + points[i].ToString("F5") + ", Prev: " + prev[points[i]].ToString("F5") + ", Next: " + next[points[i]].ToString("F5"));
                        PrintDictionary(helper);
                        leftEdge = GetLeftEdge(points[i], CurrentEdges);
                        temp = helper[leftEdge];
                        if (GetVertexType(temp, prev[temp], next[temp]) == VertexType.merge)
                        {
                            AddedDiagonals.Add(new Tuple<Vector3, Vector3>(points[i], temp));
                        }
                        helper[leftEdge] = points[i];
                    }
                    break;
            }
        }
        return AddedDiagonals;
    }

    private static void PrintDictionary(Dictionary<Vector3, Vector3> dict)
    {
        Debug.Log("Printing Dictionary...");
        if (dict.Count == 0)
        {
            Debug.Log("Empty Dict!");
            return;
        }

        foreach (KeyValuePair<Vector3, Vector3> kvp in dict)
        {
            Debug.Log(kvp.Key.ToString("F5") + " to " + kvp.Value.ToString("F5"));
        }
        Debug.Log("...Dictionary End");
    }

    private static void PrintSortedSet(SortedSet<Vector3> set)
    {
        Debug.Log("Printing Current Edges...");
        foreach (Vector3 v in set)
        {
            Debug.Log(v.ToString("F5"));
        }
        Debug.Log("...Current edges end!");
    }

    private static bool IsLeftSide(Vector3 point, Vector3 prev, Vector3 next)
    {
        return prev.y > point.y ? true : false;
    }

    private static Vector3 GetLeftEdge(Vector3 point, SortedSet<Vector3> currentEdges)
    {
        SortedSet<Vector3> temp = new SortedSet<Vector3>(currentEdges.GetViewBetween(LowerBound, point), new CompareX());
        Debug.Log("{GetLeftEdge...");
        Debug.Log("X val of point: " + point.x.ToString("F5"));
        foreach (Vector3 i in currentEdges)
        {
            Debug.Log(i.x.ToString("F5"));
        }
        Debug.Log("...GetLeftEdge}");
        return temp.Max;
    }

    private static VertexType GetVertexType(Vector3 point, Vector3 prev, Vector3 next)
    {
        //Check if neighbors are both above or below
        if (prev.y > point.y && next.y > point.y)
        {
            //Check interior angle
            if (CheckInteriorAngle(point, prev, next) > Mathf.PI)
            {
                return VertexType.merge;
            }
            else
            {
                return VertexType.end;
            }
        }
        else if (prev.y < point.y && next.y < point.y)
        {
            //Check interior angle
            if (CheckInteriorAngle(point, prev, next) > Mathf.PI)
            {
                return VertexType.split;
            }
            else
            {
                return VertexType.start;
            }
        }
        return VertexType.regular;
    }

    private static float CheckInteriorAngle(Vector3 point, Vector3 prev, Vector3 next)
    {
        //Vector3.Angle, after we normalize prev/next to point
        return Mathf.Deg2Rad * Vector3.Angle(prev - point, next - point);
    }
    
    private static int SortByY(Vector3 a, Vector3 b)
    {
        //Descending y;
        if (a.y > b.y) return -1;
        else if (a.y < b.y) return 1;
        else if (a.x > b.x) return 1;
        else if (a.x < b.x) return -1;
        else if (a.z > b.z) return 1;
        else if (a.z < b.z) return -1;
        else return 0;
    }


    class CompareX : IComparer<Vector3>
    {
        int IComparer<Vector3>.Compare(Vector3 a, Vector3 b)
        {
            return a.x > b.x ? 1 : -1;
        }
    }
}

