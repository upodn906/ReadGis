using Reader.Abstraction.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Reader.Console
{
    public static class Reporter
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
        };
        public static void Report(IGisServiceReport report)
        {
            var reportDirectory = "Reports";
            Directory.CreateDirectory(reportDirectory);
            var normalDateTime = report.Stats.StartDateTime.GetValueOrDefault().ToString("yyyy_MM_dd HH_mm_ss");
            var reportName = $"{normalDateTime}.json";
            var reportFilePath = Path.Combine(reportDirectory, reportName);
            File.WriteAllText(reportFilePath , JsonSerializer.Serialize(report , JsonOptions));
        }
    }
}
