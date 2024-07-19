using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSystem : MonoBehaviour
{
    public static CameraSystem instance;

    private void Awake()
    {
        instance = this;

        moveTarget = transform.position;
    }

    [Header("Camera Variables")]
    public float moveSpeed, manualMoveSpeed;

    float targetRot; //Rotation
    public float rotateSpeed;
    int currentAngle;


    [Header("Input Keys")]
    public KeyCode snapBackToPlayer;
    public KeyCode rotateLeft;
    public KeyCode rotateRight;


    Vector3 moveTarget; //Target that Camera should follow
    Vector2 moveInput; //For Manual Camera Movement



    private void Start()
    {
        
    }

    private void Update()
    {
        if (moveTarget != transform.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, moveTarget, moveSpeed * Time.deltaTime);
        }

        if ((LevelManager.Instance?.PlayerCanControl ?? true) && (!PauseMenu.Instance?.IsPaused ?? false))
        {
            moveInput.x = Input.GetAxis("Horizontal");
            moveInput.y = Input.GetAxis("Vertical");
            moveInput.Normalize();

            if (moveInput != Vector2.zero)
            {
                transform.position += ((transform.forward * (moveInput.y * manualMoveSpeed)) +
                    (transform.right * (moveInput.x * manualMoveSpeed))) * Time.deltaTime;

                moveTarget = transform.position;

            }

            if (Input.GetKeyDown(snapBackToPlayer)) //Snapping back to player
            {
                SetMoveTarget(GameManager.instance.ActivePlayer.transform.position);
            }

            if (Input.GetKeyDown(rotateLeft)) //Rotate Right
            {
                currentAngle++;

                if (currentAngle >= 4)
                {
                    currentAngle = 0;
                }

            }

            if (Input.GetKeyDown(rotateRight)) //Rotate Left
            {
                currentAngle--;

                if (currentAngle < 0)
                {
                    currentAngle = 3;
                }
            }
        }

        targetRot = (90f * currentAngle) + 45f;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, targetRot, 0f), rotateSpeed * Time.deltaTime);

    }

    public void SetMoveTarget(Vector3 newTarget)
    {
        moveTarget = newTarget;
    }
}
