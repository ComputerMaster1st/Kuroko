using Discord;
using Timer = System.Timers.Timer;

namespace Kuroko.Services
{
    internal static class MessageTimeoutService
    {
        public static readonly List<MessageTimeoutContainer> _containers = new();

        public static void SetTimeout(this IUserMessage msg, int minutes)
        {
            var container = new MessageTimeoutContainer(msg, minutes);
            container.MessageDeletedEvent += MessageDeletedEvent;

            _containers.Add(container);
        }

        public static void ResetTimeout(this IUserMessage msg)
        {
            var container = _containers.FirstOrDefault(x => x.MessageId == msg.Id);
            container.ResetTimer();
        }

        public static void DeleteTimeout(this IUserMessage msg)
        {
            var container = _containers.FirstOrDefault(x => x.MessageId == msg.Id);

            if (container is null)
                return;

            container.Dispose();
            _containers.Remove(container);
        }

        private static void MessageDeletedEvent(object sender)
            => _containers.Remove(sender as MessageTimeoutContainer);
    }

    internal static class TimerExtensions
    {
        public static void Reset(this Timer timer)
        {
            timer.Stop();
            timer.Start();
        }
    }

    internal class MessageTimeoutContainer : IDisposable
    {
        private readonly IUserMessage _message;
        private readonly Timer _timer;
        private bool disposedValue;

        public delegate void MessageDeleted(object sender);
        public event MessageDeleted MessageDeletedEvent;

        public ulong MessageId => _message.Id;

        public MessageTimeoutContainer(IUserMessage message, int minutes)
        {
            _message = message;
            _timer = new()
            {
                Interval = TimeSpan.FromMinutes(minutes).TotalMilliseconds,
                Enabled = true
            };

            _timer.Elapsed += (s, e) => Task.Factory.StartNew(() => ElapsedEvent(s, e));
        }

        public void ResetTimer()
            => _timer.Reset();

        private async Task ElapsedEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timer.Enabled = false;
            _timer.Stop();

            await _message.DeleteAsync();
            MessageDeletedEvent?.Invoke(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _timer.Stop();
                    _timer.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
