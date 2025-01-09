public interface INetworkedUseable : IUseable
{
    public abstract void UseClientSide();
    public abstract void UseServerSide();
    public abstract void StopUsingClientSide();
    public abstract void StopUsingServerSide();
}