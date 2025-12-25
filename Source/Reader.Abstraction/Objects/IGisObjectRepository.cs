using _Framework.Infrastructures;

namespace Reader.Abstraction.Objects
{
    public interface IGisObjectRepository<in T> : IBulkRepository<T> where T : IGisObject
    {
        Task CleanAsync();
    }

    public interface IGisObjectRepository : IGisObjectRepository<IGisObject>
    {

    }
}
