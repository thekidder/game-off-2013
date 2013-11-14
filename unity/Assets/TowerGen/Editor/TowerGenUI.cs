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

            Handles.DrawLine (bl, br);
            Handles.DrawLine (br, tr);
            Handles.DrawLine (tr, tl);
            Handles.DrawLine (tl, bl);
        }
    }
}
