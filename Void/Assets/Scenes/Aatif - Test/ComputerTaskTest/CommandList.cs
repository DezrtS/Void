using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandList : MonoBehaviour
{
   public static CommandList instance;

    public Stack<ComputerCommandTest> commands = new Stack<ComputerCommandTest>();


    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }


    public void UndoLast()
    {
        if(commands.Count == 0)
        {
            Debug.Log("No Items in stack");
        }
        else
        {
            var lastCommand = commands.Pop();
            lastCommand.Undo();
        }
    }

    public void AddToStack(ComputerCommandTest command)
    {
        commands.Push(command);
    }







}
