using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalTypes;

public class CharacterController2D : MonoBehaviour
{
    public float raycastDistance = 0.2f;
    public LayerMask layerMask;

    //flags
    public bool below;
    public GroundType groundType; //reports back to character controller 2D what  ground type we are standing on.


    private Vector2 _moveAmount;
    private Vector2 _currentPostion;
    private Vector2 _lastPosition;

    private Rigidbody2D _rigidbody;
    private CapsuleCollider2D _capsuleCollider;

    private Vector2[] _raycastPosition = new Vector2[3];
    private RaycastHit2D[] _raycastHits = new RaycastHit2D[3];

    private bool _disbaleGroundCheck;


    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = gameObject.GetComponent<Rigidbody2D>();
        _capsuleCollider = gameObject.GetComponent<CapsuleCollider2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _lastPosition = _rigidbody.position;

        _currentPostion = _lastPosition + _moveAmount;

        _rigidbody.MovePosition(_currentPostion);

        _moveAmount = Vector2.zero;

        if (!_disbaleGroundCheck)
        {

            CheckGrounded();


        }

    }

    public void Move(Vector2 movement)
    {
        _moveAmount += movement;
    }

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

            }

            //If the middle raycast hits nothing go through them all to see if any of the other rays are hitting anything.
            else 
            {
                for (int i = 0; i < _raycastHits.Length; i++)
                {
                    if (_raycastHits[i].collider)
                    {
                        groundType = DetermineGroundType(_raycastHits[i].collider);

                    }

                }

            }

            below = true;
        }
        else
        {
            groundType = GroundType.None; 
            below = false;
        }

    }

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
            return groundEffector.groundType;

        }
        else 
        {
            return GroundType.LevelGeometry;

        }


    }
}