using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace _Framework.Infrastructures
{
    public abstract class BulkRepository<T> : IBulkRepository<T> , IDisposable
    {
        private readonly int _cacheSize;
        private readonly ILogger<BulkRepository<T>> _logger;
        private readonly List<T> _objects = new();
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        protected BulkRepository(int cacheSize , ILogger<BulkRepository<T>> logger)
        {
            _cacheSize = cacheSize;
            _logger = logger;
            Task.Run(BackgroundWorkerAsync);
        }
        public async Task AddRangeAsync(IReadOnlyList<T> items)
        {
            while (true)
            {
                var reachedLimit = false;
                int count;
                lock (_objects)
                {
                    if (_objects.Count >= _cacheSize)
                        reachedLimit = true;

                    count = _objects.Count;
                }

                if (reachedLimit)
                {
                    _logger.LogWarning("Repository cache reached max size [{currentSize}].",
                        count);
                    await Task.Delay(1000);
                    continue;
                }

                lock (_objects)
                {
                    _objects.AddRange(items);
                    return;
                }
            }
        }

        public async Task FlushAsync()
        {
            List<T>? copy = null;
            try
            {
                lock (_objects)
                {
                    if (_objects.Count == 0)
                        return;

                    copy = _objects.ToList();
                    _objects.Clear();
                }
                _logger.LogInformation("Start flushing {count} objects to db.", copy.Count);
                await PerformDbOperationAsync(copy);
                _logger.LogInformation("Flushed {count} objects to db." , copy.Count);
            }
            catch (Exception)
            {
                if (copy == null)
                    return;

                lock (_objects)
                {
                    _objects.AddRange(copy);
                }

                throw;
            }
        }

        protected abstract Task PerformDbOperationAsync(List<T> items);

        private async Task BackgroundWorkerAsync()
        {
            const int insertBatchSize = 10;
            _logger.LogInformation("Started background database object saver.");
            while (_cancellationTokenSource.IsCancellationRequested == false)
            {
                try
                {
                    var waitForBatch = false;
                    lock (_objects)
                    {
                        if (_objects.Count < insertBatchSize)
                        {
                            waitForBatch = true;
                        }
                    }

                    if (waitForBatch)
                    {
                        await Task.Delay(10, _cancellationTokenSource.Token);
                        continue;
                    }
                    await FlushAsync();
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, "Background worker failed to sync objects.");
                }
            }
            _logger.LogInformation("Stopped background database object saver.");
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}
