using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using GlobalTypes;


public class PlayerController : MonoBehaviour
{

#region player properties
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
    public float glideTime = 2f;
    public float glideDesentAmount = 2f;
    public float powerJumpSpeed = 50f;
    public float powerJumpWaitTime = 1.5f;
    public float dashSpeed = 20f;
    public float dashTime = 0.2f;
    public float dashCoolDownTime = 1f;
    public float groundSlamSpeed = 60f;
#endregion

#region player abilities
    //player ability toggles - can be turned on and off
    [Header("Player Abilities")]
    public bool canDoubleJump;
    public bool canTripleJump; 
    public bool canWallJump;
    public bool canJumpAfterWallJump;
    public bool canWallRun; 
    public bool canMultipleWallRun;
    public bool canWallSlide;
    public bool canGlide;
    public bool canGlideAfterWallContact; 
    public bool canPowerJump;
    public bool canGroundDash;
    public bool canAirDash;
    public bool canGroundSlam;
#endregion

#region player state
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
    public bool isGliding;
    public bool isPowerJumping;
    public bool isDashing;
    public bool isGroundSlamming;
#endregion

#region private properties
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

    private float _currentGlideTime;//Time the player can glide in the air
    private bool _startGlide = true;

    private float _powerJumpTimer;

    private bool _facingRight;
    private float _dashTimer;
#endregion

// Start is called before the first frame update
#region void start
void Start()
{
    _characterController = gameObject.GetComponent<CharacterController2D>();
    _capsuleCollider = gameObject.GetComponent<CapsuleCollider2D>();
    _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    _originalColliderSize = _capsuleCollider.size; 
}
#endregion

// Update is called once per frame
#region void update
void Update()
{
    //Setting thr dash timer
    if (_dashTimer > 0)
    {
        _dashTimer -= Time.deltaTime;
    }
        
    ProcessHorizontalMovement();
    //Overidng the player input so if the player is wall jumping

    if (_characterController.below) //On the ground
    {
        onGround();
    }
    else //In the air
    {
        inAir();

    }
        

    _characterController.Move(_moveDirection * Time.deltaTime);
}
#endregion

#region ProcessHorizontalMovement
    private void ProcessHorizontalMovement()
    {
                if (!iswallJumping)
        {
            _moveDirection.x = _input.x;
            

            //Handling the rotation of the player in the game
            if(_moveDirection.x < 0)
            {
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                _facingRight = false;
            }   
            else if (_moveDirection.x > 0)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                _facingRight = true;
            
            }

            //if isdashing is true and the player is facing right then move them the dash speed
            if (isDashing)
            {
                if (_facingRight)
                {
                    _moveDirection.x = dashSpeed;
                }
                //if they are not facing right move them the dashspeed left
                else
                {
                    _moveDirection.x = -dashSpeed;
                }
                _moveDirection.y = 0f;

            }
            else if (isCreeping)
            {
                _moveDirection.x *= creepSpeed; 
            }
            //else just move the normal walking speed.
            else
            {
                _moveDirection.x *= walkSpeed;
            }


        }
    }
    #endregion

//Will call from the update look
#region onTheGround
    void onGround()
    {
        _moveDirection.y = 0f; //if the character is on the ground set the gravity back to zero

            ClearAirAbilityFlags();

            Jump();

            DuckingCreeping();            

            
    }
    #endregion

//Clear air abilities
#region ClearAirAbilities
    private void ClearAirAbilityFlags()
    {
        isJumping = false;
        isDoubelJumping = false;//if the player is on the ground set the doublejump to false
        isTripleJumping = false; 
        iswallJumping = false;
        _currentGlideTime = glideTime;
        isGroundSlamming = false;
        _startGlide = true;

    }
    #endregion

//Ducking and Creeping controls
#region DuckingCreeping
    private void DuckingCreeping()
    {
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
                    _powerJumpTimer += Time.deltaTime;//When the player is ducking they must duck for the PowerJumpTimer amount of time


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

                _powerJumpTimer = 0f;
            }

            if (isDucking && _moveDirection.x != 0)
            {
                isCreeping = true;
                _powerJumpTimer = 0f;
            }
            else 
            {
                isCreeping = false;
            }

    }
    #endregion

//Jumping Controls
#region Jumping
    private void Jump()
    {
        if (_startJump)
            {
                _startJump = false;

                //PowerJumping
                if (canPowerJump && isDucking && _characterController.groundType != GlobalTypes.GroundType.OneWayPlatform && (_powerJumpTimer > powerJumpWaitTime))
                {
                    _moveDirection.y = powerJumpSpeed;
                    StartCoroutine("PowerJumpWaiter");
                }
                //Check to see if we are on a one way platform.
                else if (isDucking && _characterController.groundType == GroundType.OneWayPlatform)
                {
                    StartCoroutine(DisableOneWayPlatform(true));


                }
                else
                {
                    _moveDirection.y = jumpSpeed;

                }

                isJumping = true;
                _characterController.DisableGroundCheck();
                _ableToWallRun = true;
                _currentGlideTime = glideTime;//Resetting the glide time value so it can be used again.
                isGroundSlamming = false;
            }
    }
    #endregion
    
//Will call from the update look
//In the air controls
#region intheair
    void inAir()
    {

        ClearGroundAbilityflags();

        AirJump();

        Wallrunning();

        GravityCalculations();

    }

    
    #endregion

#region voids
    private void ClearGroundAbilityflags()
        {
            if ((isDucking || isCreeping) && _moveDirection.y > 0)
            {
                StartCoroutine("ClearDuckingState");
            }

            //clear power jump timer
            _powerJumpTimer = 0f;
        }

    private void Wallrunning()
        {
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

            //CanGlideAfterWallContact
            if ((_characterController.left || _characterController.right) && canWallRun)
            {
                if (canGlideAfterWallContact)
                {
                    _currentGlideTime = glideTime;
                }
                else 
                {
                    _currentGlideTime = 0;
                }
         
            } 
        }

    private void AirJump()
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
        }

    void GravityCalculations()
    {
        //if we are jumping and we hit something drop right back down
        if (_moveDirection.y > 0f && _characterController.above)
        {
            if (_characterController.ceilingType == GroundType.OneWayPlatform)//if groundType equals OneWayPlatform then start the coroutine we created to check and disable the platform for a few seconds.
            {
                StartCoroutine(DisableOneWayPlatform(false));
            }
            else
            {
                _moveDirection.y = 0f;
            }
            
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
        else if (canGlide && _input.y > 0f && _moveDirection.y < 0.2f)//glide adjustment. canGlide is true there is up input on the y axis and we are moving down for .2 on the y axis as well. Small delay.
        {
            if (_currentGlideTime > 0f)
            {
                isGliding = true;

                if (_startGlide)
                {
                    _moveDirection.y = 0;
                    _startGlide = false;
                }

                _moveDirection.y -= glideDesentAmount * Time.deltaTime;
                _currentGlideTime -= Time.deltaTime;
            }
            else
            {
                isGliding = false; 
                _moveDirection.y -= gravity * Time.deltaTime;
            }
            

        }
        //else if (canGroundSlam && !isPowerJumping && _input.y < 0f && _moveDirection.y < 0f)//For Ground Slam
        else if (isGroundSlamming && !isPowerJumping && _moveDirection.y < 0f)
        {
            _moveDirection.y = -groundSlamSpeed;
        }
        else if (!isDashing)//normal gravity
        {
            _moveDirection.y -= gravity * Time.deltaTime;

        }
        

    }
#endregion
    
//Input Methods
//Input for a new button press must be done with a new public void

#region onMovement
    public void OnMovement(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
    }
#endregion

#region onJump
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
#endregion

#region onAttack
    //Attack Button
    public void onAttack(InputAction.CallbackContext context)
    {
        if (context.performed && _input.y < 0f)
        {
            if (canGroundSlam)
            {
                isGroundSlamming = true;
            }
        }
    }
#endregion

#region onDash
    public void onDash (InputAction.CallbackContext context)
    {   
        //if the button press is started
        if (context.started && _dashTimer <= 0)
        {
            if ((canAirDash && !_characterController.below) || (canGroundDash && _characterController.below))
            {
                StartCoroutine("Dash");
            }
        }
    }

#endregion

#region Couroutines
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

    IEnumerator PowerJumpWaiter()
    {
        isPowerJumping = true;
        yield return new WaitForSeconds(0.8f);
        isPowerJumping = false;
    }

    IEnumerator Dash()
    {
        isDashing = true;
        yield return new WaitForSeconds(dashTime);
        isDashing = false;
        _dashTimer = dashCoolDownTime;
    }

    IEnumerator DisableOneWayPlatform(bool checkBelow)
    {
        bool originalCanGroundSlam = canGroundSlam;
        GameObject tempOneWayPlatform = null; 
        //GameObject = tempOneWayPlatform = null;

        if (checkBelow)
        {
            Vector2 raycastBelow = transform.position - new Vector3(0, _capsuleCollider.size.y * 0.5f,0);//Creating a new raycast to check below the player on the y axis 
            RaycastHit2D hit = Physics2D.Raycast(raycastBelow, Vector2.down, _characterController.raycastDistance, _characterController.layerMask);//Doing the raycast
            if (hit.collider)
            {
                tempOneWayPlatform = hit.collider.gameObject;//If the raycast hit hits anything tempOneWayPlatform becomes Null.
            }
        }
        else
        {
            Vector2 raycastAbove = transform.position + new Vector3(0, _capsuleCollider.size.y * 0.5f,0);//Creating a new raycast to check above the player on the y axis 
            RaycastHit2D hit = Physics2D.Raycast(raycastAbove, Vector2.up, _characterController.raycastDistance, _characterController.layerMask);//Doing the raycast
            if (hit.collider)
            {
                tempOneWayPlatform = hit.collider.gameObject;//If the raycast hit hits anything tempOneWayPlatform becomes Null.
            }
        }
        //if tempOneWayPlatform is true disbale the edge colider and can ground slam for point 25f to give the player time to pass through.
        if (tempOneWayPlatform)
        {
            tempOneWayPlatform.GetComponent<EdgeCollider2D>().enabled = false; 
            canGroundSlam = false;
        }

        yield return new WaitForSeconds(0.25f);
        //After waiting point 25f re enable the edge collider.
        if (tempOneWayPlatform)
        {
            tempOneWayPlatform.GetComponent<EdgeCollider2D>().enabled = true;
            canGroundSlam = originalCanGroundSlam;
        }
    }


#endregion

}

