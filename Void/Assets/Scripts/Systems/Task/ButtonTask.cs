using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTask : Task
{
    [SerializeField] private Material activationMaterial;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Press();
        }
    }

    public void Press()
    {
        CompleteSubtask(0);
    }

    public override void CompleteTask()
    {
        base.CompleteTask();
        TaskManager.Instance.RemoveTask(this);
        GetComponent<MeshRenderer>().material = activationMaterial;
    }
}
