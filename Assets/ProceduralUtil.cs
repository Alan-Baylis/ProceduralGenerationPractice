using UnityEngine;
using System.Collections.Generic;

public class ProceduralUtil {

	public static void generateCube(Box box, Vector3[] baseVertices) {
		Vector3 p0 = baseVertices [0];
		Vector3 p1 = baseVertices [1];
		Vector3 p2 = baseVertices [2];
		Vector3 p3 = baseVertices [3];

		Vector3 p4 = new Vector3(baseVertices[0].x, baseVertices[0].y + box.getHeight(), baseVertices[0].z);
		Vector3 p5 = new Vector3(baseVertices[1].x, baseVertices[1].y + box.getHeight(), baseVertices[1].z);
		Vector3 p6 = new Vector3(baseVertices[2].x, baseVertices[2].y + box.getHeight(), baseVertices[2].z);
		Vector3 p7 = new Vector3(baseVertices[3].x, baseVertices[3].y + box.getHeight(), baseVertices[3].z);

		Vector3[] vertices = new Vector3[] {

			// bottom
			p0, p1 , p2, p3,

			// left
			p7, p4, p0, p3,

			// front
			p4, p5, p1, p0,

			// back
			p6, p7, p3, p2,

			// right
			p5, p6, p2, p1,

			// top
			p7, p6, p5, p4,

		};

		Vector3 top = Vector3.up;
		Vector3 down = Vector3.down;
		Vector3 left = Vector3.left;
		Vector3 right = Vector3.right;
		Vector3 front = Vector3.forward;
		Vector3 back = Vector3.back;

		Vector3[] normals = new Vector3[] {
			down,   down,   down,   down,

			left,   left,   left,   left,

			front,  front,  front,  front,

			back,   back,   back,   back,

			right,  right,  right,  right,

			top,    top,    top,    top
		};

		Vector2 _00 = new Vector2(0f, 0f);
		Vector2 _01 = new Vector2(0f, 1f);
		Vector2 _10 = new Vector2(1f, 0f);
		Vector2 _11 = new Vector2(1f, 1f);

		Vector2[] uvs = new Vector2[] {
			_11, _01, _00, _10,
			_11, _01, _00, _10,
			_11, _01, _00, _10,

			_11, _01, _00, _10,
			_11, _01, _00, _10,
			_11, _01, _00, _10,
		};

		int[] triangles = new int[] {
			3, 1, 0,
			3, 2, 1,

			3 + 4, 1 + 4, 0 + 4,
			3 + 4, 2 + 4, 1 + 4,

			3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2,
			3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,

			3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3,
			3 + 4 * 3, 2 + 4 *3 , 1 + 4 * 3,

			3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4,
			3 + 4 * 4, 2 + 4 * 4, 1 + 4 *4,

			3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5,
			3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5
		};

		box.getMesh().vertices = vertices;
		box.getMesh().normals = normals;
		box.getMesh().uv = uvs;
		box.getMesh().triangles = triangles;

        box.getMesh().RecalculateNormals();
		box.getMesh().RecalculateBounds();
		box.getMesh().Optimize();
	}

    public static List<Vector3> generateSquareBaseFace(Transform transform, float width, float length) {
        List<Vector3> baseFace = new List<Vector3>();
        
        // tranform from local to world space
        baseFace.Add(transform.TransformPoint(new Vector3(0.0f, 0.0f, length)));
        baseFace.Add(transform.TransformPoint(new Vector3(width, 0.0f, length)));
        baseFace.Add(transform.TransformPoint(new Vector3(width, 0.0f, 0.0f)));
        baseFace.Add(transform.position); 
        
        return baseFace;
    }

    public static List<Vector3> generateBaseShape_1(Transform transform, float width, float length, float offset) {
        List<Vector3> baseFace = new List<Vector3>();

        // tranform from local to world space
        baseFace.Add(transform.TransformPoint(new Vector3(0.0f, 0.0f, length - offset)));
        baseFace.Add(transform.TransformPoint(new Vector3(offset, 0.0f, length)));
        baseFace.Add(transform.TransformPoint(new Vector3(width, 0.0f, length)));
        baseFace.Add(transform.TransformPoint(new Vector3(width, 0.0f, offset)));
        baseFace.Add(transform.TransformPoint(new Vector3(width - offset, 0.0f, 0.0f)));
        baseFace.Add(transform.position);

        return baseFace;
    }

    public static List<Vector3> generateBaseShape_2(Transform transform, float width, float length, float offset)
    {
        List<Vector3> baseFace = new List<Vector3>();

        // tranform from local to world space
        baseFace.Add(transform.TransformPoint(new Vector3(0.0f, 0.0f, offset)));
        baseFace.Add(transform.TransformPoint(new Vector3(0.0f, 0.0f, length - offset)));
        baseFace.Add(transform.TransformPoint(new Vector3(offset, 0.0f, length)));
        baseFace.Add(transform.TransformPoint(new Vector3(width - offset, 0.0f, length)));
        baseFace.Add(transform.TransformPoint(new Vector3(width, 0.0f, length - offset)));
        baseFace.Add(transform.TransformPoint(new Vector3(width, 0.0f, offset)));
        baseFace.Add(transform.TransformPoint(new Vector3(width - offset, 0.0f, 0.0f)));
        baseFace.Add(transform.TransformPoint(new Vector3(offset, 0.0f, 0.0f)));

        return baseFace;
    }
        
    public static Mesh extrudeAFaceWithLevels(Box box, Vector3[] baseVertices, float baseBoundWidth, float baseBoundLength,
                                            float levelHeight, float resizeRatio) {
        List<Vector3> lowerLevelTransformedToLocal = new List<Vector3>();
        List<Vector3> upperLevel = new List<Vector3>();

            for (int j = 0; j < baseVertices.Length; j++) {
            // To resize, vertices have to be tranformed from world to local
			Vector3 lowerLevelWorldToLocal = box.getMeshFilter().transform.InverseTransformPoint(baseVertices[j]);
            lowerLevelTransformedToLocal.Add(lowerLevelWorldToLocal);

                float offset_x = baseBoundWidth * (1.0f - resizeRatio) / 2;
                float offset_z = baseBoundLength * (1.0f - resizeRatio) / 2;

                // Step 2 : Find upper level vertices(local) by resizing then adding offset, so two level's center will be on the same point
                Vector3 upperLevelLocalResized = new Vector3(lowerLevelWorldToLocal.x * resizeRatio + offset_x, lowerLevelWorldToLocal.y + levelHeight, lowerLevelWorldToLocal.z * resizeRatio + offset_z);
            
                upperLevel.Add(upperLevelLocalResized);
            }

        CombineInstance[] combine = new CombineInstance[2];
        Mesh lowerMesh = extrudeAFace (box, lowerLevelTransformedToLocal.ToArray(), levelHeight, false);
        combine[0].mesh = lowerMesh;

        Mesh upperMesh = extrudeAFace(box, upperLevel.ToArray(), levelHeight, false);
        combine[1].mesh = upperMesh;

        combine [0].transform = box.getMeshFilter().transform.localToWorldMatrix;
        combine [1].transform = box.getMeshFilter().transform.localToWorldMatrix;

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);

        // Step 3: Transform upper level into world and assign it to current box
        for (int i = 0; i < upperLevel.Count; i++) {
            upperLevel[i] = box.getMeshFilter().transform.TransformPoint(upperLevel[i]);
        }

        box.extrudedFaceVertices = upperLevel.ToArray();
        
        combinedMesh.RecalculateBounds();

        return combinedMesh;
    }

    public static Mesh extrudeAFace(Box box, Vector3[] baseVertices, float height, bool strip) {
		List<Vector3> verticeList = new List<Vector3> (baseVertices);
		List<Vector3> extrudedFace = new List<Vector3> ();

		for (int i = 0; i < baseVertices.Length; i++) {
			verticeList.Add (new Vector3(verticeList[i].x, verticeList[i].y + height, verticeList[i].z));
			extrudedFace.Add (new Vector3 (verticeList [i].x, verticeList [i].y + height, verticeList [i].z));
		} 

		List<int> trianglesList = new List<int> ();

		// sorting vertice by its index
		int count = baseVertices.Length;

		// initilize as top left corner
		int cursor = count + count - 1;

		List<int> verticesIndex = new List<int> ();

		// Iterate for i time. i = number of cap vertices
		for (int i = count; i > 0; i--) {

			if (cursor + 1 > count + count - 1) {
				// when exceed number of vertices
				verticesIndex.Add (cursor);
				verticesIndex.Add(cursor - count + 1);
				verticesIndex.Add(cursor - count + 1 - count);
				verticesIndex.Add(cursor - count);
             
				cursor = count - 1;
			} else {
				verticesIndex.Add (cursor);
				verticesIndex.Add(cursor + 1);
				verticesIndex.Add(cursor + 1 - count);
				verticesIndex.Add(cursor- count);
             
			}
			cursor += 1;
		}

        // add cap face
        for (int i = count * 2 - 1; i >= count; i--)
        {
            verticesIndex.Add(i);
        }
        
		List<Vector3> sortedVerticeList = new List<Vector3> ();
		for (int i = 0; i < verticesIndex.Count; i++) {
			sortedVerticeList.Add(verticeList[verticesIndex[i]]);
		}
			
		int verticeSetIterator;
		for (verticeSetIterator = 0; verticeSetIterator < count; verticeSetIterator++) {
			trianglesList.Add (3 + 4 * verticeSetIterator);
			trianglesList.Add (1 + 4 * verticeSetIterator);
			trianglesList.Add (0 + 4 * verticeSetIterator);

			trianglesList.Add (3 + 4 * verticeSetIterator);
			trianglesList.Add (2 + 4 * verticeSetIterator);
			trianglesList.Add (1 + 4 * verticeSetIterator);
		}

		for (int i = 0; i < count - 2; i++) {
			trianglesList.Add (2 + i + 4 * verticeSetIterator);
			trianglesList.Add (1 + i + 4 * verticeSetIterator);
			trianglesList.Add (0 + 4 * verticeSetIterator);
		}

        // ignore low levels

        List<Vector2> uvs = new List<Vector2>();

        if (height > 0.1f && !strip) {
            Vector2 _00 = new Vector2(0f, 0f);
            Vector2 _01 = new Vector2(0f, 1f);
            Vector2 _10 = new Vector2(1f, 0f);
            Vector2 _11 = new Vector2(1f, 1f);

            for (int i = 0; i < baseVertices.Length; i++)
            {
                uvs.Add(_11);
                uvs.Add(_01);
                uvs.Add(_00);
                uvs.Add(_10);
            }

            for (int i = baseVertices.Length * 4; i < sortedVerticeList.Count; i++)
            {
                uvs.Add(_11);
            }
        }
        
        Mesh extrudedMesh = new Mesh();
        extrudedMesh.vertices = sortedVerticeList.ToArray();
        extrudedMesh.triangles = trianglesList.ToArray();
        extrudedMesh.uv = uvs.ToArray();
        extrudedMesh.RecalculateNormals();
        extrudedMesh.RecalculateBounds();
        extrudedMesh.Optimize();


        // set top face box
        if (!strip) {
            box.extrudedFaceVertices = extrudedFace.ToArray();
        }

        return extrudedMesh;
	}

    public static Vector3[] generateRectangleBaseFromVertice(Vector3 center, float width, float length) {
        List<Vector3> squareBase = new List<Vector3>();
        squareBase.Add(new Vector3(center.x - width / 2, center.y, center.z - length / 2));
        squareBase.Add(new Vector3(center.x - width / 2, center.y, center.z + length / 2));
        squareBase.Add(new Vector3(center.x + width / 2, center.y, center.z + length / 2));
        squareBase.Add(new Vector3(center.x + width / 2, center.y, center.z - length / 2));

        return squareBase.ToArray();
    }

    public static Vector3[] generateSquareBaseFromVertice(Vector3 center, float length)
    {
        return generateRectangleBaseFromVertice(center, length, length);
    }

    public static Mesh generateBoardOnSide(Box box) {
        float resizeRatio = Random.Range(0.7f, 0.8f);

        // need to fix
        // width is for x axis
        float width = 0.005f;
        float boardLength = 1.0f;
        float height = 0.4f;

        float randomHangingPosX = 0.0f;
        float randomHangingPosZ = Random.Range(0.1f, box.getLength());
        float randomHangingPosY = Random.Range(0.1f, box.getHeight());

        int randomBaseStartingVertice = Random.Range(1, 2);
        Vector3 centerVertice;

        if (randomBaseStartingVertice == 1)
        {
            Vector3 sideBoard = box.baseVertices[box.baseVertices.Length - 1];
            centerVertice = new Vector3(sideBoard.x, sideBoard.y + randomHangingPosZ, sideBoard.z + randomHangingPosZ);
        }
        else {

            Vector3 sideBoard = box.baseVertices[box.baseVertices.Length - 2];
            centerVertice = new Vector3(sideBoard.x + randomHangingPosX, sideBoard.y + randomHangingPosZ, sideBoard.z + randomHangingPosZ);
        }
        
        Vector3[] rectangleBase = generateRectangleBaseFromVertice(centerVertice, boardLength, width);
        
        // need to fix: can be added a new method that handle board all at once
        List<Vector3> texturedBoard = new List<Vector3>();
        texturedBoard.Add(rectangleBase[2]);
        texturedBoard.Add(rectangleBase[3]);
        texturedBoard.Add(new Vector3(rectangleBase[2].x, rectangleBase[2].y + height, rectangleBase[2].z));
        texturedBoard.Add(new Vector3(rectangleBase[3].x, rectangleBase[3].y + height, rectangleBase[3].z));

        box.additionTextured.Add(new List<Vector3>(texturedBoard));

        return extrudeAFace(box, rectangleBase, height, true);
    }


    public static Mesh generateBoardOnRoof(Box box) {
        float resizeRatio = Random.Range(0.7f, 0.8f);

        float width = 0.005f;
        float height = 0.4f;

        // get any two adjacent vertice of a rectangle
        int randomBaseStartingVertice = Random.Range(0, 3);
        Vector3 startingVertice = box.extrudedFaceVertices[randomBaseStartingVertice];
        Vector3 endingVertice;

        if (randomBaseStartingVertice + 1 > box.extrudedFaceVertices.Length)
        {
            endingVertice = box.extrudedFaceVertices[randomBaseStartingVertice + 1 - box.extrudedFaceVertices.Length];
        }
        else {
            endingVertice = box.extrudedFaceVertices[randomBaseStartingVertice + 1];
        }

        if (startingVertice.x == endingVertice.x)
        {

            Vector3 centerVertice = new Vector3();
            centerVertice.x = startingVertice.x;
            centerVertice.y = startingVertice.y;

            float boardLength = Mathf.Abs(startingVertice.z - endingVertice.z) * resizeRatio;


            if (startingVertice.z > endingVertice.z)
            {
                centerVertice.z = startingVertice.z - Mathf.Abs(startingVertice.z - endingVertice.z) / 2;
            }
            else
            {
                centerVertice.z = startingVertice.z + Mathf.Abs(startingVertice.z - endingVertice.z) / 2;
            }

            List<Vector3> texturedBoard = new List<Vector3>();

            Vector3[] rectangleBase = generateRectangleBaseFromVertice(centerVertice, width, boardLength);
            texturedBoard.Add(rectangleBase[2]);
            texturedBoard.Add(rectangleBase[3]);
            texturedBoard.Add(new Vector3(rectangleBase[2].x, rectangleBase[2].y + height, rectangleBase[2].z));
            texturedBoard.Add(new Vector3(rectangleBase[3].x, rectangleBase[3].y + height, rectangleBase[3].z));

            box.additionTextured.Add(new List<Vector3>(texturedBoard));
            
            return extrudeAFace(box, rectangleBase, height, true);
        }
        else {

            Vector3 centerVertice = new Vector3();
            centerVertice.y = startingVertice.y;
            centerVertice.z = startingVertice.z;


            float boardLength = Mathf.Abs(startingVertice.x - endingVertice.x) * resizeRatio;


            if (startingVertice.x > endingVertice.x)
            {
                centerVertice.x = startingVertice.x - Mathf.Abs(startingVertice.x - endingVertice.x) / 2;
            }
            else
            {
                centerVertice.x = startingVertice.x + Mathf.Abs(startingVertice.x - endingVertice.x) / 2;
            }


            List<Vector3> texturedBoard = new List<Vector3>();

            Vector3[] rectangleBase = generateRectangleBaseFromVertice(centerVertice, boardLength, width);
            texturedBoard.Add(rectangleBase[0]);
            texturedBoard.Add(rectangleBase[3]);
            texturedBoard.Add(new Vector3(rectangleBase[0].x, rectangleBase[0].y + height, rectangleBase[0].z));
            texturedBoard.Add(new Vector3(rectangleBase[3].x, rectangleBase[3].y + height, rectangleBase[3].z));

            box.additionTextured.Add(new List<Vector3>(texturedBoard));

            return extrudeAFace(box, rectangleBase, height, true);
        }
    }

    public static List<int> removeTrianglesFromList(List<int> meshTriangles, List<int> trianglesToRemove) {
        List<int> remainingTriangles = new List<int>();
        bool pop;
        for (int i = 0; i < meshTriangles.Count / trianglesToRemove.Count; i ++   ) {
            pop = false;
            
            for (int j = 0; j < trianglesToRemove.Count; j++) {
                if (meshTriangles[i * trianglesToRemove.Count + j] != trianglesToRemove[j]) {
                    pop = true;
                }
            }
            
            if (pop) {
                for (int j = 0; j < trianglesToRemove.Count; j++) {
                    remainingTriangles.Add(meshTriangles[i * trianglesToRemove.Count + j]);
                }
            } 
        }

        return remainingTriangles;
    }

    /*
    public static void testRemoveTrianglesFromList() {
        List<int> meshTriangles = new List<int>(new int[] { 1, 2, 3, 4, 5, 6, 1, 3, 4, 2, 5, 6});
        List<int> trianglesToRemove = new List<int>(new int[] { 1, 2, 3, 4, 5, 6});
        
        string debug = "testRemoveTrianglesFromList: ";
        foreach (int t in ProceduralUtil.removeTrianglesFromList(meshTriangles, trianglesToRemove)) {
            debug += t + " ";
        }
        Debug.Log(debug);
    } */

    // will be always passing world vertices
    public static List<int> findTrianglesWithinVertices(Box box, Vector3[] vertices) {
        List<Vector3> localVerticeList = new List<Vector3>();
        List<int> triangleList = new List<int>();

        foreach (Vector3 worldVectice in vertices) {
            localVerticeList.Add(box.getMeshFilter().transform.InverseTransformPoint(worldVectice));
        }


        for (int i = 0; i < box.getMeshFilter().mesh.triangles.Length; i += 3) {
            
            if (localVerticeList.Contains(box.getMeshFilter().mesh.vertices[box.getMeshFilter().mesh.triangles[i]])
                && localVerticeList.Contains(box.getMeshFilter().mesh.vertices[box.getMeshFilter().mesh.triangles[i + 1]])
                && localVerticeList.Contains(box.getMeshFilter().mesh.vertices[box.getMeshFilter().mesh.triangles[i + 2]]) ) {
                triangleList.Add(box.getMeshFilter().mesh.triangles[i]);
                triangleList.Add(box.getMeshFilter().mesh.triangles[i + 1]);
                triangleList.Add(box.getMeshFilter().mesh.triangles[i + 2]);

            } 
        }

        return triangleList;
    }
}