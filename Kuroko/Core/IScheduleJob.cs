using FluentScheduler;

namespace Kuroko.Core
{
    public interface IScheduleJob
    {
        void ScheduleJob(Registry registry);
    }
}