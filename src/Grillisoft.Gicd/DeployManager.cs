using System.Diagnostics;
using System.IO.Abstractions;
using Grillisoft.Gicd.Abstractions;
using Microsoft.Extensions.Logging;

namespace Grillisoft.Gicd;

public class DeployManager
{
    private readonly IRepository _repository;
    private readonly IStack _stack;
    private readonly IDecryption _decryption;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<DeployManager> _logger;

    public DeployManager(
        IRepository repository,
        IStack stack,
        IDecryption decryption,
        IFileSystem fileSystem,
        ILogger<DeployManager> logger)
    {
        _repository = repository;
        _stack = stack;
        _decryption = decryption;
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task Execute(CancellationToken cancellationToken)
    {
        try
        {
            await ExecuteInternal(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred during deployment: {Message}", ex.Message);
        }
    }

    private async Task ExecuteInternal(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Starting deploy manager...");
        
        var repoDirectory = _fileSystem.CurrentDirectory().SubDirectory("repo");

        if (!await _repository.Poll(cancellationToken))
        {
            _logger.LogInformation("No changes detected in the repository. Deployment skipped.");
        }
        else
        {
            await _repository.Pull(repoDirectory, cancellationToken);

            foreach (var stackDirectory in repoDirectory.SubDirectory("stacks").GetDirectories())
            {
                await _decryption.Decrypt(stackDirectory, cancellationToken);
                await _stack.Deploy(stackDirectory, cancellationToken);
            }
        }
        
        _logger.LogInformation("Deploy manager finished in {Elapsed}", stopwatch.Elapsed);
    }
}