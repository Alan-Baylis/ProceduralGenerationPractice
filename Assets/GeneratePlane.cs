using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GeneratePlane : MonoBehaviour {

    private Mesh mesh;
    private Vector3[] vertices;
    private Vector2[] uv;
    public int xSize;
    public int zSize;
    public float height;
    
    public GameObject cube;

    void Awake() {
        generate();
        gameObject.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Snow", typeof(Material)) as Material; ;
    }

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    private void generate() {
        mesh = new Mesh();
        mesh.name = "Procedural Grid";
        
        vertices = new Vector3[(xSize + 1) * (zSize + 1) * 2];
        uv = new Vector2[vertices.Length];

        for (int i = 0, z = 0; z <= zSize; z++) {
            for (int x = 0; x <= xSize; x++, i ++) {
                vertices[i] = new Vector3(x, 0, z);
                uv[i] = new Vector2((float)x / xSize, (float)z / zSize);

                if (z != zSize) {
                    int toGenerateBuilding = Random.Range(1, 3);
                    if (toGenerateBuilding == 2)
                    {
                        GameObject cubeObject = (GameObject)Instantiate(cube, new Vector3(x, 0, z), transform.rotation);
                        cubeObject.tag = "building";
                    }
                }
            }
        }

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {
                vertices[i + (xSize + 1) * (zSize + 1)] = new Vector3(x, height, z);
                uv[i + (xSize + 1) * (zSize + 1)] = new Vector2((float)x / xSize, (float)z / zSize);
            }
        }

        mesh.vertices = vertices;
        
        List<int> trianglesList = new List<int>();
        for (int i = 0; i < xSize; i++) {
            // Right
            trianglesList.Add(i);
            trianglesList.Add(i + (xSize + 1) * (zSize + 1));
            trianglesList.Add(i + (xSize + 1) * (zSize + 1) + 1);
            
            trianglesList.Add(i);
            trianglesList.Add(i + (xSize + 1) * (zSize + 1) + 1);
            trianglesList.Add(i + 1);

            // Left
            trianglesList.Add(zSize * (xSize + 1) + i);
            trianglesList.Add(zSize * (xSize + 1) + i + (xSize + 1) * (zSize + 1) + 1);
            trianglesList.Add(zSize * (xSize + 1) + i + (xSize + 1) * (zSize + 1));

            trianglesList.Add(zSize * (xSize + 1) + i);
            trianglesList.Add(zSize * (xSize + 1) + i + 1);
            trianglesList.Add(zSize * (xSize + 1) + i + (xSize + 1) * (zSize + 1) + 1);
        }

        // Top
        for (int i = 0; i < (xSize + 1) * zSize; i++) {
            trianglesList.Add((xSize + 1) * (zSize + 1) + i );
            trianglesList.Add((xSize + 1) * (zSize + 1) + i + xSize);
            trianglesList.Add((xSize + 1) * (zSize + 1) + i + xSize + 1);

            trianglesList.Add((xSize + 1) * (zSize + 1) + i);
            trianglesList.Add((xSize + 1) * (zSize + 1) + i + xSize + 1);
            trianglesList.Add((xSize + 1) * (zSize + 1) + i + 1);
        }

        // Front
        for (int i = 0; i < zSize; i++) {
            trianglesList.Add((i + 1) * (zSize + 1));
            trianglesList.Add(i * (zSize + 1) + (xSize + 1) * (zSize + 1));
            trianglesList.Add(i * (zSize + 1));

            trianglesList.Add((i + 1) * (zSize + 1));
            trianglesList.Add((i + 1) * (zSize + 1) + (xSize + 1) * (zSize + 1));
            trianglesList.Add(i * (zSize + 1) + (xSize + 1) * (zSize + 1));
        }

        // Back
        for (int i = 0; i < zSize; i++)
        {
            trianglesList.Add(zSize + (i + 1) * (zSize + 1));
            trianglesList.Add(zSize + i * (zSize + 1));
            trianglesList.Add(zSize + i * (zSize + 1) + (xSize + 1) * (zSize + 1));


            trianglesList.Add(zSize + (i + 1) * (zSize + 1));
            trianglesList.Add(zSize + i * (zSize + 1) + (xSize + 1) * (zSize + 1));
            trianglesList.Add(zSize + (i + 1) * (zSize + 1) + (xSize + 1) * (zSize + 1));
        }

        mesh.triangles = trianglesList.ToArray();
        mesh.RecalculateNormals();
        mesh.uv = uv;
        
        gameObject.GetComponent<MeshFilter>().mesh = mesh;

        generateHeight();
    }

    private void generateHeight() {
        Vector3[] vertices = mesh.vertices;
        float randomSeed = 1.0f;
        for (int i = 0; i < xSize + 1; i++) {
            for (int j = 0; j < zSize + 1; j++) {
                vertices[(xSize + 1) * (zSize + 1) + i * (xSize + 1) + j].y = Mathf.PerlinNoise((xSize + 1) * (zSize + 1) + i * (xSize + 1) + j, randomSeed);
                randomSeed += 0.1f;
            }
        }

        mesh.vertices = vertices;
    }
}
