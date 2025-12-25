using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reader.Abstraction.Services;
using Reader.Abstraction.Services.Models;
using Reader.Console;
using Reader.Infrastructures.Bootstrapper;
using Topshelf;
using static Reader.Console.SyncService;
AppDomain.CurrentDomain.UnhandledException += (sender, ex) =>
{
    Console.WriteLine(ex);
};
HostFactory.Run(x =>
{
    x.Service<ReaderService>(s =>
    {
        s.ConstructUsing(name => new ReaderService());
        s.WhenStarted(svc => svc.Run());
        s.WhenStopped(svc => { });
    });
    x.RunAsLocalSystem();

    x.SetDescription("Gis Reader Service");
    x.SetDisplayName("Gis Reader Service");
    x.SetServiceName("GisReaderService");

    x.StartAutomatically();
});
