using UnityEngine;
using System.Collections;

public class Camera : MonoBehaviour {
    public Transform anchor;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        gameObject.transform.position = new Vector3(anchor.transform.position.x, anchor.transform.position.y, -10f);
	}
}
