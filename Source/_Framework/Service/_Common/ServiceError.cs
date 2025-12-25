namespace _Framework.Service._Common;

public abstract record ServiceError
{
    public DateTime DateTime { get; }
    public string Error { get; }
    protected ServiceError(string error)
    {
        DateTime = DateTime.Now;
        Error = error;
    }
}