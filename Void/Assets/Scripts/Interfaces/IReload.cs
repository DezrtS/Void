public interface IReload
{
    public abstract int MaxAmmo { get; }
    public abstract int Ammo { get; set;  }
    public abstract bool CanReload();
    public abstract void RequestReload();
    public abstract void Reload();
}