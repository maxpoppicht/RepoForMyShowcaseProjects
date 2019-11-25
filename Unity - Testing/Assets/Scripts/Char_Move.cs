using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Char_Move : MonoBehaviour
{
    #region <Components>
    Rigidbody rigidbody;

    Camera mainCam;

    #endregion
    [Range(0,1)]
    public float movementMultiplier;

    [Range(0, 10)]
    public int mouseSpeed;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        mainCam = Camera.main;
        
    }

    // Update is called once per frame
    void Update()
    {
        Movement();



    }

    void Movement()
    {
        var vertical = Input.GetAxisRaw("Vertical");
        var horizontal = Input.GetAxisRaw("Horizontal");
        //Calculate the movement vector, normalize if strafing is too fast
        Vector3 moveVector = new Vector3(horizontal, 0f, vertical) * movementMultiplier;


        var mouseX = Input.GetAxisRaw("Mouse X");

        var mouseY = Input.GetAxisRaw("Mouse Y");

        Vector3 rotVector = new Vector3(mouseX, mouseY, 0f);

        Quaternion rotation = Quaternion.Euler(rotVector * Time.deltaTime * mouseSpeed);

        //Move the Character
        rigidbody.MovePosition(this.transform.position + moveVector);
        //Rotate around mouse
        mainCam.transform.rotation = rotation;
    }
}
