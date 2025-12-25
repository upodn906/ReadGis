namespace Reader.Abstraction.Reports.Models;

public record GisServiceStats
{
    private int _scanCountInLastSecs = 0;
    private bool _finished = true;

    public string Status { get; private set; } = "در انتظار اجرا";

    public int? TotalLayers { get; set; }
    public int ScannedLayers { get; private set; }
    public int? RemainedLayers => TotalLayers - ScannedLayers;

    public int? TotalObjects { get; private set; }
    public int? ScannedObjects { get; private set; }
    public int? RemainedObjects => TotalObjects - ScannedObjects;

    public DateTime? StartDateTime { get; private set; }
    public TimeSpan? ElapsedTime {
        get
        {
            if (_finished == false)
                return DateTime.Now - StartDateTime;
            return FinishDateTime - StartDateTime;
        }
    }
    public TimeSpan? RemainedTime { get; private set; }
    public DateTime? FinishDateTime { get; private set; }
    public int ErrorsCount { get; private set; } = 0;

    public decimal? Progress
    {
        get
        {
            if (TotalObjects == null || ScannedObjects == null)
                return null;
            return Math.Round(ScannedObjects.Value / (decimal)TotalObjects.Value * 100, 2);
        }
    }

    public void Initialize()
    {
        StartDateTime = DateTime.Now;
        Status = "درحال اسکن";
        _finished = false;
        CalculateEndTime();
    }

    public void Finish()
    {
        Status = "پایان یافته";
        FinishDateTime = DateTime.Now;
        _finished = true;
    }
    public void IncreaseScannedLayer()
    {
        lock (this)
        {
            ScannedLayers += 1;
        }
    }

    public void IncreaseScannedObjects(int count)
    {
        lock (this)
        {
            ScannedObjects ??= 0;
            ScannedObjects += count;
            _scanCountInLastSecs += count;
        }

     
    }

    public void IncreaseTotalObjects(int count)
    {
        lock (this)
        {
            TotalObjects ??= 0;
            TotalObjects += count;
        }
    }

    public void IncreaseErrorCount()
    {
        lock (this)
        {
            ErrorsCount++;
        }
    }
    private void CalculateEndTime()
    {
        const int delay = 5000;
        Task.Run(async () =>
        {
            while (_finished == false)
            {
                lock (this)
                {
                    if (RemainedObjects != null)
                    {
                        var count = RemainedObjects.Value / (_scanCountInLastSecs + 1);
                        var totalMs = count * delay;
                        var totalTimeSpan = TimeSpan.FromMilliseconds(totalMs);
                        RemainedTime = totalTimeSpan;
                        FinishDateTime = DateTime.Now + totalTimeSpan;
                    }

                    _scanCountInLastSecs = 0;
                }
                await Task.Delay(delay);
            }
        });
    }
}