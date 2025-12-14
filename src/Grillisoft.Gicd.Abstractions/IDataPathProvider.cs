namespace Grillisoft.Gicd.Abstractions;

public interface IDataPathProvider
{
    Task<string> GetDataPath();
}