using _Framework.Service._Common;

namespace Reader.Abstraction.Reports.Models;

public record GisServiceEvent : ServiceEvent
{
    public GisServiceEvent(string message) : base(message)
    {
    }
    public static GisServiceEvent ApplicationStart()
    {
        return new GisServiceEvent("اپلیکیشن اجرا شد.");
    }
    public static GisServiceEvent ApplicationFinished()
    {
        return new GisServiceEvent("اپلیکیشن به پایان رسید.");
    }
    public static GisServiceEvent CleanedOldTemps()
    {
        return new GisServiceEvent("موارد قدیمی پاک سازی شد.");
    }
    public static GisServiceEvent LoadedLayers(int layersCount)
    {
        return new GisServiceEvent($"تعداد [{layersCount}] لایه برای اسکن بارگزاری شد.");
    }
    public static GisServiceEvent StartLoadingLayersObjectsCount()
    {
        return new GisServiceEvent($"آغاز بارگزاری تعداد لایه ها.");
    }
    public static GisServiceEvent EndLoadingLayersObjectsCount()
    {
        return new GisServiceEvent($"پایان بارگزاری تعداد لایه ها.");
    }
}

