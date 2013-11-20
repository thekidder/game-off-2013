using UnityEngine;
using System.Collections.Generic;

public class RoomGen : MonoBehaviour {
    public TowerGen.Room room;
    public TowerGen.Params towerParams;

    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject ladderPrefab;

    delegate void GeometryGenerator(Vector2 size, Vector2 pos, Color color);

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
        GenerateWallWithOpenings(
            TowerGen.ConnectionType.FLOOR,
            new Vector2(0f, 0f),
            GenerateFloor, Color.black);

        GenerateWallWithOpenings(
            TowerGen.ConnectionType.CEILING,
            new Vector2(0f, room.h * towerParams.blockHeight - 1f),
            GenerateWall, Color.black);

        GenerateWallWithOpenings(
            TowerGen.ConnectionType.LEFT_WALL,
            new Vector2(0f, 0f),
            GenerateWall, Color.black);

        GenerateWallWithOpenings(
            TowerGen.ConnectionType.RIGHT_WALL,
            new Vector2(room.w * towerParams.blockWidth - 1f, 0f),
            GenerateWall, Color.black);

        GenerateWallWithOpenings(
            TowerGen.ConnectionType.FLOOR,
            new Vector2(0f, towerParams.blockHeight / 2f - 2f),
            GenerateFloor, Color.grey);

        foreach (TowerGen.Connection c in room.connections) {
            if (c.type == TowerGen.ConnectionType.CEILING) {
                GenerateLadder(
                    new Vector2(4f, towerParams.blockHeight * room.h - towerParams.blockHeight / 2f + 1f),
                    new Vector2(c.placement.x, towerParams.blockHeight / 2f - 1f),
                    Color.yellow);
            } else if (c.type == TowerGen.ConnectionType.FLOOR) {
                GenerateLadder(
                    new Vector2(4f, towerParams.blockHeight / 2f - 1f),
                    new Vector2(c.placement.x, 0f),
                    Color.yellow);
            } else if (c.type == TowerGen.ConnectionType.LEFT_WALL) {
                if (c.placement.x > towerParams.blockHeight / 2f - 2f) {
                    GenerateLadder(
                        new Vector2(4f, c.placement.y - towerParams.blockHeight / 2f + 1f),
                        new Vector2(1f, towerParams.blockHeight / 2f - 1f),
                        Color.yellow);
                }
            } else if (c.type == TowerGen.ConnectionType.RIGHT_WALL) {
                if (c.placement.x > towerParams.blockHeight / 2f - 2f) {
                    GenerateLadder(
                        new Vector2(4f, c.placement.y - towerParams.blockHeight / 2f + 1f),
                        new Vector2(towerParams.blockWidth * room.w - 5f, towerParams.blockHeight / 2f - 1f),
                        Color.yellow);
                }
            }
        }
    }

    void GenerateWallWithOpenings(TowerGen.ConnectionType type, Vector2 startPos, GeometryGenerator generator, Color color) {
        List<TowerGen.Connection> connections = room.connections.FindAll(conn => conn.type == type);
        connections.Sort((lhs, rhs) => (int)(lhs.placement.x - rhs.placement.x));

        foreach (TowerGen.Connection c in connections) {
            float end = c.placement.x;

            if (type == TowerGen.ConnectionType.LEFT_WALL || type == TowerGen.ConnectionType.RIGHT_WALL) {
                generator(new Vector2(1f, end - startPos.y), startPos, color);
                startPos = new Vector2(startPos.x, c.placement.y);
            } else {
                generator(new Vector2(end - startPos.x, 1f), startPos, color);
                startPos = new Vector2(c.placement.y, startPos.y);
            }
        }

        if (type == TowerGen.ConnectionType.LEFT_WALL || type == TowerGen.ConnectionType.RIGHT_WALL) {
            float end = room.h * towerParams.blockHeight;
            generator(new Vector2(1f, end - startPos.y), startPos, color);
        } else {
            float end = room.w * towerParams.blockWidth;
            generator(new Vector2(end - startPos.x, 1f), startPos, color);
        }
    }

    void GenerateFloor(Vector2 size, Vector2 position, Color color) 
    {
        GameObject floor = Instantiate(floorPrefab) as GameObject;
        floor.GetComponent<SpriteRenderer>().color = color;
        SetTransform(floor, size, position);
    }

    void GenerateWall(Vector2 size, Vector2 position, Color color) {
        GameObject wall = Instantiate(wallPrefab) as GameObject;
        wall.GetComponent<SpriteRenderer>().color = color;
        SetTransform(wall, size, position);
    }

    void GenerateLadder(Vector2 size, Vector2 position, Color color) {
        GameObject ladder = Instantiate(ladderPrefab) as GameObject;
        ladder.GetComponent<SpriteRenderer>().color = color;
        SetTransform(ladder, size, position);
    }

    void SetTransform(GameObject go, Vector2 size, Vector2 position) {
        Bounds bounds = go.GetComponent<SpriteRenderer>().bounds;
        go.transform.parent = gameObject.transform;

        go.transform.localScale = new Vector3(size.x / bounds.size.x, size.y / bounds.size.y, 0f);
        go.transform.localPosition = new Vector3(position.x, position.y, 0);
    }
}
