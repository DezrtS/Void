using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PeiceCustomizer
{
    public GameObject character;
    public List<GameObject> peiceOptions; // List of hair objects

    private List<ICommand> commands = new List<ICommand>();

    public void AddCommand(ICommand command)
    {
        commands.Add(command);
    }

    public void ExecuteCommands()
    {
        foreach (var command in commands)
        {
            command.Execute();
        }
        commands.Clear();
    }

 
}