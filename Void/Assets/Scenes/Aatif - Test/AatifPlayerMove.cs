using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AatifPlayerMove : MonoBehaviour
{
    /*
     * This is the player movement class, it may be changed later to allow more functions, it also may require button remapping, and it will need to store its local buttom mapping 
     * so it is not changed across the server
     */

    public int health = 10;
    float speed = 15;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        moveCharacter();
    }

    void moveCharacter()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * Time.deltaTime * speed);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back * Time.deltaTime * speed);
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * Time.deltaTime * speed);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * Time.deltaTime * speed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Dmg")
        {
            health = health - 1;
        }
    }

}
