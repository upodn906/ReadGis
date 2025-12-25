namespace Reader.Abstraction.Reports.Models;

public record GisLayerStats
{
    private int _responseTimeCount = 0;
    public GisLayerStats(int code, string? name)
    {
        Code = code;
        Name = $"{name} ({code})";
    }


    public int Code { get; }
    public string? Name { get; }
    public int? TotalObjects { get; private set; }
    public int? ScannedObjects { get; private set; }
    public int? RemainedObjects => TotalObjects - ScannedObjects;

    public decimal? Progress
    {
        get
        {
            try
            {
                if (TotalObjects == null || ScannedObjects == null)
                    return null;
                return Math.Round(ScannedObjects.Value / (decimal)TotalObjects.Value * 100, 2);
            }
            catch
            {
                return null;
            }
        }
    }
    public DateTime? StartDateTime { get; private set; }
    public DateTime? EndDateTime { get; private set; }
    public TimeSpan? ElapsedTime => EndDateTime - StartDateTime;
    public TimeSpan? LastResponseTime { get; private set; }
    public TimeSpan? AverageResponseTime { get; private set; }

    public void Initialize(int total)
    {
        TotalObjects = total;
        ScannedObjects = 0;
    }

    public void Scanned(int count, TimeSpan responseTime)
    {
        lock (this)
        {
            StartDateTime ??= DateTime.Now;
            ScannedObjects += count;
            if (_responseTimeCount != 0 && AverageResponseTime != null)
            {
                var totalMs = AverageResponseTime.Value.TotalMilliseconds * _responseTimeCount;
                totalMs += responseTime.TotalMilliseconds;
                _responseTimeCount++;
                var avg = totalMs / _responseTimeCount;
                AverageResponseTime = TimeSpan.FromMilliseconds(avg);
            }
            else
            {
                _responseTimeCount = 1;
                AverageResponseTime = responseTime;
            }
            LastResponseTime = responseTime;
        }
    }
    public void Finished()
    {
        EndDateTime = DateTime.Now;
    }
}