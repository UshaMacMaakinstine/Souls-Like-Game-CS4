using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
    // references
    public CharacterController controller;
    public Transform cam;

    public GameObject[] colliders;

    public float speed = 6f;

    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void EyeFramesStart()
    {
        foreach (GameObject collider in colliders)
        {
            collider.SetActive(false);
        }
    }

    private void EyeFramesEnd()
    {
        foreach (GameObject collider in colliders)
        {
            collider.SetActive(true);
        }
    }

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 dir = new Vector3(horizontal, 0f, vertical).normalized;

        if(dir.magnitude >= 0.1f)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                Debug.Log("walking");
                speed = 3f;
                anim.SetBool("isRunning", false);
                anim.SetBool("isWalking", true);
            }
            else
            {
                Debug.Log("running");
                speed = 6f;
                anim.SetBool("isRunning", true);
                anim.SetBool("isWalking", false);
            }


            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                EyeFramesStart();
                anim.SetTrigger("Slide");
                EyeFramesEnd();
            }
            float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }
        else
        {
            anim.SetBool("isRunning", false);
            anim.SetBool("isWalking", false);
        }
    }
}
