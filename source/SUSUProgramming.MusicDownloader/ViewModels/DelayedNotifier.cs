using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    public class DelayedNotifier
    {
        private readonly System.Timers.Timer eventTimer;
        private readonly Action? onNotify;
        private readonly Func<CancellationToken, Task>? onNotifyAsync;

        private CancellationTokenSource cts;

        [MemberNotNullWhen(true, nameof(onNotifyAsync))]
        [MemberNotNullWhen(false, nameof(onNotify))]
        private bool IsAsync => onNotifyAsync != null;

        public DelayedNotifier(Action onNotify, double delayMilliseconds)
        {
            this.onNotify = onNotify;
            cts = new();
            eventTimer = new(delayMilliseconds)
            {
                AutoReset = false
            };
            eventTimer.Elapsed += OnTimerElapsed;
        }

        public DelayedNotifier(Func<CancellationToken, Task> onNotifyAsync, double delayMilliseconds)
        {
            this.onNotifyAsync = onNotifyAsync;
            cts = new();
            eventTimer = new(delayMilliseconds)
            {
                AutoReset = false
            };
            eventTimer.Elapsed += OnTimerElapsed;
        }

        private async void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            await RaiseAsync();
        }

        public void NotifyUpdate()
        {
            eventTimer.Stop();
            eventTimer.Start();
        }

        public async Task RaiseAsync()
        {
            Cancel();
            if (IsAsync)
            {
                await onNotifyAsync(cts.Token);
            }
            else
            {
                onNotify();
            }
        }

        public void Cancel()
        {
            eventTimer.Stop();
            cts.Cancel();
            cts.Dispose();
            cts = new();
        }
    }
}
