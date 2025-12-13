using System.IO.Abstractions;
using Grillisoft.Gicd.Abstractions;
using Octokit;
using CliWrap;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Credentials = Octokit.Credentials;

namespace Grillisoft.Gicd;

public class Repository : IRepository
{
    private readonly IOptions<GithubOptions> _options;
    private readonly ILogger<Repository> _logger;

    private string? _lastSeenCommitSha;

    public Repository(IOptions<GithubOptions> options, ILogger<Repository> logger)
    {
        _options = options;
        _logger = logger;
    }

    private GitHubClient Client
    {
        get
        {
            if(field != null)
                return field;
        
            field = new GitHubClient(new ProductHeaderValue(nameof(Repository)));

            var basicAuth = new Credentials(_options.Value.Token);
            field.Credentials = basicAuth;
            return field;
        }
    }

    public async Task<bool> Poll(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        var owner = _options.Value.Owner;
        var repository = _options.Value.RepositoryName;
        
        // Get repository metadata to determine default branch
        var repo = await this.Client.Repository.Get(owner, repository);
        if (repo == null)
            throw new InvalidOperationException("Repository not found.");

        var defaultBranch = repo.DefaultBranch;
        if (string.IsNullOrWhiteSpace(defaultBranch))
            throw new InvalidOperationException("Repository default branch is not set.");

        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        // Get branch information (contains latest commit)
        var branch = await this.Client.Repository.Branch.Get(owner, repository, defaultBranch);
        var latestSha = branch?.Commit?.Sha ?? throw new InvalidOperationException("Unable to read latest commit SHA.");

        // If first poll, treat as change (return true) and store SHA
        if (string.IsNullOrEmpty(_lastSeenCommitSha))
        {
            _lastSeenCommitSha = latestSha;
            return true;
        }

        if (_lastSeenCommitSha == latestSha)
            return false;

        _lastSeenCommitSha = latestSha;
        return true;
    }

    public async Task Pull(IDirectoryInfo directory, CancellationToken cancellationToken)
    {
        // Ensure directory exists
        if (!directory.Exists)
            directory.Create();
        
        var owner = _options.Value.Owner;
        var repository = _options.Value.RepositoryName;

        // Get repo info to determine default branch
        var repo = await this.Client.Repository.Get(owner, repository);
        if (repo == null)
            throw new InvalidOperationException("Repository not found.");

        var defaultBranch = repo.DefaultBranch;
        if (string.IsNullOrWhiteSpace(defaultBranch))
            throw new InvalidOperationException("Repository default branch is not set.");

        // Determine if directory is empty
        var isEmpty = directory.EnumerateFileSystemInfos().FirstOrDefault() == null;

        if (isEmpty)
        {
            // Clone into the directory
            // Use --depth=1 to be efficient and --branch to checkout default branch
            await Git($"clone --depth=1 --branch {defaultBranch} {_options.Value.RemoteUrl} .", directory, cancellationToken);
            return;
        }
        
        if (!directory.SubDirectory(".git").Exists)
            throw new InvalidOperationException("Target directory is not empty and does not contain a git repository.");

        // If we get here, it's a git repository: update origin and reset to remote default branch
        // Ensure origin points to the correct repository
        await Git($"remote set-url origin {_options.Value.RemoteUrl}", directory, cancellationToken);

        // Fetch all updates and prune stale refs
        await Git("fetch --all --prune", directory, cancellationToken);

        // Reset local state to remote default branch (hard reset + clean)
        await Git($"reset --hard origin/{defaultBranch}", directory, cancellationToken);
        await Git("clean -fd", directory, cancellationToken);
    }
    
    async Task Git(string args, IDirectoryInfo directory, CancellationToken cancellationToken)
    {
        await Cli.Wrap("git")
            .WithArguments(args)
            .WithWorkingDirectory(directory.FullName)
            .WithStandardErrorPipe(PipeTarget.ToDelegate(s => _logger.LogError(s)))
            .WithStandardOutputPipe(PipeTarget.ToDelegate(s => _logger.LogInformation(s)))
            .ExecuteAsync(cancellationToken);
    }
}
