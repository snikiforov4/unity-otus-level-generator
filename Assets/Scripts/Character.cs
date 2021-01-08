using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Character : MonoBehaviour
{
    #region Params
    public Transform visual;
    public GameObject bloodStream;
    public float moveForce;
    public float jumpForce;

    Rigidbody2D rigidBody2D;
    TriggerDetector triggerDetector;
    Animator animator;
    float visualDirection;
    InputAction moveAction;
    bool jumpPressed; 

    public static Character I { get;private set; }
    #endregion

    public virtual void Start()
    {
        I = this;
        InitComponents();
        var playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        InputAction jumpAction = playerInput.actions["Jump"];
        jumpAction.performed += (context) => { jumpPressed = true; };
        jumpAction.canceled += (context) => { jumpPressed = false; };
    }

    protected void InitComponents()
    {
        visualDirection = 1.0f;
        rigidBody2D = GetComponent<Rigidbody2D>();
        triggerDetector = GetComponentInChildren<TriggerDetector>();
        animator = GetComponentInChildren<Animator>();
    }

    /*
    void OnMove(InputValue value)
    {
        Vector2 dir = value.Get<Vector2>();
        rigidBody2D.AddForce(new Vector2(moveForce * dir.x, 0.0f), ForceMode2D.Impulse);
    }

    void OnJump(InputValue value)
    {
        if (triggerDetector.inTrigger)
            rigidBody2D.AddForce(new Vector2(0.0f, jumpForce), ForceMode2D.Impulse);
    }
    */

    /*
    public void MoveLeft()
    {
        rigidBody2D.AddForce(new Vector2(-moveForce, 0.0f), ForceMode2D.Impulse);
    }

    public void MoveRight()
    {
        rigidBody2D.AddForce(new Vector2(moveForce, 0.0f), ForceMode2D.Impulse);
    }

    public void Jump()
    {
        if (triggerDetector.inTrigger)
            rigidBody2D.AddForce(new Vector2(0.0f, jumpForce), ForceMode2D.Impulse);
    }
    */

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.GetComponent<MovingPlatform>() != null)
            transform.SetParent(collision.transform);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.GetComponent<MovingPlatform>() != null)
            transform.SetParent(null);
    }

    // Update is called once per frame
    void Update()
    {
        /*
        float axis = Input.GetAxis("Horizontal");
        if (axis < 0.0f) //Input.GetKey(KeyCode.LeftArrow))
            MoveLeft();
        if (axis > 0.0f) //Input.GetKey(KeyCode.RightArrow))
            MoveRight();
        if (Input.GetButton("Jump")) //Input.GetKey(KeyCode.Space))
            Jump();
        */

        Vector2 dir = moveAction.ReadValue<Vector2>();
        if (!Mathf.Approximately(dir.x, 0.0f) || !Mathf.Approximately(dir.y, 0.0f))
            rigidBody2D.AddForce(new Vector2(moveForce * dir.x, 0.0f), ForceMode2D.Impulse);

        if (jumpPressed) {
            if (triggerDetector.inTrigger)
                rigidBody2D.AddForce(new Vector2(0.0f, jumpForce), ForceMode2D.Impulse);
        }

        float velocity = rigidBody2D.velocity.x;

        if (velocity < -0.1f) {
            visualDirection = -1.0f;
        } else if (velocity > 0.1f) {
            visualDirection = 1.0f;
        }

        Vector3 scale = visual.localScale;
        scale.x = visualDirection;
        visual.localScale = scale;

        animator.SetFloat("speed", Mathf.Abs(velocity));
    }
}
