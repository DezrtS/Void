public interface IUseable
{
    public delegate void UseHandler(bool onBegin);
    public event UseHandler OnUsed;
    public bool IsUsing {  get; }
    public abstract bool CanUse();
    public abstract bool Use();
    public abstract bool StopUsing();
}