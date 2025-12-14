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
    private readonly IDataPathProvider _dataPathProvider;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<DeployManager> _logger;

    public DeployManager(
        IRepository repository,
        IStack stack,
        IDecryption decryption,
        IDataPathProvider dataPathProvider,
        IFileSystem fileSystem,
        ILogger<DeployManager> logger)
    {
        _repository = repository;
        _stack = stack;
        _decryption = decryption;
        _dataPathProvider = dataPathProvider;
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task Execute(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Starting deploy manager...");

        try
        {
            await ExecuteInternal(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred during deployment: {Message}", ex.Message);
        }
        
        _logger.LogInformation("Deploy manager finished in {Elapsed}", stopwatch.Elapsed);
    }

    private async Task ExecuteInternal(CancellationToken cancellationToken)
    {
        var dataPath = await _dataPathProvider.GetDataPath();
        var repoDirectory = _fileSystem.DirectoryInfo.New(dataPath);
        if(!repoDirectory.Exists)
            repoDirectory.Create();

        if (!await _repository.Poll(cancellationToken))
        {
            _logger.LogInformation("No changes detected in the repository. Deployment skipped.");
            return;
        }

        await _repository.Pull(repoDirectory, cancellationToken);

        var stacksDirectory = repoDirectory.SubDirectory("stacks");
        if(!stacksDirectory.Exists)
        {
            _logger.LogError("Stacks directory does not exist in the repo");
            return;
        }

        var stacks = stacksDirectory.GetDirectories();
        if (stacks.Length <= 0)
        {
            _logger.LogError("Stacks directory is empty in the repo");
            return;
        }
        
        foreach (var stack in stacks)
        {
            await _decryption.Decrypt(stack, cancellationToken);
            await _stack.Deploy(stack, cancellationToken);
        }
    }
}