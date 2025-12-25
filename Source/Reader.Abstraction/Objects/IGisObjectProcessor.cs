namespace Reader.Abstraction.Objects
{
    public interface IGisObjectProcessor
    {
        Task ProcessAsync(IGisObject obj);
        Task InitializeAsync();
    }
}
