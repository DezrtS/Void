using System.Collections.Generic;
using UnityEngine;

public class DraggableDropOff : MonoBehaviour
{
    public delegate void DraggableDropOffHandler(Draggable draggable, DraggableDropOff draggableDropOff);
    public static event DraggableDropOffHandler OnDropOff;

    private List<Draggable> draggables = new List<Draggable>();
    private Dictionary<Draggable, bool> proccessed = new Dictionary<Draggable, bool>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Draggable draggable))
        {
            draggables.Add(draggable);
            proccessed.Add(draggable, false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Draggable draggable))
        {
            draggables.Remove(draggable);
            proccessed.Remove(draggable);
        }
    }

    private void FixedUpdate()
    {
        for (int i = draggables.Count - 1; i >= 0; i--)
        {
            Draggable draggable = draggables[i];
            proccessed.TryGetValue(draggable, out bool state);
            if (state) return;

            if (!draggable.IsUsing)
            {
                proccessed[draggable] = true;
                OnDropOff?.Invoke(draggable, this);
                Debug.Log("Processed");
            }
        }
    }

    public void AcceptDraggable(Draggable draggable)
    {
        draggables.Remove(draggable);
        proccessed.Remove(draggable);
        draggable.RequestUse();
    }
}
