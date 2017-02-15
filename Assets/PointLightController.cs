using UnityEngine;
using System.Collections;

public class PointLightController : MonoBehaviour {

    public float speed = 10.0f;
    public GameObject _gridPrefab;
    public float gridWidth = 10.0f;
    public float gridLength = 10.0f;
    
    void Start() {
        Instantiate(_gridPrefab, Vector3.zero, transform.rotation);
    }
    
    void Update() {
        if (Input.GetButton("Horizontal")) {
            transform.position = new Vector3(transform.position.x + Time.deltaTime * Input.GetAxis("Horizontal") * 10, transform.position.y, transform.position.z);
        }

        if (Input.GetButton("Vertical"))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + Time.deltaTime * Input.GetAxis("Vertical") * 10);
        }
    }

    /// <summary>
    /// Regenerate the grid Prefab
    /// </summary>
    public void reGenerate() {
        Instantiate(_gridPrefab, Vector3.zero, transform.rotation);
    }
}
