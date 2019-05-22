using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngotShape : MonoBehaviour {

    public float[] dimensions = new float[3];
    public float curvature;
    public GameObject sparks;
    

    private Mesh mesh;
    private MeshCollider meshCol;
    private Vector3 center = new Vector3(-1 - (2 / (2 * Mathf.PI)), 0, 0);
    private Vector3[] changeVertices;

    private Temperature tempInfo;

    private int pinchAxis;
    private int edge;
    private int tip;

    private List<int> x1Face = new List<int>();
    private List<int> y1Face = new List<int>();
    private List<int> z1Face = new List<int>();
    private List<int> x2Face = new List<int>();
    private List<int> y2Face = new List<int>();
    private List<int> z2Face = new List<int>();

    private bool grinding;

    private List<List<int>> faces = new List<List<int>>();

    private void Awake()
    {
        sparks = GameObject.Find("Sparks");
        mesh = GetComponent<MeshFilter>().mesh;
        tempInfo = GetComponent<Temperature>();
        meshCol = GetComponent<MeshCollider>();
        changeVertices = mesh.vertices;
    }

    void Start () {
        grinding = false;
        curvature = 0.0f;
        dimensions[0] = 1.0f;
        dimensions[1] = 1.0f;
        dimensions[2] = 1.0f;

        pinchAxis = 0;
        edge = 1;
        GetFaces();
        meshCol.sharedMesh = mesh;
        mesh.MarkDynamic();
	}

    private void GetFaces()
    {
        for (int i = 0; i < changeVertices.Length; i++)
        {
            if (changeVertices[i].x >= 0.99f)
            {
                x1Face.Add(i);
            }
            else if (changeVertices[i].x <= -0.99f)
            {
                x2Face.Add(i);
            }

            if (changeVertices[i].y >= 0.99f)
            {
                y1Face.Add(i);
            }
            else if (changeVertices[i].y <= -0.99f)
            {
                y2Face.Add(i);
            }

            if (changeVertices[i].z >= 0.99f)
            {
                z1Face.Add(i);
            }
            else if (changeVertices[i].z <= -0.99f)
            {
                z2Face.Add(i);
            }
        }

        faces.Add(x1Face);
        faces.Add(y1Face);
        faces.Add(z1Face);
        faces.Add(x2Face);
        faces.Add(y2Face);
        faces.Add(z2Face);
    }


    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.name == "Grindstone" && collision.rigidbody.angularVelocity.magnitude > 6 && grinding == false && GetComponent<IngotProperties>().type != IngotProperties.WeaponType.ingot)
        {
            StartCoroutine("SparksAndPause", collision.GetContact(0).point);
            Vector3 grindDirection;
            Vector3 grindFace;
            Vector3 tipFace;
            Vector3 toGrindstone = Vector3.Normalize(collision.gameObject.transform.position - transform.position);
            if (pinchAxis == 0) grindDirection = transform.right;
            else if (pinchAxis == 1) grindDirection = transform.up;
            else grindDirection = transform.forward;

            if (edge == 0) grindFace = transform.right;
            else if (edge == 1) grindFace = transform.up;
            else grindFace = transform.forward;

            if (tip == 0) tipFace = transform.right;
            else if (tip == 1) tipFace = transform.up;
            else tipFace = transform.forward;

            if (Mathf.Abs(toGrindstone.y - grindDirection.y) < 0.5 && Mathf.Abs(toGrindstone.z - grindDirection.z) < 0.5) // If the flat of the blade (positive direction) is facing the grindstone
            {
                if ((grindFace - toGrindstone).magnitude > (-grindFace - toGrindstone).magnitude) //If the back edge is tilted towards the grindstone
                {
                    Sharpen(edge + 3, pinchAxis, true);
                }
                else Sharpen(edge, pinchAxis, true); 
            }
            else if (Mathf.Abs(-toGrindstone.y - grindDirection.y) < 0.5 && Mathf.Abs(-toGrindstone.z - grindDirection.z) < 0.5) //If flat of the blade (other side) is facing the grindstone
            {
                if ((grindFace - toGrindstone).magnitude > (-grindFace - toGrindstone).magnitude)
                {

                    Sharpen(edge + 3, pinchAxis, false);//Sharpen(false);
                }
                else Sharpen(edge, pinchAxis, false);
            }
            else if (Mathf.Abs(toGrindstone.y - tipFace.y) < 0.3 && Mathf.Abs(toGrindstone.z - tipFace.z) < 0.3) //Point can be sharpened in 4 directions
            {
                Point(tip);
            }
            else if (Mathf.Abs(-toGrindstone.y - tipFace.y) < 0.3 && Mathf.Abs(-toGrindstone.z - tipFace.z) < 0.3)
            {
                Point(tip + 3);
            }
        }
    }

    IEnumerator SparksAndPause(Vector3 position)
    {
        if (!sparks.activeInHierarchy) sparks.SetActive(true);
        grinding = true;
        float time = Time.time;
        sparks.transform.position = position;
        sparks.transform.LookAt(transform.position);
        sparks.GetComponent<ParticleSystem>().Play();

        while (Time.time - time < .3)
        { 
            //throw up some sparks here
            yield return null;
        }
        grinding = false;
        sparks.GetComponent<ParticleSystem>().Stop();
    }


    private void UpdateMesh()
    {
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        meshCol.sharedMesh = mesh;
    }
    
    public void Point(int face)
    {
        Vector3 v = new Vector3(0.9f, 0.9f, 0.9f);
        v[tip] = 1.0f;

        for (int i = 0; i < faces[face].Count; i++)
        {
            changeVertices[faces[face][i]] = Vector3.Scale(changeVertices[faces[face][i]], v);
        }
        mesh.vertices = changeVertices;
        UpdateMesh();

        IngotProperties data = GetComponent<IngotProperties>();
        data.UpdatePointedness(face);
    }

    public void Sharpen(int face, int axis, bool pos)
    {
        Vector3 v = new Vector3(1, 1, 1);
        v[axis] = 0.9f;
        for (int i = 0; i < faces[face].Count; i++)
        {
            if((changeVertices[faces[face][i]][axis] > 0 && pos) || (changeVertices[faces[face][i]][axis] < 0 && !pos)) changeVertices[faces[face][i]] = Vector3.Scale(changeVertices[faces[face][i]], v);
        }
        mesh.vertices = changeVertices;
        UpdateMesh();

        int side = pos ? 1 : 0;
        IngotProperties data = GetComponent<IngotProperties>();
        data.UpdateSharpness(face, side);

    }



    public void Flatten(int axis)
    {
        if (CheckTemperature()) return;
        if (CheckLowerBound(axis)) return;
        float x = CheckUpperBound(0) ? 1.0f : 0.95f;
        float y = CheckUpperBound(1) ? 1.0f : 0.95f;
        float z = CheckUpperBound(2) ? 1.0f : 0.95f;

        if (axis == 0) x = 1.2f;
        else if (axis == 1) y = 1.2f;
        else z = 1.2f;

        UpdateDimensions(x,y,z);

        for (int i =- 0; i < changeVertices.Length; i++)
        {
            changeVertices[i] = new Vector3(changeVertices[i].x / x, changeVertices[i].y / y, changeVertices[i].z / z);
        }
        mesh.vertices = changeVertices;
        UpdateMesh();
    }

    private bool CheckWeaponType()
    {
        return false;
    }

    private bool CheckLowerBound(int axis)
    {
        //Returns true if the lower bound of given axis is outside physical limitations
        if (dimensions[axis] < 0.01f) return true;
        return false;
    }

    private bool CheckUpperBound(int axis)
    {
        //Returns true if the upper bound of the given axis is outside intended limitations
        if (dimensions[axis] > 4.0f) return true;
        return false;
    }

    private bool CheckTemperature()
    {
        if (tempInfo.currentHeat != Temperature.Heat.cold) return false;
        return true;
    }

    private void UpdateDimensions(float x, float y, float z)
    {
        // Updates the dimensions
        dimensions[0] = dimensions[0] / x;
        dimensions[1] = dimensions[1] / y;
        dimensions[2] = dimensions[2] / z;

        pinchAxis = GetMin(dimensions);
        edge = GetMid(dimensions);
        tip = GetMax(dimensions);
    }

    public int GetMin(float[] arr)
    {
        float lowest = Mathf.Infinity;
        int retval = arr.Length;
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] < lowest)
            {
                retval = i;
                lowest = arr[i];
            }
        }
        return retval;
    }

    public int GetMid(float[] arr)
    {
        //returns the middle value in an array of 3
        int min = GetMin(arr);
        if (min == 0)
        {
            return arr[1] < arr[2] ? 1 : 2;
        }
        if (min == 1)
        {
            return arr[0] < arr[2] ? 0 : 2;
        }
        if (min == 2)
        {
            return arr[0] < arr[1] ? 0 : 1;
        }
        return -1;

    }

    public int GetMax(float[] arr)
    {
        float highest = -1f;
        int retval = arr.Length;
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] > highest)
            {
                retval = i;
                highest = arr[i];
            }
        }
        return retval;
    }

}
