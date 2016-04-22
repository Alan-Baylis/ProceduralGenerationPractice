using UnityEngine;
using System.Collections.Generic;

public class Box : MonoBehaviour
{

	private float length;
	private float width;
	private float height;

	public float maxLength = 0.8f;
	public float maxWidth = 0.8f;

    public float heightFraction = 0.5f;

    MeshRenderer _render;
    MeshFilter _filter;
    Mesh _mesh;
    MeshCollider _collider;

    public Vector3[] baseVertices;
    // extrudedFaceVertices represents vertices on upper level. Will always be on world scope.
    public Vector3[] extrudedFaceVertices;
    public List<List<Vector3>> additionTextured;

    List<Mesh> _meshList = new List<Mesh>();
    List<Vector3> debugVectices = new List<Vector3>();

    Material selfIlluminMainBuilding;
    Material boardMaterial;

    private Texture2D emissionMap;

    void Awake() {

        // Minimum = 1
        length = Random.Range(0.5f, maxLength);
		width = Random.Range(0.5f, maxWidth);

        height = Random.Range(10.0f / getDistanceTowardsCenter(), 10.0f / getDistanceTowardsCenter() + heightFraction);
        
        int baseShape = Random.Range(0, 3);

        if (baseShape == 0)
        {
            baseVertices = ProceduralUtil.generateSquareBaseFace(transform, getWidth(), getLength()).ToArray();
        }
        else if (baseShape == 1)
        {
            baseVertices = ProceduralUtil.generateBaseShape_1(transform, getWidth(), getLength(), 0.1f).ToArray();
        }
        else if (baseShape == 2) {
            baseVertices = ProceduralUtil.generateBaseShape_2(transform, getWidth(), getLength(), 0.1f).ToArray();
        }

        additionTextured = new List<List<Vector3>>();
    }

    // Use this for initialization
    void Start()
    {
        _render = gameObject.AddComponent<MeshRenderer>();
        // _render.materials[0].shader = Shader.Find("Diffuse");
        
        // Material for main building 
        selfIlluminMainBuilding = Resources.Load("Materials/apt_4", typeof(Material)) as Material;
        selfIlluminMainBuilding.SetColor("_EmissionColor", Color.white);

        int randomEmissionMap = Random.Range(4, 6);
        Texture emission = Resources.Load("Texture/emission_" + randomEmissionMap, typeof(Texture)) as Texture;
        selfIlluminMainBuilding.SetTexture("_EmissionMap", emission);

        boardMaterial = Resources.Load("Materials/board", typeof(Material)) as Material;

        _filter = gameObject.AddComponent<MeshFilter>();
        _mesh = _filter.mesh;
        
        // generate Random level of towers
        _meshList.Add(ProceduralUtil.extrudeAFace(this, baseVertices, getHeight(), false));

        // need to perform strip in the first place or top face vertice will be overwritten
        int strip = Random.Range(0, 3);
        float stripSize = Random.Range(0.0f, 0.2f);
        float stripHeight = Random.Range(0.0f, getHeight());
        if (strip == 0)
        {
            for (int i = 0; i < baseVertices.Length; i++)
            {
                Vector3[] squareBase = new Vector3[4];
                squareBase = ProceduralUtil.generateSquareBaseFromVertice(baseVertices[i], stripSize);

                _meshList.Add(ProceduralUtil.extrudeAFace(this, squareBase, stripHeight, true));
            }
            // only generate strip lines if the building is a cube
        }

        int preserveResizeRatio = Random.Range(0, 4);
        float randomHeightLevel = Random.Range(0.1f, 0.3f);

        if (preserveResizeRatio == 0)
        {
            int numberOfLevel = Random.Range(0, 5);
            float randomRatio = Random.Range(0.0f, 1.0f);
            for (int i = 0; i < numberOfLevel; i++)
            {
                _meshList.Add(ProceduralUtil.extrudeAFaceWithLevels(this, extrudedFaceVertices, getWidth(), getLength(), randomHeightLevel, randomRatio));
                _meshList.Add(ProceduralUtil.extrudeAFaceWithLevels(this, extrudedFaceVertices, getWidth(), getLength(), randomHeightLevel, 1.0f / randomRatio));
            }
            
            for (int i = 0; i < extrudedFaceVertices.Length; i++)
            {
                extrudedFaceVertices[i] = extrudedFaceVertices[i] + new Vector3(0.0f, randomHeightLevel, 0.0f);
            }

        }
        else if(preserveResizeRatio == 1){
            int numberOfLevel = Random.Range(0, 3);
            float randomRatio = Random.Range(0.0f, 1.0f); for (int i = 0; i < numberOfLevel; i++)
            {
                _meshList.Add(ProceduralUtil.extrudeAFaceWithLevels(this, extrudedFaceVertices, getWidth(), getLength(), randomHeightLevel, randomRatio));
            }
            
            for (int i = 0; i < extrudedFaceVertices.Length; i++)
            {
                extrudedFaceVertices[i] = extrudedFaceVertices[i] + new Vector3(0.0f, randomHeightLevel, 0.0f);
            }

        }
        
        // 1 / 6 chance to generate a board
        int generateABoard = Random.Range(1, 4);
        if (generateABoard == 1)
        {
            _meshList.Add(ProceduralUtil.generateBoardOnRoof(this));
        }

        generateABoard = Random.Range(1, 4);
        if (generateABoard == 1)
        {
            _meshList.Add(ProceduralUtil.generateBoardOnSide(this));
        }

        Mesh combinedMesh = combineMeshes();
        List<int> combineMeshTriangles = new List<int>(combinedMesh.triangles);

        // need to set filter mesh before call findTrianglesWIthinVertices
        this._filter.mesh = combinedMesh;

        List<int> boardTriangles = new List<int>();

        // if the tower contains board 
        if (additionTextured.Count != 0)
        {

            combinedMesh.subMeshCount = additionTextured.Count + 1;
            List<int> towerTriangles = new List<int>();

            for (int i = 0; i < additionTextured.Count; i++) {
                boardTriangles = ProceduralUtil.findTrianglesWithinVertices(this, additionTextured[i].ToArray());

                combinedMesh.SetTriangles(boardTriangles.ToArray(), i);
                towerTriangles = ProceduralUtil.removeTrianglesFromList(new List<int>(combineMeshTriangles), boardTriangles);
            }

            combinedMesh.SetTriangles(towerTriangles.ToArray(), additionTextured.Count);

            Material[] materials = new Material[additionTextured.Count + 1];
            for (int i = 0; i < additionTextured.Count; i++)
            {
                materials[i] = boardMaterial;

            }
            materials[additionTextured.Count] = selfIlluminMainBuilding;
            _render.materials = materials;
        }
        else {
            // set default material as main building material
            _render.material = selfIlluminMainBuilding;
        }

        this._filter.mesh = combinedMesh;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public float getLength() {
        return length;
    }

	public float getWidth() {
        return width;
    }

	public float getHeight() {
		return height;
	}

	public Mesh getMesh() {
		return _mesh;
	}

	public MeshFilter getMeshFilter() {
		return _filter;
	}

    private float getDistanceTowardsCenter() {
        return Mathf.Ceil(Mathf.Sqrt(transform.position.x * transform.position.x + transform.position.z * transform.position.z));
    }

    private Mesh combineMeshes() {
        CombineInstance[] combine = new CombineInstance[_meshList.Count];
        for (int i = 0; i < _meshList.Count; i++) {
            combine[i].mesh = _meshList[i];
            combine[i].transform = transform.worldToLocalMatrix;
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);

        // combinedMesh.RecalculateNormals();
        combinedMesh.RecalculateBounds();

        return combinedMesh;
    }

    // for debugging
    /*
	void OnDrawGizmos() {
		for (int i = 0; i < debugVectices.Count; i++) {
			Gizmos.DrawSphere (debugVectices[i], 0.1f);
		}
	} */
    
    private Texture2D generateEmissionMap(Texture mainTexture)
    {
        Texture2D emissionMap = new Texture2D(mainTexture.width, mainTexture.height);

        int window_width = 80;
        int window_height = 160;

        int interval_y = 135;
        int interval_x = 320;

        // fill in all black initially
        for (int i = 0; i < mainTexture.width; i++)
        {
            for (int j = 0; j < mainTexture.height; j++)
            {
                emissionMap.SetPixel(i, j, Color.black);
            }
        }

        int numLightedWindow = Random.Range(4, 8);

        for (int i = 0; i < numLightedWindow; i++)
        {
            int row = Random.Range(0, 6);
            int col = Random.Range(0, 5);

            Vector2 cursor = new Vector2(100 + (window_width + interval_x) * row, 24 + (window_height + interval_y) * col);

            for (int k = (int)cursor.x; k < cursor.x + window_width; k++)
            {
                for (int j = (int)cursor.y; j < cursor.y + window_height; j++)
                {
                    emissionMap.SetPixel(k, j, Color.white);
                }
            }

            emissionMap.Apply();
        }

        return emissionMap;
    }
}
