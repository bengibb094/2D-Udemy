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
    public float doubleJumpSpeed = 10f;
    public float tripleJumpSpeed = 10f; 

    //player ability toggles - can be turned on and off
    public bool  canDoubleJump;
    public bool canTripleJump; 

    //player state handy for making animations in the future
    public bool isJumping;
    public bool isDoubelJumping;
    public bool isTripleJumping; 

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

//Handling the rotation of the player in the game
        if(_moveDirection.x < 0)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }else if (_moveDirection.x > 0){
             transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            
        }

        Debug.Log(_moveDirection.y);

        if (_characterController.below) //On the ground
        {
            _moveDirection.y = 0f; //if the character is on the ground set the gravity back to zero
            isJumping = false;
            isDoubelJumping = false;//if the player is on the ground set the doublejump to false
            isTripleJumping = false; 

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

            //doubleJumping 
            /*first check if the jump button has been pressed a second time.
            Check if the player can doublejump in the first place and check if there is anything to their immediate left or right.
            Also don't forget to reset start jump to false. */
            if (_startJump)
            {
                
                if (canDoubleJump && (!_characterController.left && !_characterController.right))
                {
                    if (isDoubelJumping && !isTripleJumping)
                    {
                        _moveDirection.y = tripleJumpSpeed;
                        isTripleJumping = true;
                    }
                }


                if (canDoubleJump && (!_characterController.left && !_characterController.right))
                {
                    //cheks the player is not already double jumping and if they aren't it will do the double jump.
                    if (!isDoubelJumping)
                    {
                        _moveDirection.y = doubleJumpSpeed;
                        isDoubelJumping = true;
                    }

                }
                



               _startJump = false; 
            }

            GravityCalculations();
        }
        

        _characterController.Move(_moveDirection * Time.deltaTime);
    }

    void GravityCalculations()
    {
        //if we are jumping and we hit something drop right back down
        if (_moveDirection.y > 0f && _characterController.above)
        {
            _moveDirection.y = 0f;
        }
        
        _moveDirection.y -= gravity * Time.deltaTime;

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
            _releaseJump = false;
        }
        else if (context.canceled)
        {
            _releaseJump = true;
            _startJump = false; 
        }
    }
}

