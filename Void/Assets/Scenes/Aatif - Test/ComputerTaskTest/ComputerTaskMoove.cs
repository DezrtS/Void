using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerTaskMoove : MonoBehaviour
{


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            MoveCommand goUp = new MoveCommand(transform, Vector2.up * 100);
            goUp.Execute();
            CommandList.instance.AddToStack(goUp);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            MoveCommand goDown = new MoveCommand(transform, Vector2.down * 100);
            goDown.Execute();
            CommandList.instance.AddToStack(goDown);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            MoveCommand goLeft = new MoveCommand(transform, Vector2.left * 100);
            goLeft.Execute();
            CommandList.instance.AddToStack(goLeft);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            MoveCommand goRight = new MoveCommand(transform, Vector2.right * 100);
            goRight.Execute();
            CommandList.instance.AddToStack(goRight);
        }
        
       

        Mathf.Clamp(transform.position.y,-Screen.height,Screen.height);
        Mathf.Clamp(transform.position.x, -Screen.width, Screen.width);
    }

 
    public void UIUndo()
    {      
        CommandList.instance.UndoLast();
    }

}
