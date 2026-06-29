using MapReduceEngine.Console;

if (args.Length < 2)
{
    Console.WriteLine("Usage: MapReduceEngine.Console <plugin-directory> <plugin-name.dll|plugin-name>");
    return 1;
}

var dir = args[0];
var pluginName = args[1];

try
{
    var (mapper, reducer) = PluginLoader.LoadPlugin(dir, pluginName);

    mapper.Map("key", "value").GetEnumerator().MoveNext();
    reducer.Reduce("key", ["value"]).GetEnumerator().MoveNext();

    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"Error loading plugin: {ex.Message}");
    return 2;
}
