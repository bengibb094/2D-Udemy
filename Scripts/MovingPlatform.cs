using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{

    public Transform[] waypoints; //Array to hold the moving platforms waypoints
    public float moveSpeed = 5f;
    public Vector2 difference;

    private Vector3 _lastPosition;
    private Vector3 _currentWaypoint;
    private int _waypointCounter;

    // Start is called before the first frame update
    void Start()
    {
        //waypointcounter is set to 0 and then the current waypoint at the beginning is set to the position we have assigned to _waypointcounter, what ever it is.
        _waypointCounter = 0;
        _currentWaypoint = waypoints[_waypointCounter].position;

        
    }

    // Update is called once per frame
    void Update()
    {
        _lastPosition = transform.position;//the current position at the start of the update loop we assign to _lastposition.

        transform.position = Vector3.MoveTowards(transform.position, _currentWaypoint, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _currentWaypoint) < 0.1f)
        {
            _waypointCounter++;
            if (_waypointCounter >= waypoints.Length)
            {
                _waypointCounter = 0;
            }
           _currentWaypoint = waypoints[_waypointCounter].position; 
        }

        difference = transform.position - _lastPosition;
        
    }
}
