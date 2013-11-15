using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class TowerGen : MonoBehaviour
{
    public float blockWidth = 32f;
    public float blockHeight = 32f;

    public int towerHeight;
    public int towerWidth;

    public Seed seed;
    public GameObject block;

    public List<GridSquare> grid;
    public List<bool> verticalConnections;
    public List<bool> horizontalConnections;

    public class GridSquare
    {
        public GridSquare ()
        {
            connected = false;
            filled = false;
        }

        public bool connected;
        public bool filled;
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

        verticalConnections = new List<bool>();
        for (int i = 0; i < (towerHeight - 1) * towerWidth; ++i) {
            verticalConnections.Add(false);
        }

        horizontalConnections = new List<bool>();
        for (int i = 0; i < (towerWidth - 1) * towerHeight; ++i) {
            horizontalConnections.Add(false);
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

        // flood fill to ensure tower is connected
        setConnected (grid, 0, 0);

        // build connections
        for (int i = 0; i < towerWidth; ++i) {
            for (int j = 0; j < towerHeight; ++j) {
                GridSquare sq = GetSquare (i, j);

                if (i < towerWidth - 1) {
                    // horizontal connection
                    GridSquare connection = GetSquare(i + 1, j);

                    if (sq.filled && sq.connected && connection.filled && connection.connected) {
                        horizontalConnections[i + j * (towerWidth - 1)] = true;
                    }
                }

                if (j < towerHeight - 1) {
                    // vertical connection
                    GridSquare connection = GetSquare(i, j + 1);

                    if (sq.filled && sq.connected && connection.filled && connection.connected) {
                        verticalConnections[i * (towerHeight - 1) + j] = true;
                    }
                }
            }
        }

        for (int i = 0; i < towerWidth; ++i) {
            for (int j = 0; j < towerHeight; ++j) {
                GridSquare square = GetSquare (i, j);
                if (!square.filled || !square.connected) {
                    continue;
                }

                Vector3 pos = new Vector3 (i * blockWidth, j * blockHeight, 
                                           this.gameObject.transform.position.z);
                GameObject newBlock = (GameObject)Instantiate (block, pos, Quaternion.identity);
                newBlock.transform.localScale = new Vector3 (blockWidth, blockHeight, 1f);
                
                newBlock.transform.parent = gameObject.transform;
            }
        }
    }

    void setConnected (List<GridSquare> grid, int x, int y)
    {
        if (!InRange (x, y))
            return;
         
        GridSquare sq = GetSquare (x, y);

        if (sq.connected)
            return;

        if (!sq.filled)
            return;


        sq.connected = true;

        setConnected (grid, x - 1, y);
        setConnected (grid, x + 1, y);
        setConnected (grid, x, y - 1);
        setConnected (grid, x, y + 1);
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
