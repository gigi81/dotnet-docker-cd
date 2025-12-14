using System.IO.Abstractions;
using Docker.DotNet;
using Grillisoft.Gicd.Abstractions;

namespace Grillisoft.Gicd.Service;

public class DataPathProvider : IDataPathProvider
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<DataPathProvider> _logger;
    private string? _path;

    public DataPathProvider(IFileSystem fileSystem, ILogger<DataPathProvider> logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
    }
    
    public async Task<string> GetDataPath()
    {
        if(_path != null)
            return _path;
        
        const string dataPath = "/data";
        
        //get docker mount point for dataPath
        var mountPoint = await GetMountPointByDestination(dataPath);
        if (mountPoint.Equals(dataPath))
            return _path = mountPoint;
        
        var mountPointInfo = _fileSystem.DirectoryInfo.New(mountPoint);

        if (mountPointInfo.Exists)
        {
            //check if mountPointInfo is a symbolic link
            if (mountPointInfo.LinkTarget is dataPath)
            {
                _logger.LogInformation("Symbolic link from {MountPoint} to {DataPath} already exists", mountPoint, dataPath);
                return _path = mountPoint;
            }
            
            throw new InvalidOperationException($"Symbolic link from {mountPoint} already exists but its not pointing to {dataPath}");
        }

        _logger.LogInformation("Creating symbolic link from {MountPoint} to {DataPath}", mountPoint, dataPath);
        mountPointInfo.Parent?.Create();
        mountPointInfo.CreateAsSymbolicLink(dataPath);

        return _path = mountPoint;
    }

    private async Task<string> GetMountPointByDestination(string path)
    {
        var hostname = Environment.GetEnvironmentVariable("HOSTNAME");
        _logger.LogInformation("Getting container info for hostname {Hostname}", hostname);
        var container = await Client.Containers.InspectContainerAsync(hostname);
        
        var mountPoint = container.Mounts.FirstOrDefault(m => m.Destination.Equals(path))
            ?? throw new Exception($"Could not find docker mount point for volume {path}");

        return mountPoint.Source;
    }

    private static DockerClient Client
    {
        get
        {
            if (field != null)
                return field;
            
            field = new DockerClientConfiguration( new Uri("unix:///var/run/docker.sock"))
                .CreateClient();
            
            return field;
        }
    }
}