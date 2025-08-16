namespace HangfireJobs.Jobs;

public interface IJob
{
    public Task Execute(CancellationToken cancellationToken);
}