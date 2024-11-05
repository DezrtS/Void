using UnityEngine;

public interface ILook
{
    public abstract Vector3 LookForward { get; set; }
    public abstract Vector2 GetLookInput();
}