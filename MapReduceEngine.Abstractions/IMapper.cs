namespace MapReduceEngine.Abstractions;

public interface IMapper
{
    IEnumerable<(string key, string value)> Map(string key, string value);
}