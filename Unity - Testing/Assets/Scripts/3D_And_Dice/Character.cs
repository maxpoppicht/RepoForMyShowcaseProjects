using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public int moveSpeed;


    private int rotationSpeed = 2;
    private Animator animator;
    private Rigidbody rigidbody;
    private bool isWalking;


    //Bools
    bool lookingForward;

    bool lookingRight;

    bool lookingBack;

    bool lookingLeft;


    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        rigidbody.isKinematic = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        MoveCharacter();
        //Do this or else animation doesnt work
        isWalking = false;


       
    }

    void MoveCharacter()
    {
        //negative Keys = a,s 
        //Vector for moving
        Vector3 moveVector = Vector3.zero;

        //Vector for rotation
        Vector3 rotationVector = Vector3.zero;

        //Calculate speed
        var addedSpeed = moveSpeed * Time.deltaTime;

        var vertical = Input.GetAxisRaw("Vertical");

        var horizontal = Input.GetAxisRaw("Horizontal");


        Debug.Log("left " + lookingLeft);
        Debug.Log("right " + lookingRight);
        Debug.Log("forward " + lookingForward);
        Debug.Log("back " + lookingBack);
        Debug.Log("rotation " + rigidbody.rotation.y);

        //Move forward, positive key
        if (Input.GetKey(KeyCode.W))
        {
            if (rigidbody.rotation.y <= 0.01 && rigidbody.rotation.y >= -0.01)
            {
                lookingForward = true;
            }

            if (lookingLeft)
            {
                //Rotate around y
                rotationVector = new Vector3(0f, 180f, 0f);
            }
            else
            {
                //Rotate around y
                rotationVector = new Vector3(0f, -180f, 0f);
            }
            //Calculate rotation Quaternion
            Quaternion rotation = Quaternion.Euler(rotationVector * Time.deltaTime * rotationSpeed);


            //Works so far
            if (lookingLeft)
            {
                if (rigidbody.rotation.y >= 0.01 || rigidbody.rotation.y <= -0.01)
                {
                    //Apply rotation
                    rigidbody.MoveRotation(rigidbody.rotation * rotation);
                }
                
                if (rigidbody.rotation.y >= -0.01 && rigidbody.rotation.y <= 0.01)
                {
                    lookingForward = true;
                }
            }
            //Check after implementing right turn
            if (lookingRight)
            {
                rigidbody.MoveRotation(rigidbody.rotation * rotation);

                if (rigidbody.rotation.y == 0)
                {
                    lookingForward = true;
                }
            }
            if (lookingForward)
            {
                lookingBack = false;
                lookingLeft = false;
                lookingRight = false;

                //Move character
                moveVector = new Vector3(0f, 0f, vertical).normalized * addedSpeed;
                rigidbody.MovePosition(transform.position + moveVector);
                //Activate trigger
                animator.SetTrigger("isWalking");
                //set true
                animator.SetBool("currentlyWalking", true);
                //set walking to true
                isWalking = true;
            }
        }

        //Move backwards, negative key
        if (Input.GetKey(KeyCode.S))
        {
            //Rotate y
            rotationVector = new Vector3(0f, 180f, 0f);
            //Calculate rotation Quaternion
            Quaternion rotation = Quaternion.Euler(rotationVector * Time.deltaTime * rotationSpeed);

            //Starting from looking forward
            if (lookingForward)
            {
                //Rotate the character, TARGET IS 1
                if (rigidbody.rotation.y <= 0.99 || rigidbody.rotation.y >= 0.97)
                {
                    //Apply rotation
                    rigidbody.MoveRotation(rigidbody.rotation * rotation);
                }
                if (rigidbody.rotation.y <= 0.99 && rigidbody.rotation.y >= 0.97)
                {
                    lookingBack = true;
                }
                //Rotate the character, TARGET IS -1
                if (rigidbody.rotation.y >= -0.99 || rigidbody.rotation.y <= -0.97)
                {
                    //Apply rotation
                    rigidbody.MoveRotation(rigidbody.rotation * rotation);
                }
                if (rigidbody.rotation.y >= -0.99 && rigidbody.rotation.y <= -0.97)
                {
                    lookingBack = true;
                }
            }


            

            




            if (lookingBack)
            {
                lookingForward = false;
                lookingLeft = false;
                lookingRight = false;

                //Move character, minus to walk backwards
                moveVector = new Vector3(0f, 0f, -vertical).normalized * addedSpeed;
                rigidbody.MovePosition(transform.position - moveVector);
                //Set trigger, Start animation after rotating
                animator.SetTrigger("isWalking");
                //Set true
                animator.SetBool("currentlyWalking", true);
                //set walking to true
                isWalking = true;
            }
        }
        //Move left, negative key
        if (Input.GetKey(KeyCode.A))
        {
            if (lookingBack)
            {
                //Rotate around y
                rotationVector = new Vector3(0f, 180f, 0f);
            }
            else
            {
                //Rotate around y
                rotationVector = new Vector3(0f, -180f, 0f);
            }
            //Calculate rotation Quaternion
            Quaternion rotation = Quaternion.Euler(rotationVector * Time.deltaTime * rotationSpeed);

            if (lookingForward)
            {
                //Rotate the character
                if (rigidbody.rotation.y != -0.7f && rigidbody.rotation.y >= -0.7f)
                {
                    //Apply rotation
                    rigidbody.MoveRotation(rigidbody.rotation * rotation);
                }
                //Don't rotate too fast, limit rotation
                if (rigidbody.rotation.y <= -0.7f)
                {
                    //move character
                    moveVector = new Vector3(horizontal, 0f, 0f).normalized * addedSpeed;
                    rigidbody.MovePosition(transform.position + moveVector);
                    //Set trigger, start animation after rotating
                    animator.SetTrigger("isWalking");
                    //Set true
                    animator.SetBool("currentlyWalking", true);
                    //Set walking to true
                    isWalking = true;
                }
            }

            if (lookingBack)
            {
                if (rigidbody.rotation.y != 0.7f && rigidbody.rotation.y >= 0.7f)
                {
                    //Apply rotation
                    rigidbody.MoveRotation(rigidbody.rotation * rotation);
                }
                //Don't rotate too fast, limit rotation
                if (rigidbody.rotation.y <= 0.7f)
                {

                    //move character
                    moveVector = new Vector3(horizontal, 0f, 0f).normalized * addedSpeed;
                    rigidbody.MovePosition(transform.position + moveVector);
                    //Set trigger, start animation after rotating
                    animator.SetTrigger("isWalking");
                    //Set true
                    animator.SetBool("currentlyWalking", true);
                    //Set walking to true
                    isWalking = true;

                    lookingLeft = true;
                }
            }
            if (!lookingBack && !lookingLeft)
            {
                //Rotate the character
                if (rigidbody.rotation.y != -0.7f && rigidbody.rotation.y >= -0.7f)
                {
                    //Apply rotation
                    rigidbody.MoveRotation(rigidbody.rotation * rotation);
                }
                //Don't rotate too fast, limit rotation
                if (rigidbody.rotation.y <= -0.7f)
                {
                    //move character
                    moveVector = new Vector3(horizontal, 0f, 0f).normalized * addedSpeed;
                    rigidbody.MovePosition(transform.position + moveVector);
                    //Set trigger, start animation after rotating
                    animator.SetTrigger("isWalking");
                    //Set true
                    animator.SetBool("currentlyWalking", true);
                    //Set walking to true
                    isWalking = true;

                    lookingLeft = true;
                }
            }
            if (lookingLeft)
            {
                lookingBack = false;
                lookingForward = false;
                lookingRight = false;

                //move character
                moveVector = new Vector3(horizontal, 0f, 0f).normalized * addedSpeed;
                rigidbody.MovePosition(transform.position + moveVector);
                //Set trigger, start animation after rotating
                animator.SetTrigger("isWalking");
                //Set true
                animator.SetBool("currentlyWalking", true);
                //Set walking to true
                isWalking = true;
            }
        }
        //Move right, positive key
        if (Input.GetKey(KeyCode.D))
        {
            //Move character
            moveVector = new Vector3(horizontal, 0f, 0f).normalized * addedSpeed;
            rigidbody.MovePosition(transform.position + moveVector);
            //add animation
        }


        //if not walking reset trigger and 
        if (!isWalking)
        {
            animator.SetBool("currentlyWalking", false);
            animator.ResetTrigger("isWalking");
        }
    }


}
