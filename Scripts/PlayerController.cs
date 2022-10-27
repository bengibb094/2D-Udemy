using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //player properties
    //Any settings you want to set for your player must be set here
    [Header("Player Properties")]
    public float walkSpeed = 10f;
    public float gravity = 20f;
    public float jumpSpeed = 15f;
    public float doubleJumpSpeed = 10f;
    public float tripleJumpSpeed = 10f; 
    public float xWallJumpSpeed = 15f;
    public float yWallJumpSpeed = 15f;
    public float wallRunAmount = 8f;
    public float wallSlideAmount = 0.2f; 
    public float creepSpeed = 5f;


    //player ability toggles - can be turned on and off
    [Header("Player Abilities")]
    public bool  canDoubleJump;
    public bool canTripleJump; 
    public bool canWallJump;
    public bool canJumpAfterWallJump;
    public bool canWallRun; 
    public bool canMultipleWallRun;
    public bool canWallSlide;

    //player state handy for making animations in the future
    [Header("Player State")]
    public bool isJumping;
    public bool isDoubelJumping;
    public bool isTripleJumping; 
    public bool iswallJumping; 
    public bool isWallRunning;
    public bool isWallSliding;
    public bool isDucking;
    public bool isCreeping;

    //input flags
    private bool _startJump;
    private bool _releaseJump;

    private Vector2 _input;
    private Vector2 _moveDirection;
    private CharacterController2D _characterController;

    private bool _ableToWallRun = true;

    private CapsuleCollider2D _capsuleCollider;
    private Vector2 _originalColliderSize;
    //Remove later when not needed
    private SpriteRenderer _spriteRenderer; //Allows us to change sprite midgame


    // Start is called before the first frame update
    void Start()
    {
        _characterController = gameObject.GetComponent<CharacterController2D>();
        _capsuleCollider = gameObject.GetComponent<CapsuleCollider2D>();
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        _originalColliderSize = _capsuleCollider.size; 
    }

    // Update is called once per frame
    void Update()
    {
        //Overidng the player input so if the player is wall jumping
        if (!iswallJumping)
        {
            _moveDirection.x = _input.x;
            _moveDirection.x *= walkSpeed;

            //Handling the rotation of the player in the game
            if(_moveDirection.x < 0)
            {
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }   
            else if (_moveDirection.x > 0)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            
            }

        }




        Debug.Log(_moveDirection.y);

        if (_characterController.below) //On the ground
        {
            _moveDirection.y = 0f; //if the character is on the ground set the gravity back to zero
            isJumping = false;
            isDoubelJumping = false;//if the player is on the ground set the doublejump to false
            isTripleJumping = false; 
            iswallJumping = false;

            //Check if the jump button has been pressed
            if (_startJump)
            {
                _startJump = false;
                _moveDirection.y = jumpSpeed;
                isJumping = true;
                _characterController.DisableGroundCheck();
                _ableToWallRun = true;
            }

            //Ducking and Creeping
            if (_input.y < 0f)//_input is a vector2 so it's y postion can be manipulated
            {
                if (!isDucking && !isCreeping)
                {
                    _capsuleCollider.size = new Vector2(_capsuleCollider.size.x, _capsuleCollider.size.y /2);//new vector 2 _capsuleCollider same size on the x but half the size on the y
                    transform.position = new Vector2(transform.position.x, transform.position.y - (_originalColliderSize.y /4));//same as above but its position is same on the x but quartered on the y
                    isDucking = true;
                    _spriteRenderer.sprite = Resources.Load<Sprite>("directionSpriteUp_crouching");//Allows us to swap out sprites from our resources folder.
                }

            }
            else
            {   //if there is no y input and isDucking or isCreeping is true then change the sprite back
                if (isDucking || isCreeping)
                {
                    //Raycast to check if there is anything double or crouched height above us.
                    RaycastHit2D hitCeiling = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, transform.localScale, CapsuleDirection2D.Vertical, 0f, Vector2.up, _originalColliderSize.y / 2, _characterController.layerMask);
                    if (!hitCeiling.collider)
                    {
                        _capsuleCollider.size = _originalColliderSize;
                        transform.position = new Vector2(transform.position.x, transform.position.y + (_originalColliderSize.y /4));
                        _spriteRenderer.sprite = Resources.Load<Sprite>("directionSpriteUp");
                        isDucking= false;
                        isCreeping = false;

                    }

                }
            }

            if (isDucking && _moveDirection.x != 0)
            {
                isCreeping = true;
            }
            else 
            {
                isCreeping = false;
            }
        }
        else //In the air
        {

            if ((isDucking || isCreeping) && _moveDirection.y > 0)
            {
                StartCoroutine("ClearDuckingState");
            }

            //if you are in the air when the button is released you will only do a small jump
            if (_releaseJump)
            {
                _releaseJump = false;

                if (_moveDirection.y > 0)
                {
                    _moveDirection.y *= 0.5f;
                }

            }

            //pressed jump button in air
            /*first check if the jump button has been pressed a second time.
            Check if the player can doublejump in the first place and check if there is anything to their immediate left or right.
            Also don't forget to reset start jump to false. */
            if (_startJump)
            {
                //Triple Jump
                if (canDoubleJump && (!_characterController.left && !_characterController.right))
                {
                    if (isDoubelJumping && !isTripleJumping)
                    {
                        _moveDirection.y = tripleJumpSpeed;
                        isTripleJumping = true;
                    }
                }

                //DoubleJump
                if (canDoubleJump && (!_characterController.left && !_characterController.right))
                {
                    //cheks the player is not already double jumping and if they aren't it will do the double jump.
                    if (!isDoubelJumping)
                    {
                        _moveDirection.y = doubleJumpSpeed;
                        isDoubelJumping = true;
                    }

                }
                //Wall Jumping
                if (canWallJump && (_characterController.left || _characterController.right))
                {
                
                if (_moveDirection.x <= 0 && _characterController.left)
                {
                    _moveDirection.x = xWallJumpSpeed;
                    _moveDirection.y = yWallJumpSpeed;
                    transform.rotation = Quaternion.Euler(0f, 0f, 0f);

                }
                else if (_moveDirection.x >= 0 && _characterController.right)
                {
                    _moveDirection.x = -xWallJumpSpeed;
                    _moveDirection.y = yWallJumpSpeed;
                    transform.rotation = Quaternion.Euler(0f, 180f, 0f);

                }

                iswallJumping = true;

                StartCoroutine("WallJumpingWaiter");

                if (canJumpAfterWallJump)
                {
                    isDoubelJumping = false;
                    isTripleJumping = false;
                }

                }
                



               _startJump = false; 
            }

            //wall running
            //Check is the ability on and is there actually a wall to our left or right
            if (canWallRun && (_characterController.left || _characterController.right))
            {
                if (_input.y > 0 && _ableToWallRun)//if the move up input is being pressed and ableToWallRun is true then wall run
                {
                    _moveDirection.y = wallRunAmount;

                   if (_characterController.left)
                    {
                        transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                    }
                    else if (_characterController.right)
                    {
                        transform.rotation = Quaternion.Euler(0f, 0f, 0f);

                    }

                    StartCoroutine ("wallRunWaiter");

                }
            }
            else
            {
                if (canMultipleWallRun)
                {
                    StopCoroutine("WallRunWaiter");
                    _ableToWallRun = true;
                    isWallRunning = false;
                }
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
//Setting the gravity to affect the player when they are moving down a wall
        if (canWallSlide && (_characterController.left || _characterController.right))//if the ability is enabled and there is something to the right or left then the gravity is affected
        {
            if (_characterController.hitWallThisFrame)
            {
                _moveDirection.y =0f;

            }

            if (_moveDirection.y <= 0)
            {
                _moveDirection.y -= (gravity * wallSlideAmount) * Time.deltaTime; 

            }
            else
            {
                _moveDirection.y -= gravity * Time.deltaTime;

            }
        }
        else
        {
            _moveDirection.y -= gravity * Time.deltaTime;

        }
        

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

    //coroutines

    //Waits .4 of a second then sets the walljumping state to false
    IEnumerator WallJumpingWaiter()
    {
        iswallJumping = true;
        yield return new WaitForSeconds(0.4f);
        iswallJumping = false;
    }

    IEnumerator wallRunWaiter()
    {
        isWallRunning = true;
        yield return new WaitForSeconds(0.5f);
        isWallRunning = false;
        if (!iswallJumping)
        {
            _ableToWallRun = false;
        }
    }

    IEnumerator ClearDuckingState()
    {
        yield return new WaitForSeconds(0.05f);
        RaycastHit2D hitCeiling = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, transform.localScale, CapsuleDirection2D.Vertical, 0f, Vector2.up, _originalColliderSize.y / 2, _characterController.layerMask);

        if (!hitCeiling.collider)
        {
            _capsuleCollider.size = _originalColliderSize;
            //transform.position = new Vector2(transform.position.x, transform.position.y + (_originalColliderSize.y /4));
            _spriteRenderer.sprite = Resources.Load<Sprite>("directionSpriteUp");
            isDucking= false;
            isCreeping = false;
        }

    }
}

