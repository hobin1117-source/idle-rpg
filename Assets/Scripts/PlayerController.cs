using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("MonveMent")]  //변수이름은 자유!
    public float MoveSpeed;
    public float JumpedPower;
    private int maxJump = 1;
    private int JumpCount;
    private Vector2 curMovemetInput;
    public LayerMask groundLayerMask;
    public ItemSlot[] slots;

    public GameObject inventoryWindow;
    public Transform slotPanel;
    public Transform dropPosition;
    [Header("Look")]
    public Transform cameraContainer;
    public float minXLook;
    public float maxXLook;
    private float camCurXRot;
    public float lookSenitivity;
    public float wallDistance = 0.1f;
    public LayerMask wallLayer;
    public float climbHeight;
    private Vector2 mouseDelta;
    private Rigidbody _rigidbody;
    bool isJumping = false;
    float lastJumpTime;
    float jumpBuffer = 0.1f;
    public bool canLook = true;
    public Action inventory;





    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;  //커서 온 오프

    }
    private void Update()
    {

    }

    private void FixedUpdate()
    {
        Move();

        if (isJumping && Time.time - lastJumpTime < jumpBuffer)
        {
            if (IsGrounded())
            {
                isJumping = false;
                JumpCount = 0;
            }
        }
        WallClimb();
    }
    private void LateUpdate()
    {

        if (canLook)
        {
            CameraLook();
        }

    }
    private void Move()
    {

        
            Vector3 dir = transform.forward * curMovemetInput.y + transform.right * curMovemetInput.x;
        if (IsGrounded())
        {
            dir *= MoveSpeed;
        }
        else
        {
            dir *= MoveSpeed*0.3f;
        }
            dir.y = _rigidbody.velocity.y;
            _rigidbody.velocity = dir;
        

    }
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started || context.phase == InputActionPhase.Performed)
        {
            curMovemetInput = context.ReadValue<Vector2>();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            curMovemetInput = Vector2.zero;
        }


    }
    void CameraLook()
    {
        camCurXRot += mouseDelta.y * lookSenitivity;
        camCurXRot = Mathf.Clamp(camCurXRot, minXLook, maxXLook);
        cameraContainer.localEulerAngles = new Vector3(-camCurXRot, 0, 0);
        transform.eulerAngles += new Vector3(0, mouseDelta.x * lookSenitivity, 0);
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        mouseDelta = context.ReadValue<Vector2>();
    }


    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            if (IsGrounded())
            {
                _rigidbody.AddForce(Vector2.up * JumpedPower, ForceMode.Impulse);
                isJumping = true;
                lastJumpTime = Time.time;

            }
            else if (JumpCount < maxJump)
            {

                _rigidbody.AddForce(Vector2.up * JumpedPower, ForceMode.Impulse);
                isJumping = true;
                JumpCount++;
                Debug.Log(JumpCount);
            }

        }

    }
    bool IsGrounded()
    {
        Ray[] rays = new Ray[4]
        {
            new Ray(transform.position + (transform.forward* 0.2f) + (transform.up *0.01f), Vector3.down),
            new Ray(transform.position + (-transform.forward* 0.2f) + (transform.up*0.01f), Vector3.down),
            new Ray(transform.position + (transform.right* 0.2f) + (transform.up *0.01f), Vector3.down),
            new Ray(transform.position + (-transform.right* 0.2f) + (transform.up *0.01f), Vector3.down)
        };
        for (int i = 0; i < rays.Length; i++)
        {
           
            if (Physics.Raycast(rays[i], 0.5f, groundLayerMask))
            {
                return true;
            }
        }
        return false;

    }
    bool IsWall()
    {
        Ray[] rays = new Ray[4]
        {
        new Ray(transform.position + (transform.forward * 0.2f) + (transform.up * 0.25f), transform.forward),
        new Ray(transform.position + (-transform.forward * 0.2f) + (transform.up * 0.25f), transform.forward),
        new Ray(transform.position + (transform.right * 0.2f) + (transform.up * 0.25f), transform.forward),
        new Ray(transform.position + (-transform.right * 0.2f) + (transform.up * 0.25f), transform.forward)
        };

        for(int i = 0; i< rays.Length;i++)
        {
            if (Physics.Raycast(rays[i], 0.5f, wallLayer))
            {
                return true;
            }
        }
        return false;  }
    void WallClimb()
    {
        if (IsWall())
        {
            float verticalInput = curMovemetInput.y; 
            _rigidbody.velocity = new Vector3(0, verticalInput * MoveSpeed, 0);
        }
    }


    public void OnInventoryButton(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.phase == InputActionPhase.Started)
        {
            inventory?.Invoke();
            ToggleCursor();
        }
    }

    void ToggleCursor()
    {
        bool toggle = Cursor.lockState == CursorLockMode.Locked;
        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
        canLook = !toggle;
    }


}







