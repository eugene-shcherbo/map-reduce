# MapReduce Engine — Sequential

Overview

This is the sequential runner for the MapReduce engine. It loads a plugin (DLL) that implements `IMapper` and `IReducer`, runs a map phase over input files, then reduces and writes results.

Usage

```
# args: <plugin-directory> <plugin-name.dll|plugin-name> <input-directory> <file-pattern> [output-directory]
# example (from project root):
dotnet run --project map-reduce-engine/MapReduceEngine.Sequential plugins WordCountPlugin map-reduce-apps/wc "*.cs" ./results
```

- `plugin-directory` — directory containing the plugin assembly (or directory where plugin DLLs live).
- `plugin-name` — assembly file name (e.g. `WordCountPlugin.dll`) or base name (`WordCountPlugin`).
- `input-directory` — directory to scan for files to process.
- `file-pattern` — glob/pattern (e.g. `*.txt`, `*.cs`) used with `Directory.GetFiles`.
- `output-directory` (optional) — where `output.txt` will be written. Defaults to the current working directory if omitted.

Output

- Results are printed to stdout as lines like:

  Reduced result for key 'the-key': the-value

- The same lines are saved to `output.txt` inside the resolved output directory. The program creates the directory if it doesn't exist.

Plugin contract

Plugins must reference `MapReduceEngine.Abstractions` and provide concrete implementations of `IMapper` and `IReducer`.

- `IMapper.Map(string fileName, string content)` should return an enumerable of `(string key, string value)` pairs.
- `IReducer.Reduce(string key, IEnumerable<string> values)` should return an enumerable of reduced string values for the key.
