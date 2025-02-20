public interface IUseable
{
    public delegate void UseHandler(IUseable useable, bool isUsing);
    public event UseHandler OnUsed;
    public bool IsUsing {  get; }
    public abstract bool CanUse();
    public abstract bool CanStopUsing();
    public abstract void Use();
    public abstract void StopUsing();
}

public interface INetworkUseable : IUseable
{
    public abstract void RequestUse();
    public abstract void RequestStopUsing();
}