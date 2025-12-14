using System.IO.Abstractions;
using CliWrap;
using Grillisoft.Gicd.Abstractions;
using Microsoft.Extensions.Logging;

namespace Grillisoft.Gicd;

public class Stack : IStack
{
    private readonly ILogger<Stack> _logger;

    public Stack(ILogger<Stack> logger)
    {
        _logger = logger;
    }

    public async Task Deploy(IDirectoryInfo directory, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Pulling stack {StackName}", directory.Name);
        await DockerCompose(directory, "compose pull --policy always", cancellationToken);
        
        _logger.LogInformation("Deploying stack {StackName}", directory.Name);
        await DockerCompose(directory, "compose up -d", cancellationToken);
        
        _logger.LogInformation("Completed stack {StackName} deployment", directory.Name);
    }
    
    async Task DockerCompose(IDirectoryInfo directory, string args, CancellationToken cancellationToken)
    {
        var result = await Cli.Wrap("docker")
            .WithArguments(args)
            .WithWorkingDirectory(directory.FullName)
            .WithStandardErrorPipe(PipeTarget.ToDelegate(s => _logger.LogError(s)))
            .WithStandardOutputPipe(PipeTarget.ToDelegate(s => _logger.LogInformation(s)))
            .ExecuteAsync(cancellationToken);
        
        if(!result.IsSuccess)
            _logger.LogError($"docker failed with exit code {result.ExitCode}");
    }
}