using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SUSUProgramming.MusicDownloader.Persistence
{
    internal sealed class SyncStatsRepository : IDisposable
    {
        private readonly ITrackStatsRepository stats;
        private readonly Channel<Func<CancellationToken, Task>> channel;
        private readonly CancellationTokenSource cts = new();

        public SyncStatsRepository(ITrackStatsRepository repo, ILogger<SyncStatsRepository> logger)
        {
            stats = repo;
            channel = Channel.CreateUnbounded<Func<CancellationToken, Task>>(new UnboundedChannelOptions()
            {
                AllowSynchronousContinuations = true,
                SingleReader = true,
                SingleWriter = true,
            });
            _ = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.LongRunning).StartNew(
                async (token) =>
                {
                    await foreach (var work in channel.Reader.ReadAllAsync())
                    {
                        try
                        {
                            await work(cts.Token);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Couldn't complete synchronized db query because of an exception.");
                        }
                    }
                },
                cts.Token);
        }

        public void Dispose()
        {
            cts.Cancel();
        }
    }
}
