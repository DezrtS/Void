using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCommand : ComputerCommandTest
{
    Transform gameObj;
    Vector2 movement;


    public MoveCommand(Transform gameObj, Vector2 movement)
    {
        this.gameObj = gameObj;
        this.movement = movement;
    }
    public override void Execute()
    {
        gameObj.Translate(movement);
        Debug.Log("Command Executed");
    }

    public override void Undo()
    {
        gameObj.Translate(-movement);
        Debug.Log("Undo Executed");
    }
}
