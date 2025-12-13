using System.IO.Abstractions;
using Grillisoft.Gicd.Abstractions;
using Microsoft.Extensions.Logging;

namespace Grillisoft.Gicd;

public class DeployManager
{
    private readonly IRepository _repository;
    private readonly IStack _stack;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<DeployManager> _logger;

    public DeployManager(
        IRepository repository,
        IStack stack,
        IFileSystem fileSystem,
        ILogger<DeployManager> logger)
    {
        _repository = repository;
        _stack = stack;
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task Execute(CancellationToken cancellationToken)
    {
        var repoDirectory = _fileSystem.CurrentDirectory().SubDirectory("repo");

        if (!await _repository.Poll(cancellationToken))
        {
            _logger.LogInformation("No changes detected in the repository. Deployment skipped.");
            return;
        }
        
        await _repository.Pull(repoDirectory, cancellationToken);

        foreach (var stackDirectory in repoDirectory.SubDirectory("stacks").GetDirectories())
        {
            _logger.LogInformation("Deploying stack {StackName}", stackDirectory.Name);
            await _stack.Deploy(stackDirectory, cancellationToken);
        }
    }
}