using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class TowerGen : MonoBehaviour
{
    public float blockWidth = 32f;
    public float blockHeight = 32f;

    public int towerHeight;
    public int towerWidth;

    public int numRoomsMin;
    public int minRemovedConnections;
    public int numSpecialRooms;

    public Seed seed;
    public GameObject block;

    public List<GridSquare> grid;
    public List<Room> rooms;

    public class GridSquare
    {
        public bool filled = false;
        public Room room = null;
    }

    public class Room
    {
        public List<Room> connections = new List<Room> ();
        public bool connected = false;

        public int x;
        public int y;
        public int w;
        public int h;
    }

    class RoomProbability
    {
        public int weight;
        public int w;
        public int h;
    }


    // Use this for initialization
    void Start ()
    {

    }

    // Update is called once per frame
    void Update ()
    {
        if (seed.Changed ()) {
            Regen ();
            seed.Sync ();
        }
    }

    void Regen ()
    {
        DestroyTower ();

        Random.seed = seed.seed;

        towerWidth = Random.Range (4, 7);
        towerHeight = Random.Range (9, 16);

        grid = new List<GridSquare> (towerWidth * towerHeight);
        for (int i = 0; i < towerWidth * towerHeight; ++i) {
            grid.Add (new GridSquare ());
        }

        for (int i = 0; i < towerWidth; ++i) {
            for (int j = 0; j < towerHeight; ++j) {
                float offsetX = Random.Range (-0.5f, 0.5f);
                float offsetY = Random.Range (-0.5f, 0.5f);

                float factor = Mathf.Max (towerWidth, towerHeight);

                float x = i / factor + offsetX;
                float y = j / factor + offsetY;
               
                float heightPercent = (float)j / (towerHeight - 1);

                // reject according to perlin noise that is scaled with height
                if (Mathf.PerlinNoise (x, y) < 0.15f * heightPercent + 0.25f
                    && (i != 0 || j != 0)) {
                    continue;
                }

                GetSquare (i, j).filled = true;
            }
        }

        // create rooms
        List<RoomProbability> probs = new List<RoomProbability> ();
        probs.Add (new RoomProbability { weight =  1, w = 3, h = 1 });
        probs.Add (new RoomProbability { weight =  1, w = 1, h = 3 });
        probs.Add (new RoomProbability { weight =  8, w = 2, h = 2 });
        probs.Add (new RoomProbability { weight =  10, w = 1, h = 2 });
        probs.Add (new RoomProbability { weight =  14, w = 2, h = 1 });

        int sum = 0;
        foreach (RoomProbability r in probs) {
            sum += r.weight;
        }

        numSpecialRooms = towerWidth * towerHeight / 6;

        rooms = new List<Room> ();

        for (int i = 0; i < numSpecialRooms; ++i) {
            int choice = Random.Range (0, sum);

            int total = 0;
            RoomProbability spec = null;
            foreach (RoomProbability r in probs) {
                total += r.weight;

                if (choice < total) {
                    spec = r;
                    break;
                }
            }

            for (int j = 0; j < 5; ++j) { // attempts to place
                int x = Random.Range (0, towerWidth - spec.w + 1);
                int y = Random.Range (0, towerHeight - spec.h + 1);

                bool valid = true;

                for (int ii = 0; ii < spec.w; ++ii) {
                    for (int jj = 0; jj < spec.h; ++jj) {
                        GridSquare sq = GetSquare (x + ii, y + jj);

                        if (sq == null) {
                            valid = false;
                            break;
                        }

                        if (!sq.filled || sq.room != null) {
                            valid = false;
                        }
                    }
                }
                if (!valid) {
                    continue;
                }

                Room r = new Room {w = spec.w, h = spec.h, x = x, y = y};
                rooms.Add (r);

                for (int ii = 0; ii < spec.w; ++ii) {
                    for (int jj = 0; jj < spec.h; ++jj) {
                        GridSquare sq = GetSquare (x + ii, y + jj);
                        sq.room = r;
                    }
                }

                break;
            }
        }

        // all 1x1 rooms
        for (int i = 0; i < towerWidth; ++i) {
            for (int j = 0; j < towerHeight; ++j) {
                GridSquare sq = GetSquare (i, j);
                if (sq.room == null && sq.filled) {
                    sq.room = new Room { x = i, y = j, w = 1, h = 1};
                    rooms.Add (sq.room);
                }
            }
        }

        // connect all rooms to all neighbors
        foreach (Room r in rooms) {
            r.connections = AdjacentRooms (r);
        }

        // flood fill to ensure connected
        SetConnected (GetSquare (0, 0).room);
        rooms.RemoveAll (item => !item.connected);

        numRoomsMin = (int)Random.Range (rooms.Count * 0.5f, rooms.Count * 0.75f);
        minRemovedConnections = Random.Range (rooms.Count / 3, rooms.Count / 2);

        // flood fill to ensure tower is connected
        for (int i = 0; i < minRemovedConnections && rooms.Count >= numRoomsMin;) {
            Room r = rooms [Random.Range (0, rooms.Count)];

            if (r.connections.Count == 0)
                continue;

            RemoveConnection (r, r.connections [Random.Range (0, r.connections.Count)]);

            ResetConnected ();
            SetConnected (GetSquare (0, 0).room);
            ++i;

            rooms.RemoveAll (item => !item.connected);
        }


        // display
        foreach (Room r in rooms) {
            Vector3 pos = new Vector3 (r.x * blockWidth, r.y * blockHeight, 0f) + gameObject.transform.position;
            GameObject newBlock = (GameObject)Instantiate (block, pos, Quaternion.identity);
            newBlock.transform.localScale = new Vector3 (blockWidth * r.w, blockHeight * r.h, 1f);

            newBlock.transform.parent = gameObject.transform;
        }
    }

    public Vector2 SharedHorizontalEdge (Room lhs, Room rhs)
    {
        float left = Mathf.Max (rhs.x * blockWidth, lhs.x * blockWidth) + gameObject.transform.position.x;
        float right = Mathf.Min ((rhs.x + rhs.w) * blockWidth, (lhs.x + lhs.w) * blockWidth) + gameObject.transform.position.x;

        return new Vector2 (left, right);
    }

    public Vector2 SharedVerticalEdge (Room rhs, Room lhs)
    {
        float bottom = Mathf.Max (rhs.y * blockHeight, lhs.y * blockHeight) + gameObject.transform.position.y;
        float top = Mathf.Min ((rhs.y + rhs.h) * blockHeight, (lhs.y + lhs.h) * blockHeight) + gameObject.transform.position.y;

        return new Vector2 (bottom, top);
    }

    List<Room> AdjacentRooms (Room r)
    {
        HashSet<Room> adjacentRooms = new HashSet<Room> ();

        for (int i = 0; i < r.w; ++i) {
            for (int j = 0; j < r.h; ++j) {
                GridSquare left = GetSquare (i + r.x - 1, j + r.y);
                if (left != null && left.room != null && left.room != r) {
                    adjacentRooms.Add (left.room);
                }

                GridSquare right = GetSquare (i + r.x + 1, j + r.y);
                if (right != null && right.room != null && right.room != r) {
                    adjacentRooms.Add (right.room);
                }

                GridSquare up = GetSquare (i + r.x, j + r.y + 1);
                if (up != null && up.room != null && up.room != r) {
                    adjacentRooms.Add (up.room);
                }

                GridSquare down = GetSquare (i + r.x, j + r.y - 1);
                if (down != null && down.room != null && down.room != r) {
                    adjacentRooms.Add (down.room);
                }
            }
        }

        return new List<Room> (adjacentRooms);
    }

    void RemoveConnection (Room lhs, Room rhs)
    {
        lhs.connections.Remove (rhs);
        rhs.connections.Remove (lhs);
    }

    void RemoveRoom (Room room)
    {
        foreach (Room r in rooms) {
            if (r.connections.Contains (room)) {
                r.connections.Remove (room);
            }
        }
        for (int i = 0; i < towerWidth; ++i) {
            for (int j = 0; j < towerHeight; ++j) {
                GridSquare sq = GetSquare (i, j);
                if (sq.room == room) {
                    sq.room = null;
                }
            }
        }
        rooms.Remove (room);
    }

    void ResetConnected ()
    {
        foreach (Room r in rooms) {
            r.connected = false;
        }
    }

    void SetConnected (Room r)
    {
        r.connected = true;

        foreach (Room adj in r.connections) {
            if (!adj.connected)
                SetConnected (adj);
        }
    }

    bool InRange (int x, int y)
    {
        return x >= 0 && x < towerWidth && y >= 0 && y < towerHeight;
    }

    GridSquare GetSquare (int x, int y)
    {
        if (!InRange (x, y))
            return null;

        return grid [x + towerWidth * y];
    }

    void DestroyTower ()
    {
        List<GameObject> children = new List<GameObject> ();
        foreach (Transform child in gameObject.transform) {
            children.Add (child.gameObject);
        }

        foreach (GameObject child in children) {
            Object.DestroyImmediate (child);
        }
    }
}
