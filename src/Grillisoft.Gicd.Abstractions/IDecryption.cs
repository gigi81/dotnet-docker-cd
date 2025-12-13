using System.IO.Abstractions;

namespace Grillisoft.Gicd.Abstractions;

public interface IDecryption
{
    Task Decrypt(IDirectoryInfo directory, CancellationToken cancellationToken);
}