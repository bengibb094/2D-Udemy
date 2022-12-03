using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollapseablePlatform : GroundEffector
{
    public float fallSpeed = 10f;
    public float delayTime = 0.5f;

    public Vector3 difference;

    private bool _platformCollapsing = false;
    private Rigidbody2D _rigidbody;
    private Vector3 _lastPosition;




    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = gameObject.GetComponent<Rigidbody2D>();
        
    }

    // Update is called once per frame
    void Update()
    {
         _lastPosition = transform.position;//before any physics we're caching _lastPosition as its current position


    }

    private void LateUpdate() 
    {
        difference = transform.position - _lastPosition;

        if (_platformCollapsing)
        {
            _rigidbody.AddForce(Vector2.down * fallSpeed);//gives us control over the falling speed of the platform

            if (_rigidbody.velocity.y == 0)//reseting the platform to a static object
            {
                _platformCollapsing = false;
                _rigidbody.bodyType = RigidbodyType2D.Static;
            }
        }


        
    }
    
    public void CollapsePlatform()
    {
            StartCoroutine("CollpasePlatformCoroutine");//coroutine to monitor for the time delay before collpasing the platform
    }

    public IEnumerator CollpasePlatformCoroutine()//IEnumerator to actually do the timing for the change and making platform a physical object
    {
        yield return new WaitForSeconds(delayTime);
        _platformCollapsing = true;

        _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rigidbody.freezeRotation = true;
        _rigidbody.gravityScale = 1f;
        _rigidbody.mass = 1000f;
        _rigidbody.bodyType = RigidbodyType2D.Dynamic;
    }
}
