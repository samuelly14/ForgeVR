using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngotMesh : MonoBehaviour
{
    
    //This script will allow for full deformation of the mesh, save for the "tang". 
    //Workflow: Flatten >> Create Tang >> "Everything else". Flattening can only be done before creating a tang. Everything else can only be done after creating a tang. 
    //The tang will be created with a "guillotine" type device that the player can attach to the anvil
    //The player will attach the guillotine, place the metal inside to determine where the tang will be formed, and strike the guillotine with the hammer
    //Add bending mechanic here too: bend point will be the contact point between the bell of the anvil and the ingot. Bend direction will be towards the ground. 
    //Everything else: Bending, sculpting, chiselling. 
    //For hammers and axes: Punch a hole to determine where the haft will attach. 


    private Mesh mesh;
    private MeshCollider meshCol;
    private List<int> x1Face = new List<int>();
    private List<int> y1Face = new List<int>();
    private List<int> z1Face = new List<int>();
    private List<int> x2Face = new List<int>();
    private List<int> y2Face = new List<int>();
    private List<int> z2Face = new List<int>();

    private List<List<int>> faces;

    private void Awake()
    {
    }

    private void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        meshCol = GetComponent<MeshCollider>();
    }

    private void GetFaces()
    {
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            if (mesh.vertices[i].x >= 0.99f)
            {
                x1Face.Add(i);
            }
            else if (mesh.vertices[i].x <= -0.99f)
            {
                x2Face.Add(i);
            }

            if (mesh.vertices[i].y >= 0.99f)
            {
                y1Face.Add(i);
            }
            else if (mesh.vertices[i].y <= -0.99f)
            {
                y2Face.Add(i);
            }

            if (mesh.vertices[i].z >= 0.99f)
            {
                z1Face.Add(i);
            }
            else if (mesh.vertices[i].z <= -0.99f)
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

    private void Strike()
    {

    }
}
