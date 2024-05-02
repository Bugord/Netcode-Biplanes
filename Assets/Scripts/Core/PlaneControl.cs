using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlaneControl : NetworkBehaviour
{
    [SerializeField]
    private Rigidbody2D rigidbody2D;

    [SerializeField]
    private PlaneData planeData;

    [SerializeField]
    private float maxSpeed;

    [SerializeField]
    private float rorationCooldown;

    private float lastRotationTime;

    private Vector2 engine;
    private Vector2 lift;

    private Angle currentAngle;
    private List<Angle> anglesList;
    private bool isRight = true;

    private bool isEngineEnabled;
    
    public bool IsTakenOff { get; private set; }
    

    private void Awake()
    {
        Debug.Log($"{name} - Awake - {rigidbody2D.isKinematic}");
        anglesList = Enum.GetValues(typeof(Angle)).Cast<Angle>().ToList();
        currentAngle = Angle.Front;
    }

    private void Update()
    {
        ChangeEngineEnabled();
        ChangeDirection();
        RotateSprite();
        SetModifiers();
        DrawDebugLines();
        
        Debug.Log($"{name} - {rigidbody2D.isKinematic}");
    }

    private void FixedUpdate()
    {
        rigidbody2D.AddForce(engine);
        rigidbody2D.AddForce(lift);

        if (rigidbody2D.velocity.magnitude > maxSpeed) {
            rigidbody2D.velocity = rigidbody2D.velocity.normalized * maxSpeed;
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground")) {
            IsTakenOff = true;
            Debug.Log($"{name}: taken off");
        }
    }
    
    public void OnEnable()
    {
        Debug.Log($"{name} - OnEnable - {rigidbody2D.isKinematic}");
        rigidbody2D.isKinematic = false;
    }
    
    public void OnDisable()
    {
        Debug.Log($"{name} - OnDisable - {rigidbody2D.isKinematic}");
        rigidbody2D.velocity = Vector2.zero;
        rigidbody2D.isKinematic = true;
    }

    public void Reset()
    {
        rigidbody2D.velocity = Vector2.zero;
        currentAngle = Angle.Front;
        isRight = true;
        isEngineEnabled = false;
        rigidbody2D.isKinematic = false;

        if (!NetworkObject.IsOwnedByServer) {
            isRight = false;
            transform.localScale = new Vector3(1, -1, 1);
        }
        
        RotateSprite();
        IsTakenOff = false;
    }

    private void ChangeEngineEnabled()
    {
        if (Input.GetKey(KeyCode.W)) {
            isEngineEnabled = true;
        }
        if (Input.GetKey(KeyCode.S)) {
            isEngineEnabled = false;
        }
    }

    private void ChangeDirection()
    {
        if (!IsOwner) {
            return;
        }

        if (!isEngineEnabled) {
            return;
        }

        var moveLeft = Input.GetKey(KeyCode.A);
        var moveRight = Input.GetKey(KeyCode.D);

        if (lastRotationTime + rorationCooldown > Time.time) {
            return;
        }

        if (!moveLeft && !moveRight) {
            return;
        }

        lastRotationTime = Time.time;

        var currentIndex = anglesList.IndexOf(currentAngle);
        if (moveLeft) {
            if (currentIndex == 0) {
                isRight = false;
            }
            if (currentIndex == anglesList.Count - 1) {
                isRight = true;
            }

            currentIndex = isRight
                ? Mathf.Max(currentIndex - 1, 0)
                : Mathf.Min(currentIndex + 1, anglesList.Count - 1);
        }

        if (moveRight) {
            if (currentIndex == anglesList.Count - 1) {
                isRight = false;
            }
            if (currentIndex == 0) {
                isRight = true;
            }

            currentIndex = isRight
                ? Mathf.Min(currentIndex + 1, anglesList.Count - 1)
                : Mathf.Max(currentIndex - 1, 0);
        }

        currentAngle = anglesList[currentIndex];
    }

    private void RotateSprite()
    {
        transform.rotation =
            isRight ? Quaternion.Euler(0, 0, 90 - GetRotataion()) : Quaternion.Euler(0, 0, GetRotataion() + 90);
    }

    private void SetModifiers()
    {
        engine = planeData.engineForceModifiers[currentAngle] * planeData.engineForce;
        lift = planeData.liftForceModifiers[currentAngle] * planeData.liftForce;

        if (!isEngineEnabled) {
            engine = Vector2.zero;
            lift = Vector2.zero;
        }

        if (!isRight) {
            engine = engine * new Vector2(-1, 1);
            lift = lift * new Vector2(-1, 1);
        }
    }

    private float GetRotataion()
    {
        return currentAngle switch {
            Angle.Up => 0,
            Angle.UpUpFront => 22,
            Angle.UpFront => 45,
            Angle.UpFrontFront => 67,
            Angle.Front => 90,
            Angle.DownFrontFront => 113,
            Angle.DownFront => 135,
            Angle.DownDownFront => 157,
            Angle.Down => 180,
            _ => 0
        };
    }
    
    private void DrawDebugLines()
    {
        var position = (Vector2)transform.position;

        Debug.DrawLine(position, position + Physics2D.gravity / 5f, Color.red);
        Debug.DrawLine(position, position + engine / 5f, Color.green);
        Debug.DrawLine(position, position + lift / 5f, Color.blue);
        Debug.DrawLine(position, position + (Physics2D.gravity + engine + lift) / 5f, Color.magenta);
    }
}