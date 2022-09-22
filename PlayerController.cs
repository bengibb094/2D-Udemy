using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //player properties
    //Any settings you want to set for your player must be set here
    public float walkSpeed = 10f;
    public float gravity = 20f;
    public float jumpSpeed = 15f;

    //player state handy for making animations in the future
    public bool isJumping;

    //input flags
    private bool _startJump;
    private bool _releaseJump;

    private Vector2 _input;
    private Vector2 _moveDirection;
    private CharacterController2D _characterController;

    // Start is called before the first frame update
    void Start()
    {
        _characterController = gameObject.GetComponent<CharacterController2D>();
    }

    // Update is called once per frame
    void Update()
    {
        _moveDirection.x = _input.x;
        _moveDirection.x *= walkSpeed;

        if (_characterController.below) //On the ground
        {
            isJumping = false;

            //Check if the jump button has been pressed
            if (_startJump)
            {
                _startJump = false;
                _moveDirection.y = jumpSpeed;
                isJumping = true;
                _characterController.DisableGroundCheck();
            }
        }
        else //In the air
        {
            //if you are in the air when the button is released you will only do a small jump
            if (_releaseJump)
            {
                _releaseJump = false;

                if (_moveDirection.y > 0)
                {
                    _moveDirection.y *= 0.5f;
                }

            }

            _moveDirection.y -= gravity * Time.deltaTime;
        }
        

        _characterController.Move(_moveDirection * Time.deltaTime);
    }

    //Input Methods
    public void OnMovement(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
    }

    public void OnJump (InputAction.CallbackContext context)
    {
    //Check what state the button is in
        if (context.started)
        {
            _startJump = true;
        }
        else if (context.canceled)
        {
            _releaseJump = true;
        }
    }
}

