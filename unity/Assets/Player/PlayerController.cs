using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float moveForce;
    public float maxVelocity;

    private GameObject body;

    // Use this for initialization
    void Start ()
    {
    }
	
    // Update is called once per frame
    void Update ()
    {
		
    }

    void FixedUpdate ()
    {
        Rigidbody2D rigidBody = GetComponent<Rigidbody2D> ();

        float h = Input.GetAxis ("Horizontal");
        if (h * rigidBody.velocity.x < maxVelocity) {
            rigidBody.AddForce (h * moveForce * Vector2.right);
        }

        if (rigidBody.velocity.x > 0.1) {
            Animator animator = gameObject.GetComponent<Animator> ();
        }
    }
}
