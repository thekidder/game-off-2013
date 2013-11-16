using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(TowerGen))]
public class TowerGenUI : Editor
{
    public void OnSceneGUI ()
    {
        TowerGen tower = (TowerGen)target;

        if (tower) {
            Vector3 bl = tower.transform.position;
            Vector3 br = new Vector3 (bl.x + tower.towerWidth * tower.blockWidth, bl.y, bl.z);
            Vector3 tl = new Vector3 (bl.x, bl.y + tower.towerHeight * tower.blockHeight, bl.z);
            Vector3 tr = new Vector3 (bl.x + tower.towerWidth * tower.blockWidth, 
			                         bl.y + tower.towerHeight * tower.blockHeight, bl.z);

            Handles.color = Color.white;

            Handles.DrawLine (bl, br);
            Handles.DrawLine (br, tr);
            Handles.DrawLine (tr, tl);
            Handles.DrawLine (tl, bl);

            Handles.color = Color.red;

            float connectionLen = tower.blockHeight / 4f;
            float z = tower.transform.position.z;

            foreach (TowerGen.Room lhs in tower.rooms) {
                HashSet<TowerGen.Room> processedRooms = new HashSet<TowerGen.Room> ();
                foreach (TowerGen.Room rhs in lhs.connections) {
                    if (processedRooms.Contains (rhs)) {
                        continue;
                    }

                    Vector2 hEdge = tower.SharedHorizontalEdge (lhs, rhs);
                    Vector2 vEdge = tower.SharedVerticalEdge (lhs, rhs);

                    float x = (hEdge.x + hEdge.y) / 2f;
                    float y = (vEdge.x + vEdge.y) / 2f;

                    if (Mathf.Approximately (hEdge.x, hEdge.y)) {
                        Handles.DrawLine (new Vector3 (x - connectionLen, y, z), new Vector3 (x + connectionLen, y, z));
                    } else {
                        Handles.DrawLine (new Vector3 (x, y - connectionLen, z), new Vector3 (x, y + connectionLen, z));
                    }
                }
                processedRooms.Add (lhs);
            }
        }
    }
}
