using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class TowerGen : MonoBehaviour
{
    public Seed seed;

    // Use this for initialization
    void Start ()
    {

    }

    // Update is called once per frame
    void Update ()
    {
        if (seed.Changed ()) {
            Debug.Log ("new seed!");
            seed.Sync ();
        }
    }
}
