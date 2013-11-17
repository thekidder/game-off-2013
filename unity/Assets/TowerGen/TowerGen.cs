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
    public int numDeadEnds;
    public float connectedness;
    public float sparseness;
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
        public bool deadEnd = false;
        public int toEnd = -1;
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

        connectedness = 0f;
        sparseness = 0f;

        towerWidth = Random.Range (8, 12);
        towerHeight = Random.Range (14, 22);

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
        FloodFill ();
        RemoveAllUnconnected ();

        numRoomsMin = (int)Random.Range (rooms.Count * 0.5f, rooms.Count * 0.6f);
        float maxConnectedness = Random.Range (0.45f, 0.55f);

        //Debug.Log ("Rooms before removing connections: " + rooms.Count);

        int maxIterations = rooms.Count * 8;
        connectedness = 1f;
        for (int i =0; connectedness > maxConnectedness && i < maxIterations; ++i) {
            Room r = rooms [Random.Range (0, rooms.Count)];

            if (r.connections.Count == 0)
                continue;

            Room o = r.connections [Random.Range (0, r.connections.Count)];
            RemoveConnection (r, o);

            int numRooms = FloodFill ();

            //Debug.Log ("Now have " + numRooms + " rooms");

            if (numRooms < numRoomsMin) {
                AddConnection (r, o);
                FloodFill ();
            }

            RemoveAllUnconnected ();
            connectedness = Connectedness ();
        }

        numDeadEnds = DeadEnds ();

        int maxDeadEnds = (int)(rooms.Count * 0.15f);

        for (int i = 0; i < maxIterations && numDeadEnds > maxDeadEnds; ++i) {
            rooms.Shuffle ();
            foreach (Room r in rooms) {
                if (r.connections.Count == 1) {
                    List<Room> potential = AdjacentRooms (r);

                    if (potential.Count == 1)
                        continue;

                    potential.Shuffle ();

                    foreach (Room c in potential) {
                        if (c != r.connections [0]) {
                            AddConnection (c, r);
                            numDeadEnds = DeadEnds ();
                            break;
                        }
                    }

                    break;
                }
            }
        }

        connectedness = Connectedness ();

        sparseness = Sparseness ();
    }

    void ResetDeadEnds ()
    {
        foreach (Room r in rooms) {
            r.deadEnd = false;
            r.toEnd = -1;
        }
    }

    int DeadEnds ()
    {
        ResetDeadEnds ();

        int n = 0;
        
        // do some end end connections
        foreach (Room r in rooms) {
            if (r.connections.Count == 1 && (r.x != 0 || r.y != 0)) {
                ++n;
                WalkDeadEnd (r, r, 0);
            }
        }
        return n;
    }

    void WalkDeadEnd (Room r, Room prev, int currentCount)
    {
        if (r.connections.Count > 2 || (r.x == 0 && r.y == 0)) {
            return;
        }

        r.deadEnd = true;
        r.toEnd = currentCount;

        foreach (Room next in r.connections) {
            if (next == prev) {
                continue;
            }

            WalkDeadEnd (next, r, currentCount + 1);
        }
    }
    
    float Sparseness ()
    {
        int w = 0, h = 0, filled = 0;
        for (int i = 0; i < towerWidth; ++i) {
            for (int j = 0; j < towerHeight; ++j) {
                if (GetSquare (i, j).room != null) {
                    if (i > w) {
                        w = i;
                    }
                    if (j > h) {
                        h = j;
                    }
                    filled++;
                }
            }
        }
        
        return (float)filled / ((w + 1) * (h + 1));
    }

    float Connectedness ()
    {
        float c = 0f;
        foreach (Room r in rooms) {
            c += (float)r.connections.Count / AdjacentRooms (r).Count;
        }
        c /= rooms.Count;
        return c;
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

    void RemoveAllUnconnected ()
    {
        for (int i = rooms.Count - 1; i >= 0; --i) {
            Room r = rooms [i];
            if (!r.connected) {
                rooms.RemoveAt (i);
                RemoveRoom (r);
            }
        }
    }

    void AddConnection (Room lhs, Room rhs)
    {
        lhs.connections.Add (rhs);
        rhs.connections.Add (lhs);
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
    }

    void ResetConnected ()
    {
        foreach (Room r in rooms) {
            r.connected = false;
        }
    }

    int FloodFill ()
    {
        ResetConnected ();
        return SetConnected (GetSquare (0, 0).room, 1);
    }

    int SetConnected (Room r, int num)
    {
        r.connected = true;

        foreach (Room adj in r.connections) {
            if (!adj.connected)
                num = SetConnected (adj, num + 1);
        }
        return num;
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
