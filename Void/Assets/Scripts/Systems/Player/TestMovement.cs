using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class TestMovement : MonoBehaviour
{
    private Rigidbody rig;
    [SerializeField] private float speed;
    Animator animator;
    [SerializeField] float movementmultiplier = 1.2f;
    

    private void Awake()
    {
        rig = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
        
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));


        
        animator.SetFloat("xinput", (float)(input.x * movementmultiplier));
        animator.SetFloat("yinput", (float)(input.z * movementmultiplier));
        
        rig.velocity = speed * input.normalized;
    }
}
