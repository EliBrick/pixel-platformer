using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float maxHorizontalSpeed = .05f;
    public float horizontalAccelerationCoefficient = .01f;
    public float frictionCoefficient = .001f;
    public float groundedDistance = .05f;
    public float jumpForce = 10;
    private bool grounded = false;
    private Vector2 controlledAcceleration = Vector2.zero;
    private Rigidbody2D rb2D;
    public new Collider2D collider;
    public Dictionary<Movement, bool> hasPressed = new Dictionary<Movement, bool>();

    public enum Movement {
        Left,
        Right,
        Jump
    }

    // Start is called before the first frame update
    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        foreach(Movement m in System.Enum.GetValues(typeof(Movement)))
        {
            hasPressed[m] = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Ray2D ray = new Ray2D(new Vector2(collider.bounds.center.x, collider.bounds.center.y - collider.bounds.extents.y), Vector2.down);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, groundedDistance);
            Gizmos.color = Color.red;
            if (hit.point == Vector2.zero)
            {
                Gizmos.color = Color.green;
                hit.point = new Vector2(collider.bounds.center.x, collider.bounds.center.y - collider.bounds.extents.y-groundedDistance);
            }
            Gizmos.DrawLine(new Vector2(collider.bounds.center.x, collider.bounds.center.y - collider.bounds.extents.y), hit.point);

            Ray2D rayLeft = new Ray2D(new Vector2(collider.bounds.center.x - collider.bounds.extents.x, collider.bounds.center.y - collider.bounds.extents.y), Vector2.down);
            RaycastHit2D hitLeft = Physics2D.Raycast(rayLeft.origin, rayLeft.direction, groundedDistance);
            Gizmos.color = Color.red;
            if (hitLeft.point == Vector2.zero)
            {
                Gizmos.color = Color.green;
                hitLeft.point = new Vector2(collider.bounds.center.x - collider.bounds.extents.x, collider.bounds.center.y - collider.bounds.extents.y - groundedDistance);
            }
            Gizmos.DrawLine(new Vector2(collider.bounds.center.x - collider.bounds.extents.x, collider.bounds.center.y - collider.bounds.extents.y), hitLeft.point);

            Ray2D rayRight = new Ray2D(new Vector2(collider.bounds.center.x + collider.bounds.extents.x, collider.bounds.center.y - collider.bounds.extents.y), Vector2.down);
            RaycastHit2D hitRight = Physics2D.Raycast(rayRight.origin, rayRight.direction, groundedDistance);
            Gizmos.color = Color.red;
            if (hitRight.point == Vector2.zero)
            {
                Gizmos.color = Color.green;
                hitRight.point = new Vector2(collider.bounds.center.x + collider.bounds.extents.x, collider.bounds.center.y - collider.bounds.extents.y - groundedDistance);
            }
            Gizmos.DrawLine(new Vector2(collider.bounds.center.x + collider.bounds.extents.x, collider.bounds.center.y - collider.bounds.extents.y), hitRight.point);
        }
    }

    private void Update()
    {
        #region Handle Keyboard Inputs
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            hasPressed[Movement.Left] = true;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            hasPressed[Movement.Right] = true;
        }
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Z))
        {
            hasPressed[Movement.Jump] = true;
        }
        #endregion
    }

    void FixedUpdate()
    {
        controlledAcceleration = Vector2.zero;
        #region Determine grounded
        Ray2D ray = new Ray2D(new Vector2(collider.bounds.center.x, collider.bounds.center.y - collider.bounds.extents.y), Vector2.down);
        Ray2D rayRight = new Ray2D(new Vector2(collider.bounds.center.x + collider.bounds.extents.x, collider.bounds.center.y - collider.bounds.extents.y), Vector2.down);
        Ray2D rayLeft = new Ray2D(new Vector2(collider.bounds.center.x - collider.bounds.extents.x, collider.bounds.center.y - collider.bounds.extents.y), Vector2.down);
        grounded = !(Physics2D.Raycast(ray.origin, ray.direction, groundedDistance).collider is null && Physics2D.Raycast(rayRight.origin, rayRight.direction, groundedDistance).collider is null && Physics2D.Raycast(rayLeft.origin, rayLeft.direction, groundedDistance).collider is null);
        #endregion

        #region Translate Inputs to Controls
        if (hasPressed[Movement.Left])
        {
            PressLeft();
            hasPressed[Movement.Left] = false;
        }
        if (hasPressed[Movement.Right] == true)
        {
            PressRight();
            hasPressed[Movement.Right] = false;
        }
        if (hasPressed[Movement.Jump] == true)
        {
            PressJump();
            hasPressed[Movement.Jump] = false;
        }
        #endregion

        Move();
    }

    private void PressLeft()
    {
        float v0 = rb2D.velocity.x;
        float t = Time.deltaTime;
        float v = Mathf.Clamp(v0 - horizontalAccelerationCoefficient*t, -maxHorizontalSpeed, maxHorizontalSpeed);
        float a = (v - v0) / t;
        controlledAcceleration.x += a;
        //Debug.Log("Press Left Acceleration: " + a);

    }
    private void PressRight()
    {
        float v0 = rb2D.velocity.x;
        float t = Time.deltaTime;
        float v = Mathf.Clamp(v0 + horizontalAccelerationCoefficient * t, -maxHorizontalSpeed, maxHorizontalSpeed);
        float a = (v - v0) / t;
        controlledAcceleration.x += a;
        //Debug.Log("Press Right Acceleration: " + a);
    }
    private void PressJump()
    {
        if (!grounded)
        {
            //Debug.Log("Tried to jump when not grounded!");
            return;
        }
        controlledAcceleration.y += jumpForce;
    }
    private void ApplyFriction()
    {
        //TODO only apply if player is on ground
        float v0 = rb2D.velocity.x;
        float finalVelocity = Mathf.MoveTowards(rb2D.velocity.x, 0, frictionCoefficient*Time.deltaTime);
        float a = (finalVelocity - v0) / Time.deltaTime;
        controlledAcceleration.x += a;
    }
    private void Move()
    {
        ApplyFriction();
        rb2D.AddForce(controlledAcceleration*rb2D.mass);
    }
}
