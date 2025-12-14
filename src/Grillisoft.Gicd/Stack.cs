using System.IO.Abstractions;
using Ductus.FluentDocker.Model.Compose;
using Ductus.FluentDocker.Services;
using Ductus.FluentDocker.Services.Impl;
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

    private IHostService DockerHost
    {
        get
        {
            var hosts = new Hosts().Discover();
            var docker = hosts.FirstOrDefault(x => x.IsNative)
                         ?? hosts.FirstOrDefault(x => x.Name == "default");
            
            if(docker == null)
                throw new InvalidOperationException("No Docker host found.");
            
            return docker;
        }
    }
    
    public Task Deploy(IDirectoryInfo directory, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deploying stack {StackName}", directory.Name);
        
        var dockerFile = directory.File("docker-compose.yml");

        var config = new DockerComposeConfig
        {
            ComposeFilePath = [dockerFile.FullName],
            ForceRecreate = false,
            RemoveOrphans = true,
            StopOnDispose = false,
            AlwaysPull = true,
            ProjectDirectory = directory.FullName,
            ComposeVersion = ComposeVersion.V2
        };
        
        var svc = new DockerComposeCompositeService(DockerHost, config);
        
        return Task.Run(() => svc.Start(), cancellationToken);
    }
}