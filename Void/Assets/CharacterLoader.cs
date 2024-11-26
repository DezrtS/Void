using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterLoader : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] SceneObjectActivator hairActivator;
    [SerializeField] SceneObjectActivator bootsActivator;
    void Start()
    {  
        bootsActivator.ApplyActiveObject();
        hairActivator.ApplyActiveObject();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
