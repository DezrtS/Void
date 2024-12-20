using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ChangePeiceCommand : ICommand
{
    private GameObject character;
    private GameObject newPeice;
    private List<GameObject> peiceOptions;

    public ChangePeiceCommand(GameObject character, GameObject newPeice, List<GameObject> peiceOptions)
    {
        this.character = character;
        this.newPeice = newPeice;
        this.peiceOptions = peiceOptions;
    }

    public void Execute()
    {
        foreach (var peice in peiceOptions)
        {
            peice.SetActive(peice == newPeice);
        }
    }
    public void Undo()
    {
        throw new System.NotImplementedException();
    }
}
