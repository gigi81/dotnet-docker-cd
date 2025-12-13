namespace Grillisoft.Gicd.Service;

// A minimal background worker that is registered as a hosted service.
public class DeployJob : BackgroundService
{
    private readonly DeployManager _manager;
    private readonly ILogger<DeployJob> _logger;

    public DeployJob(DeployManager manager, ILogger<DeployJob> logger)
    {
        _manager = manager;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DeployJob started.");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _manager.Execute(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }
        catch (TaskCanceledException)
        {
            // expected on shutdown
        }

        _logger.LogInformation("DeployJob stopping.");
    }
}