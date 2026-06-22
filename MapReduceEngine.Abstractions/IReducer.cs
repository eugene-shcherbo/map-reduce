namespace MapReduceEngine.Abstractions;

public interface IReducer
{
    IEnumerable<string> Reduce(string key, IEnumerable<string> value);
}