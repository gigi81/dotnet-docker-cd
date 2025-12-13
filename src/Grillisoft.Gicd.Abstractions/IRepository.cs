using System.IO.Abstractions;

namespace Grillisoft.Gicd.Abstractions;

public interface IRepository
{
    Task<bool> Poll(CancellationToken cancellationToken);

    Task Pull(IDirectoryInfo directory, CancellationToken cancellationToken);
}