using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float jumpForce;
    public float moveForce;
    public float maxVelocity;
    
    public Transform groundCheck;

    private bool grounded;
    private bool jump;

    // Use this for initialization
    void Start ()
    {
    }
	
    // Update is called once per frame
    void Update ()
    {
        grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));

        if (Input.GetButtonDown("Jump") && grounded) {
            jump = true;
        }
    }

    void FixedUpdate ()
    {

        float h = Input.GetAxis ("Horizontal");

        Animator animator = gameObject.GetComponent<Animator> ();
        animator.SetFloat("Velocity", h);

        if (h * rigidbody2D.velocity.x < maxVelocity) {
            rigidbody2D.AddForce (h * moveForce * Vector2.right);
        }

        if (jump) {
            rigidbody2D.AddForce(Vector2.up * jumpForce);
            jump = false;
        }
    }
}
