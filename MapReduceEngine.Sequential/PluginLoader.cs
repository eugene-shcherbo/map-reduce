using System.Reflection;
using System.Runtime.Loader;
using MapReduceEngine.Abstractions;

namespace MapReduceEngine.Sequential;

public class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public PluginLoadContext(string pluginPath) : base(isCollectible: false)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var loaded = Default.Assemblies.FirstOrDefault(a => a.GetName().Name == assemblyName.Name);
        if (loaded is not null) return null;

        var path = _resolver.ResolveAssemblyToPath(assemblyName);
        if (path != null)
        {
            return LoadFromAssemblyPath(path);
        }

        return null;
    }
}

public static class PluginLoader
{
    public static (IMapper mapper, IReducer reducer) LoadPlugin(string directory, string pluginName)
    {
        var resolvedDirectory = Path.IsPathRooted(directory)
            ? directory
            : Path.GetFullPath(directory, Environment.CurrentDirectory);

        var pluginPath = Path.Combine(resolvedDirectory, pluginName);
        if (!File.Exists(pluginPath))
        {
            var alt = pluginPath + ".dll";
            if (File.Exists(alt)) pluginPath = alt;
            else throw new FileNotFoundException("Plugin assembly not found", pluginPath);
        }

        var ctx = new PluginLoadContext(pluginPath);
        var assembly = ctx.LoadFromAssemblyPath(pluginPath);

        IMapper? mapper = null;
        IReducer? reducer = null;

        foreach (var type in assembly.GetTypes())
        {
            if (mapper is null && typeof(IMapper).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            {
                if (Activator.CreateInstance(type) is IMapper createdMapper)
                    mapper = createdMapper;
            }

            if (reducer is null && typeof(IReducer).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            {
                if (Activator.CreateInstance(type) is IReducer createdReducer)
                    reducer = createdReducer;
            }

            if (mapper is not null && reducer is not null)
            {
                break;
            }
        }

        if (mapper is null)
            throw new InvalidOperationException($"No IMapper implementation found in {pluginPath}");

        if (reducer is null)
            throw new InvalidOperationException($"No IReducer implementation found in {pluginPath}");

        return (mapper, reducer);
    }
}
