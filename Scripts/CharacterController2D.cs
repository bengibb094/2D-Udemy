using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalTypes;

public class CharacterController2D : MonoBehaviour
{
    public float raycastDistance = 0.2f;
    public LayerMask layerMask;
    public float slopeAngleLimit = 45f;
    public float downForceAdjustment = 1.2f; 

    //flags
    public bool below;
    public bool left;
    public bool right;
    public bool above;
    public bool hitGroundThisFrame; 
    public bool hitWallThisFrame;

    public GroundType groundType; //reports back to character controller 2D what  ground type we are standing on.

    //WallType on our left
    public WallType leftWallType; 
    public bool leftIsRunnable;
    public bool leftIsJumpable;
    public float leftSlideModifier;

    //WallType on your right
    public WallType rightWallType; 
    public bool rightIsRunnable;
    public bool rightIsJumpable;
    public float rightSlideModifier;

    public GroundType ceilingType; //Ceiling above our heads

    public WallEffector leftwallEffector;
    public WallEffector rightWallEffector;

    public float jumpPadAmount;
    public float jumPadUpperLimit; 


    private Vector2 _moveAmount;
    private Vector2 _currentPostion;
    private Vector2 _lastPosition;

    private Rigidbody2D _rigidbody;
    private CapsuleCollider2D _capsuleCollider;

    private Vector2[] _raycastPosition = new Vector2[3];
    private RaycastHit2D[] _raycastHits = new RaycastHit2D[3];

    private bool _disbaleGroundCheck;

    private Vector2 _slopeNormal; //a grounds normal is a projection from that surface in the direction that surface is facing. Great for angles.
    private float _slopeAngle; //float means the number can change
    private bool _inAirLastFrame; 
    private bool _noSideCollissionsLastFrame;

    private Transform _tempMovingPlatform;
    private Vector2 _movingPlatformVelocity; 


    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = gameObject.GetComponent<Rigidbody2D>();
        _capsuleCollider = gameObject.GetComponent<CapsuleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        _inAirLastFrame = !below;

        _noSideCollissionsLastFrame = (!right && !left);//creating a variable and making it equal when both left and right colisions are false

        _lastPosition = _rigidbody.position;

        //slope angle adjustment
        if (_slopeAngle != 0 && below == true)
        {
            if((_moveAmount.x > 0f && _slopeAngle > 0f) || (_moveAmount.x < 0f && _slopeAngle < 0f))//checking for movement and angles to adjust y axis movement.
            {
                _moveAmount.y = -Mathf.Abs(Mathf.Tan(_slopeAngle * Mathf.Deg2Rad) * _moveAmount.x); //adjusting y axis movement to reflect slope we are on.
                _moveAmount.y *= downForceAdjustment;//increasing downward force by 20% when travelling down a slope.
            }
        }

        //moving platform adjustment
        if (groundType == GroundType.MovingPlatform)
        {
            //offset the players movement on the X with moving platform velocity
            _moveAmount.x += MovingPlatformAdjust().x;//Give the player the same moveamount as the platform on the x


            //if platform is moving down offset the players movement on the Y
            if (MovingPlatformAdjust().y < 0f)
            {
                _moveAmount.y += MovingPlatformAdjust().y;
                _moveAmount.y *= downForceAdjustment;
            }
        }

        if (groundType == GroundType.CollapseablePlatform)
        {
            if (MovingPlatformAdjust().y < 0f)//is the platform moving down away from the player
            {
                _moveAmount.y += MovingPlatformAdjust().y;
                _moveAmount.y *= downForceAdjustment * 4;
            }
        }

        _currentPostion = _lastPosition + _moveAmount;

        _rigidbody.MovePosition(_currentPostion);

        _moveAmount = Vector2.zero;

        if (!_disbaleGroundCheck)
        {

            CheckGrounded();


        }

        CheckOtherCollisions();

        if (below && _inAirLastFrame)
        {
            hitGroundThisFrame = true; 
        }
        else
        {
            hitGroundThisFrame = false; 
        }

        if ((right || left) && _noSideCollissionsLastFrame)//if right or left colision is true and so is the variable _nosidecolls that means we are hiting a wall now else we are not
        {
            hitWallThisFrame = true;
        }else
        {
            hitWallThisFrame = false;
        }

    }

    public void Move(Vector2 movement)
    {
        _moveAmount += movement;
    }

    private void CheckGrounded()
    {
        //variable to hold the results of the raycast so they can be used again.
        RaycastHit2D hit = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, _capsuleCollider.size, CapsuleDirection2D.Vertical, 0f, Vector2.down, raycastDistance, layerMask); 

        if (hit.collider)
        {
            groundType = DetermineGroundType(hit.collider);

            _slopeNormal = hit.normal;
            _slopeAngle = Vector2.SignedAngle(_slopeNormal, Vector2.up);

            if (_slopeAngle > slopeAngleLimit || _slopeAngle < -slopeAngleLimit)
            {
                below = false;
            }
            else
            {
                below = true;
            }
            //if the ground type we are on is JumpPad we want to read in the prperties from the attached script in green.
            if (groundType == GroundType.JumpPad)
            {
                JumpPad jumpPad = hit.collider.GetComponent<JumpPad>();//allows us to get components from the attahced script in green
                jumpPadAmount = jumpPad.jumpPadAmount; 
                jumPadUpperLimit = jumpPad.jumPadUpperLimit;
            }
            
        }
        else
        {
            groundType = GroundType.None;
            below = false;
            if (_tempMovingPlatform)
            {
                _tempMovingPlatform = null;
            }
        }
    }

    private void CheckOtherCollisions()
    {
        //check left

        RaycastHit2D leftHit = Physics2D.BoxCast(_capsuleCollider.bounds.center, _capsuleCollider.size * 0.75f, 0f, Vector2.left, raycastDistance * 2, layerMask);

        if (leftHit.collider)
        {
            leftWallType = DetermineWallType(leftHit.collider);
            left = true;
            leftwallEffector = leftHit.collider.GetComponent<WallEffector>();

            if (leftwallEffector)
            {
                leftIsRunnable = leftwallEffector.isRunnable;
                leftIsJumpable = leftwallEffector.isjumpable;
                leftSlideModifier =  leftwallEffector.wallSlideAmount;
            }
        }
        else
        {
            leftWallType = WallType.None;
            left = false;
        }

        //check right

        RaycastHit2D rightHit = Physics2D.BoxCast(_capsuleCollider.bounds.center, _capsuleCollider.size * 0.75f, 0f, Vector2.right, raycastDistance * 2, layerMask);

        if (rightHit.collider)
        {
            rightWallType = DetermineWallType(rightHit.collider);
            right = true;
            rightWallEffector = rightHit.collider.GetComponent<WallEffector>();

            rightWallEffector = rightHit.collider.GetComponent<WallEffector>();

            if (rightWallEffector)
            {
                rightIsRunnable = rightWallEffector.isRunnable;
                rightIsJumpable = rightWallEffector.isjumpable;
                rightSlideModifier = rightWallEffector.wallSlideAmount;
            }
        }
        else
        {
            rightWallType = WallType.None;
            right = false;
        }

        //check above
        RaycastHit2D aboveHit = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, _capsuleCollider.size, CapsuleDirection2D.Vertical, 0f, Vector2.up, raycastDistance, layerMask);

        if (aboveHit.collider)
        {
            ceilingType = DetermineGroundType(aboveHit.collider);
            above = true;
        }
        else
        {
            ceilingType = GroundType.None;
            above = false;
        }

    }





#region CheckGrounded

/*
    private void CheckGrounded()
    {
        Vector2 raycastOrigin = _rigidbody.position - new Vector2(0, _capsuleCollider.size.y * 0.5f);

        _raycastPosition[0] = raycastOrigin + (Vector2.left * _capsuleCollider.size.x * 0.25f + Vector2.up * 0.1f);
        _raycastPosition[1] = raycastOrigin;
        _raycastPosition[2] = raycastOrigin + (Vector2.right * _capsuleCollider.size.x * 0.25f + Vector2.up * 0.1f);

        DrawDebugRays(Vector2.down, Color.green);

        int numberOfGroundHits = 0;

        for (int i = 0; i < _raycastPosition.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(_raycastPosition[i], Vector2.down, raycastDistance, layerMask);

            if (hit.collider)
            {
                _raycastHits[i] = hit;
                numberOfGroundHits++;
            }
        }
//Check if there is something else below us
        if (numberOfGroundHits > 0)
        {
            //Check the middle raycast for the ground type it is hitting off
            if (_raycastHits[1].collider)//Does the raycast at position 1 hit off a collider
            {
                groundType = DetermineGroundType(_raycastHits[1].collider);
                _slopeNormal = _raycastHits[1].normal; //Determine the normal of the angle from the middle raycast. 
                _slopeAngle = Vector2.SignedAngle(_slopeNormal, Vector2.up);//calculate the angle between the up direction and the slope. 

            }

            //If the middle raycast hits nothing go through them all to see if any of the other rays are hitting anything.
            else 
            {
                for (int i = 0; i < _raycastHits.Length; i++)
                {
                    if (_raycastHits[i].collider)
                    {
                        groundType = DetermineGroundType(_raycastHits[i].collider);
                        _slopeNormal = _raycastHits[i].normal; //Determine the normal of the angle from the middle raycast. 
                        _slopeAngle = Vector2.SignedAngle(_slopeNormal, Vector2.up);//calculate the angle between the up direction and the slope.

                    }

                }

            }
            if (_slopeAngle > slopeAngleLimit || _slopeAngle < -slopeAngleLimit)//if the slope is higher than the angle act as if we are not on the ground.
            {
                below = false;
            }
            else
            {
                below = true;
            }
            
        }
        else
        {
            groundType = GroundType.None; 
            below = false;
        }

        System.Array.Clear(_raycastHits, 0, _raycastHits.Length);//Clear the array ray cast hits after each ground check and reset all arrays to 0

    }*/
    #endregion

    private void DrawDebugRays(Vector2 direction, Color color)
    {
        for (int i = 0; i < _raycastPosition.Length; i++)
        {
            Debug.DrawRay(_raycastPosition[i], direction * raycastDistance, color);
        }
    }

//When this method is called the ground check method will be disabled temporarily
    public void DisableGroundCheck()
    {
        below = false;
        _disbaleGroundCheck = true;

        //Coroutine is a timer you can use for actions
        StartCoroutine("EnableGroundCheck");

    }

    IEnumerator EnableGroundCheck()
    {

        yield return new WaitForSeconds (0.1f);
        _disbaleGroundCheck = false;

    }

    //if we have a special ground type return that ground type if we don't just return the default ground type.
    private GroundType DetermineGroundType(Collider2D collider)
    {
        //if the collider the raycast hits has a GroundEffector attached to it we want to return that ground effector.
        if (collider.GetComponent<GroundEffector>())
        {
            GroundEffector groundEffector = collider.GetComponent<GroundEffector>();
            if (groundType == GroundType.MovingPlatform || groundType == GroundType.CollapseablePlatform)
            {
                if (!_tempMovingPlatform)
                {
                    _tempMovingPlatform = collider.transform;

                    if (groundType == GroundType.CollapseablePlatform)
                    {
                        _tempMovingPlatform.GetComponent<CollapseablePlatform>().CollapsePlatform();//lok for the script CollapseablePlatform and in that script call the method CollapsePlatform
                    }
                }
            }

            return groundEffector.groundType;

        }
        else 
        {
            if (_tempMovingPlatform)
            {
                _tempMovingPlatform = null;
            }

            return GroundType.LevelGeometry;

        }


    }

    private WallType DetermineWallType(Collider2D collider)//passing the WallType the 2D collider we created above and calling it collider to use here
    {
        if (collider.GetComponent<WallEffector>())//if the collider hits a WallEffector we want to return the wall type. If there is no WallType detected by the collider than return the walltype normal.
        {
            WallEffector wallEffector = collider.GetComponent<WallEffector>();
            return wallEffector.wallType;
        }
        else
        {
            return WallType.Normal;
        }
    }

    private Vector2 MovingPlatformAdjust()
    {
        if (_tempMovingPlatform && groundType == GroundType.MovingPlatform)
        {
        //the velocity equals the difference variable from the script MovingPlatform
        _movingPlatformVelocity = _tempMovingPlatform.GetComponent<MovingPlatform>().difference;
        //_movingPlatformVelocity can now be used in other places in the script.
        return _movingPlatformVelocity;
        }
        else if (_tempMovingPlatform && groundType == GroundType.CollapseablePlatform)
        {
            _movingPlatformVelocity = _tempMovingPlatform.GetComponent<CollapseablePlatform>().difference;
            return _movingPlatformVelocity;
        }
        else 
        {
            return Vector2.zero;
        }
    }

    public void ClearMovingPlatform()
    {
        if (_tempMovingPlatform)
        {
            _tempMovingPlatform = null;
        }
    }
}
