using UnityEngine;
using System.Collections;

public class RoomGen : MonoBehaviour {
    public TowerGen.Room room;
    public GameObject wallPrefab;

	// Use this for initialization
	void Start () 
    {
	}
	
	// Update is called once per frame
	void Update () 
    {
	}

    public void Generate()
    {
        GameObject leftWall = Instantiate(wallPrefab) as GameObject;
        leftWall.transform.localScale = new Vector3(1f, room.h * 16f);
        leftWall.transform.localPosition = new Vector3(room.x * 32f, room.y * 32f, 0);
        leftWall.transform.parent = gameObject.transform;

        GameObject rightWall = Instantiate(wallPrefab) as GameObject;
        rightWall.transform.localScale = new Vector3(1f, room.h * 16f);
        rightWall.transform.localPosition = new Vector3((room.x + room.w) * 32f - 2f, room.y * 32f, 0);
        rightWall.transform.parent = gameObject.transform;

        GameObject topWall = Instantiate(wallPrefab) as GameObject;
        topWall.transform.localScale = new Vector3(room.w * 16f, 1f);
        topWall.transform.localPosition = new Vector3(room.x * 32f, (room.y + room.h) * 32f - 2f, 0);
        topWall.transform.parent = gameObject.transform;
        
        GameObject bottomWall = Instantiate(wallPrefab) as GameObject;
        bottomWall.transform.localScale = new Vector3(room.w * 16f, 1f);
        bottomWall.transform.localPosition = new Vector3(room.x * 32f, room.y * 32f, 0);
        bottomWall.transform.parent = gameObject.transform;
    }
}
