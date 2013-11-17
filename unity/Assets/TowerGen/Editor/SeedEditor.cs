using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Seed))]
public class SeedEditor : PropertyDrawer
{
    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty (position, label, property);

        position = EditorGUI.PrefixLabel (position, GUIUtility.GetControlID (FocusType.Passive), label);

        Rect fieldRect = new Rect (position.x, position.y, position.width - 82 - 62, position.height);
        Rect regenRect = new Rect (position.x + position.width - 80, position.y, 80, position.height);
        Rect rebuildRect = new Rect (position.x + position.width - 82 - 60, position.y, 60, position.height);

        property.FindPropertyRelative ("seed").intValue = EditorGUI.IntField (fieldRect, property.FindPropertyRelative ("seed").intValue);

        if (GUI.Button(rebuildRect, "Rebuild")) {
            GUI.FocusControl(null);
            property.FindPropertyRelative("oldSeed").intValue = 0;
        }

        if (GUI.Button (regenRect, "Randomize")) {
            GUI.FocusControl (null);
            property.FindPropertyRelative ("seed").intValue = Random.Range (0, System.Int32.MaxValue);
        }

        EditorGUI.EndProperty ();
    }
}
