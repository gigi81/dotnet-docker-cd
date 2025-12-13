using System.IO.Abstractions;

namespace Grillisoft.Gicd.Abstractions;

public interface IStack
{
    Task Deploy(IDirectoryInfo directory, CancellationToken cancellationToken);
}