using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObserverMichael : MonoBehaviour
{
    public TextMeshProUGUI boots;
    public TextMeshProUGUI hair;
    //public Transform hairsTransform;
    //public Transform bootsTransform;
        
    // Start is called before the first frame update
    void Start()
    {
        PeiceCustomizerGUI.activePeice += CurrentClothingName;
        
       
    }

    void CurrentClothingName(string active)
    {
        
        
        Debug.Log("CLOTHING CHANGED!!" + active);
        if (active.Contains("H"))
        {
            hair.text = "Active Hair: " + active;
        }

        if (active.Contains("Boots"))
        {
            boots.text = ("Active boots: " + active);    
        }
        
        
        
        
        
        /*//hairsTransform = transform.Find("Hairs");

        if (hairsTransform != null)
        {
            Debug.Log("clothing name");
            foreach (Transform child in hairsTransform)
            {
                if (child.gameObject.activeSelf)
                {

                    text.text = "Active Hair: " + child.name;
                    break;
                }
            }
            
        }
        
        //bootsTransform = transform.Find("Bootss");

        if (bootsTransform != null)
        {

            foreach (Transform child in bootsTransform)
            {
                if (child.gameObject.activeSelf)
                {
                
                    text2.text = "Active Boots: " + child.name;
                    break;
                }
            }
        }*/
    }
    
    // Update is called once per frame
    void Update()
    {
        //PeiceCustomizerGUI.onButtonClicked = CurrentClothingName;
    }
}
