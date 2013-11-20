using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(TowerGen))]
public class TowerGenUI : Editor
{
    Tool LastTool = Tool.None;
    Color faceColor;
    Color deadEndFaceColor;
    Color outlineColor;

    public TowerGenUI ()
    {
        outlineColor = Color.grey;
        faceColor = new Color (1f, 1f, 1f, 0.3f);
        deadEndFaceColor = new Color (1f, 0f, 0f, 0.3f);
    }

    void OnEnable ()
    {
        LastTool = Tools.current;
        Tools.current = Tool.None;
    }

    void OnDisable ()
    {
        Tools.current = LastTool;
    }

    public override void OnInspectorGUI ()
    {
        serializedObject.Update ();

        DrawDefaultInspector ();

        TowerGen tower = (TowerGen)target;

        if (tower != null) {
            EditorGUILayout.IntField ("Number of Rooms", tower.rooms.Count);
        }
    }

    public void OnSceneGUI ()
    {
        serializedObject.Update ();

        TowerGen tower = (TowerGen)target;

        if (tower != null) {
            Vector3 bl = tower.transform.position;
            Vector3 br = new Vector3(bl.x + tower.towerWidth * tower.towerParams.blockWidth, bl.y, bl.z);
            Vector3 tl = new Vector3(bl.x, bl.y + tower.towerHeight * tower.towerParams.blockHeight, bl.z);
            Vector3 tr = new Vector3(bl.x + tower.towerWidth * tower.towerParams.blockWidth,
                                     bl.y + tower.towerHeight * tower.towerParams.blockHeight, bl.z);

            Handles.color = Color.white;

            Handles.DrawLine (bl, br);
            Handles.DrawLine (br, tr);
            Handles.DrawLine (tr, tl);
            Handles.DrawLine (tl, bl);

            float connectionLen = tower.towerParams.blockHeight / 4f;
            float z = tower.transform.position.z;

            foreach (TowerGen.Room lhs in tower.rooms) {
                DrawRoomOverlay (tower, lhs);

                HashSet<TowerGen.Room> processedRooms = new HashSet<TowerGen.Room> ();
                foreach (TowerGen.Connection conn in lhs.connections) {
                    if (processedRooms.Contains (conn.room)) {
                        continue;
                    }

                    if(conn.type == TowerGen.ConnectionType.PORTAL) {
                        continue;
                    }

                    float x, y;

                    Handles.color = Color.blue;

                    if (conn.type == TowerGen.ConnectionType.LEFT_WALL || conn.type == TowerGen.ConnectionType.RIGHT_WALL) {
                        x = lhs.x * tower.towerParams.blockWidth;

                        if (conn.type == TowerGen.ConnectionType.RIGHT_WALL) {
                            x += lhs.w * tower.towerParams.blockWidth;
                        }

                        y = tower.towerParams.blockHeight * lhs.y + (conn.placement.x + conn.placement.y) / 2f;

                        Handles.DrawLine(
                            tower.gameObject.transform.position + new Vector3(x - connectionLen, y, z),
                            tower.gameObject.transform.position + new Vector3(x + connectionLen, y, z));
                    } else {
                        y = lhs.y * tower.towerParams.blockHeight;

                        if (conn.type == TowerGen.ConnectionType.CEILING) {
                            y += lhs.h * tower.towerParams.blockHeight;
                        }

                        x = tower.towerParams.blockWidth * lhs.x + (conn.placement.x + conn.placement.y) / 2f;

                        Handles.DrawLine(
                            tower.gameObject.transform.position + new Vector3(x, y - connectionLen, z),
                            tower.gameObject.transform.position + new Vector3(x, y + connectionLen, z));
                    }
                }
                processedRooms.Add (lhs);
            }
        }
    }

    void DrawRoomOverlay (TowerGen t, TowerGen.Room r)
    {
        Handles.color = Color.white;

        Vector3 v1 = new Vector3(t.gameObject.transform.position.x + r.x * t.towerParams.blockWidth,
                                 t.gameObject.transform.position.y + r.y * t.towerParams.blockHeight,
                                 t.gameObject.transform.position.z);

        Vector3 v2 = new Vector3(t.gameObject.transform.position.x + (r.x + r.w) * t.towerParams.blockWidth,
                                 t.gameObject.transform.position.y + r.y * t.towerParams.blockHeight,
                                 t.gameObject.transform.position.z);

        Vector3 v3 = new Vector3(t.gameObject.transform.position.x + (r.x + r.w) * t.towerParams.blockWidth,
                                 t.gameObject.transform.position.y + (r.y + r.h) * t.towerParams.blockHeight,
                                 t.gameObject.transform.position.z);

        Vector3 v4 = new Vector3(t.gameObject.transform.position.x + r.x * t.towerParams.blockWidth,
                                 t.gameObject.transform.position.y + (r.y + r.h) * t.towerParams.blockHeight,
                                 t.gameObject.transform.position.z);

        Color fillColor = faceColor;

        if (r.deadEnd) {
            fillColor = deadEndFaceColor;

            float gb = Mathf.Min (0.8f, r.toEnd * 0.2f);
            fillColor.g = fillColor.b = gb;
        }

        Handles.DrawSolidRectangleWithOutline (new Vector3[] {v1, v2, v3, v4}, fillColor, outlineColor);
    }
}
