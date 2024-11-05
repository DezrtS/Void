public interface IReload
{
    public abstract int MaxAmmo { get; }
    public abstract int Ammo { get; }
    public abstract float ReloadSpeed { get; }
    public abstract void Reload();
}