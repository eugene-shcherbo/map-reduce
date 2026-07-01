using MapReduceEngine.Sequential;

if (args.Length < 4)
{
    Console.WriteLine("Usage: MapReduceEngine.Sequential <plugin-directory> <plugin-name.dll|plugin-name> <input-directory> <file-pattern> [output-directory]");
    return 1;
}

var pluginDirectory = args[0];
var pluginName = args[1];
var inputDirectory = args[2];
var filePattern = args[3];
var outputDirectory = args.Length >= 5 ? args[4] : Environment.CurrentDirectory;

try
{
    var resolvedPluginDirectory = Path.IsPathRooted(pluginDirectory)
        ? pluginDirectory
        : Path.GetFullPath(pluginDirectory, Environment.CurrentDirectory);

    var resolvedInputDirectory = Path.IsPathRooted(inputDirectory)
        ? inputDirectory
        : Path.GetFullPath(inputDirectory, Environment.CurrentDirectory);

    var resolvedOutputDirectory = Path.IsPathRooted(outputDirectory)
        ? outputDirectory
        : Path.GetFullPath(outputDirectory, Environment.CurrentDirectory);

    Directory.CreateDirectory(resolvedOutputDirectory);

    if (!Directory.Exists(resolvedInputDirectory))
    {
        Console.WriteLine($"Input directory not found: {resolvedInputDirectory}");
        return 3;
    }

    var inputFiles = Directory.GetFiles(resolvedInputDirectory, filePattern, SearchOption.TopDirectoryOnly);
    if (inputFiles.Length == 0)
    {
        Console.WriteLine($"No input files matched pattern '{filePattern}' in '{resolvedInputDirectory}'.");
        return 4;
    }

    var (mapper, reducer) = PluginLoader.LoadPlugin(resolvedPluginDirectory, pluginName);
    var mappedResults = new Dictionary<string, List<string>>();

    foreach (var inputFile in inputFiles)
    {
        var fileName = Path.GetFileName(inputFile);
        var fileContent = File.ReadAllText(inputFile);

        foreach (var (key, value) in mapper.Map(fileName, fileContent))
        {
            if (!mappedResults.ContainsKey(key))
                mappedResults[key] = [];
            mappedResults[key].Add(value);
        }
    }

    var outputLines = mappedResults
        .Select(kv => reducer.Reduce(kv.Key, kv.Value))
        .SelectMany(a => a)
        .ToList();

    var outputFilePath = Path.Combine(resolvedOutputDirectory, "mr-out-0");
    File.WriteAllLines(outputFilePath, outputLines);

    Console.WriteLine($"Results written to {outputFilePath}");

    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    return 2;
}
