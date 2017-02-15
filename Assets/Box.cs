using UnityEngine;
using System.Collections.Generic;

public class Box : MonoBehaviour
{
    #region private field 
    private float length;
	private float width;
	private float height;
    
    private MeshRenderer _render;
    private MeshFilter _filter;
    private Mesh _mesh;
    private MeshCollider _collider;

    private List<Mesh> _meshList = new List<Mesh>();
    private List<Vector3> debugVectices = new List<Vector3>();

    private Material selfIlluminMainBuilding;
    private Material boardMaterial;
    private Texture2D emissionMap;
    #endregion

    #region public field 
    public float maxLength = 0.8f;
	public float maxWidth = 0.8f;

    public float heightFraction = 0.5f;
    
    public Vector3[] baseVertices;
    public Vector3[] extrudedFaceVertices;
    public List<List<Vector3>> additionTextured;
    #endregion

    #region setter and getter
    public float Length
    {
        set
        {
            this.length = value;
        }
        get
        {
            return this.length;
        }
    }

    public float Width
    {
        set
        {
            this.width = value;
        }
        get
        {
            return this.width;
        }
    }

    public float Height
    {
        set
        {
            this.height = value;
        }
        get
        {
            return this.height;
        }
    }

    public Mesh Mesh
    {
        set
        {
            this._mesh = value;
        }
        get
        {
            return this._mesh;
        }
    }

    public MeshFilter MeshFilter
    {
        set
        {
            this._filter = value;
        }
        get
        {
            return this._filter;
        }
    }
    #endregion


    void Awake() {
        length = Random.Range(0.5f, maxLength);
		width = Random.Range(0.5f, maxWidth);
        height = Random.Range(10.0f / getDistanceTowardsCenter(), 10.0f / getDistanceTowardsCenter() + heightFraction);
        
        // Random BaseShape
        int baseShape = Random.Range(0, 3);

        if (baseShape == 0)
        {
            baseVertices = ProceduralUtil.generateSquareBaseFace(transform, Width, Length).ToArray();
        }
        else if (baseShape == 1)
        {
            baseVertices = ProceduralUtil.generateBaseShape_1(transform, Width, Length, 0.1f).ToArray();
        }
        else if (baseShape == 2) {
            baseVertices = ProceduralUtil.generateBaseShape_2(transform, Width, Length, 0.1f).ToArray();
        }

        additionTextured = new List<List<Vector3>>();
    }
    
    void Start()
    {
        _render = gameObject.AddComponent<MeshRenderer>();

        // Main building material
        selfIlluminMainBuilding = Resources.Load("Materials/apt_4", typeof(Material)) as Material;
        selfIlluminMainBuilding.SetColor("_EmissionColor", Color.white);

        int randomEmissionMap = Random.Range(4, 6);
        Texture emission = Resources.Load("Texture/emission_" + randomEmissionMap, typeof(Texture)) as Texture;
        selfIlluminMainBuilding.SetTexture("_EmissionMap", emission);

        boardMaterial = Resources.Load("Materials/board", typeof(Material)) as Material;

        _filter = gameObject.AddComponent<MeshFilter>();
        _mesh = _filter.mesh;
        
        // generate Random level of towers
        _meshList.Add(ProceduralUtil.extrudeAFace(this, baseVertices, Height, false));

        // need to perform strip in the first place or top face vertice will be overwritten
        int strip = Random.Range(0, 3);
        float stripSize = Random.Range(0.0f, 0.2f);
        float stripHeight = Random.Range(0.0f, Height);
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
                _meshList.Add(ProceduralUtil.extrudeAFaceWithLevels(this, extrudedFaceVertices, Height, Length, randomHeightLevel, randomRatio));
                _meshList.Add(ProceduralUtil.extrudeAFaceWithLevels(this, extrudedFaceVertices, Height, Length, randomHeightLevel, 1.0f / randomRatio));
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
                _meshList.Add(ProceduralUtil.extrudeAFaceWithLevels(this, extrudedFaceVertices, Width, Length, randomHeightLevel, randomRatio));
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
    
    /// <summary>
    /// Calculate Distance of current transform from world center
    /// </summary>
    private float getDistanceTowardsCenter() {
        return Mathf.Ceil(Mathf.Sqrt(transform.position.x * transform.position.x + transform.position.z * transform.position.z));
    }

    /// <summary>
    /// Combine different mesh from a mesh list 
    /// </summary>
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
    
    /// <summary>
    /// Procedural Generate emission Map
    /// </summary>
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
