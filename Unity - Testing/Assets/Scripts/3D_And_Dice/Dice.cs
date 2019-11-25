using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Dice : MonoBehaviour
{
    public GameObject diceObject;

    public int Throw_Power;

    public int Rotation_Power;

    private Rigidbody diceRigid;

    private GameObject[] numberObjects;

    private bool isGrounded;

    //Click dice to throw
    void OnMouseDown()
    {
        ThrowDice();
    }

    // Start is called before the first frame update
    void Start()
    {
        //Get dice as object
        diceObject = this.gameObject;

        //Get rigidbody
        diceRigid = diceObject.GetComponent<Rigidbody>();

        //Get number objects and add into list
        numberObjects = GameObject.FindGameObjectsWithTag("DiceNumber");
    }

    void OnCollisionStay(Collision hit)
    {
        isGrounded = true;

        //Show dice side facing up
        Debug.Log(GetDiceFace(numberObjects));
    }

    void OnCollisionExit(Collision hit)
    {
        isGrounded = false;
    }

    //Function for finding side facing up, takes array of objects that are attached to the dice faces
    public string GetDiceFace(GameObject[] _diceArray)
    {
        //dice side facing up
        GameObject highestNumber;

        //Sort all sides by y pos
        numberObjects = numberObjects.OrderBy(numberObjects => numberObjects.transform.position.y).ToArray();

        //dice side facing up is the last object in array
        highestNumber = numberObjects[numberObjects.Length - 1];
        return highestNumber.name;
    }

    
    //Throw the dice and rotate
    void ThrowDice()
    {
        //grounded = allow and execute throw
        if (isGrounded)
        {
            //dice jumps into air
            diceRigid.AddForce(Vector3.up * Throw_Power);
            diceRigid.AddTorque(Vector3.right * Rotation_Power);
            diceRigid.AddTorque(Vector3.forward * Rotation_Power);
            diceRigid.AddTorque(Vector3.up * Rotation_Power);
        }
    }
}
