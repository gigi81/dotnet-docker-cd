using System.IO.Abstractions;

namespace Grillisoft.Gicd.Abstractions;

public interface IDataPathProvider
{
    Task<IDirectoryInfo> GetDataPath();
}