using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Plane : NetworkBehaviour
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

    private void Awake()
    {
        anglesList = Enum.GetValues(typeof(Angle)).Cast<Angle>().ToList();
        currentAngle = Angle.Front;
    }

    private void Update()
    {
        ChangeDirection();
        RotateSprite();
        SetModifiers();
        DrawDebugLines();
    }

    private void FixedUpdate()
    {
        rigidbody2D.AddForce(engine);
        rigidbody2D.AddForce(lift);

        if (rigidbody2D.velocity.magnitude > maxSpeed) {
            rigidbody2D.velocity = rigidbody2D.velocity.normalized * maxSpeed;
        }
    }

    private void SetModifiers()
    {
        engine = planeData.engineForceModifiers[currentAngle] * planeData.engineForce;
        lift = planeData.liftForceModifiers[currentAngle] * planeData.liftForce;

        if (!isRight) {
            engine = engine * new Vector2(-1, 1);
            lift = lift * new Vector2(-1, 1);
        }
    }

    private void DrawDebugLines()
    {
        var position = (Vector2)transform.position;

        Debug.DrawLine(position, position + Physics2D.gravity / 5f, Color.red);
        Debug.DrawLine(position, position + engine / 5f, Color.green);
        Debug.DrawLine(position, position + lift / 5f, Color.blue);
        Debug.DrawLine(position, position + (Physics2D.gravity + engine + lift) / 5f, Color.magenta);
    }

    private void RotateSprite()
    {
        transform.rotation =
            isRight ? Quaternion.Euler(0, 0, 90 - GetRotataion()) : Quaternion.Euler(0, 0, GetRotataion() + 90);
    }

    private void ChangeDirection()
    {
        if (!IsOwner) {
            return;
        }
        
        var moveLeft = Input.GetKey(KeyCode.A);
        var moveRight = Input.GetKey(KeyCode.D);

        if (lastRotationTime + rorationCooldown > Time.time) {
            return;
        }

        lastRotationTime = Time.time;

        if (!moveLeft && !moveRight) {
            return;
        }

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

    private float GetRotataion()
    {
        switch (currentAngle) {
            case Angle.Up:
                return 0;
            case Angle.UpUpFront:
                return 22;
            case Angle.UpFront:
                return 45;
            case Angle.UpFrontFront:
                return 67;
            case Angle.Front:
                return 90;
            case Angle.DownFrontFront:
                return 113;
            case Angle.DownFront:
                return 135;
            case Angle.DownDownFront:
                return 157;
            case Angle.Down:
                return 180;
        }

        return 0;
    }
}