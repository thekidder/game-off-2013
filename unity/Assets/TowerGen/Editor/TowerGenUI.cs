using UnityEngine;
using UnityEditor;
using System.Collections;

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

            float connectionLen = tower.blockHeight  / 4f;

            for (int i = 0; i < tower.towerWidth; ++i) {
                for (int j = 0; j < tower.towerHeight - 1; ++j) {
                    if (!tower.verticalConnections[i * (tower.towerHeight - 1) + j]) {
                        continue;
                    }

                    float x = i * tower.blockWidth + tower.blockWidth / 2f + tower.transform.position.x;
                    float y = (j + 1) * tower.blockHeight + tower.transform.position.y;
                    float z = tower.transform.position.z;

                    Handles.DrawLine(new Vector3(x, y - connectionLen, z), new Vector3(x, y + connectionLen, z));
                }
            }

            for (int i = 0; i < tower.towerWidth - 1; ++i) {
                for (int j = 0; j < tower.towerHeight; ++j) {
                    if (!tower.horizontalConnections[i + j * (tower.towerWidth - 1)]) {
                        continue;
                    }

                    float x = (i + 1) * tower.blockWidth + tower.transform.position.x;
                    float y = j * tower.blockHeight + tower.blockHeight / 2f + tower.transform.position.y;
                    float z = tower.transform.position.z;

                    Handles.DrawLine(new Vector3(x - connectionLen, y, z), new Vector3(x + connectionLen, y, z));
                }
            }
        }
    }
}
