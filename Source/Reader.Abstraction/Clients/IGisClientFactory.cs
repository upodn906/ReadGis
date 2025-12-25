namespace Reader.Abstraction.Clients
{
    public interface IGisClientFactory
    {
        IGisClient Create(GisProvider provider);
    }
}
