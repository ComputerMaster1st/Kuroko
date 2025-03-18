using FluentScheduler;

namespace Kuroko.Jobs;

public interface IScheduleJob
{
    void ScheduleJob(Registry registry);
}