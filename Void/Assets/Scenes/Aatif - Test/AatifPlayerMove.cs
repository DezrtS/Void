using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AatifPlayerMove : MonoBehaviour
{
    public int health = 10;




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
        if (Input.GetKeyDown(KeyCode.W))
        {
            transform.Translate(Vector3.forward);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            transform.Translate(Vector3.back);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            transform.Translate(Vector3.left);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            transform.Translate(Vector3.right);
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
