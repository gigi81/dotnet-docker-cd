using System.IO.Abstractions;
using CliWrap;
using Grillisoft.Gicd.Abstractions;
using Microsoft.Extensions.Logging;

namespace Grillisoft.Gicd;

public class SopsDecrypt : IDecryption
{
    private readonly ILogger<SopsDecrypt> _logger;

    public SopsDecrypt(ILogger<SopsDecrypt> logger)
    {
        _logger = logger;
    }
    
    public async Task Decrypt(IDirectoryInfo directory, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Decrypting folder {Path}", directory.FullName);
        
        if (!directory.Exists)
            return;

        var files = directory.GetFiles("*.enc.*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            _logger.LogInformation("Decrypting file {Path}", file.FullName);
            await SopsDecryptFile(file, cancellationToken);            
        }
    }
    
    async Task SopsDecryptFile(IFileInfo file, CancellationToken cancellationToken)
    {
        var result = await Cli.Wrap("sops")
            .WithArguments($"-d --output \"{file.Name}\" \"{file.Name}\"")
            .WithWorkingDirectory(file.Directory?.FullName ?? "")
            .WithStandardErrorPipe(PipeTarget.ToDelegate(s => _logger.LogError(s)))
            .WithStandardOutputPipe(PipeTarget.ToDelegate(s => _logger.LogInformation(s)))
            .ExecuteAsync(cancellationToken);
        
        if(!result.IsSuccess)
            throw new Exception($"Sops decrypt failed with exit code {result.ExitCode} while decrypting file {file.FullName}");
    }
}