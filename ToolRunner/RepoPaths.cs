namespace ToolRunner;

/// <summary>
/// Locates the repository root and the tool data folder.
///
/// Paths are derived, not hardcoded: the data folder was renamed from <c>tools</c>
/// to <c>Tool Triggering (Synthetic Data)</c>, and a hardcoded name would have to
/// be edited on 312 branches the next time it changes. The folder name is read
/// from dataset.json (<c>toolData.folder</c>). REPO_ROOT, when set, wins.
/// </summary>
public sealed class RepoPaths
{
    public string RepoRoot { get; }
    public string ToolsRoot { get; }

    private RepoPaths(string repoRoot, string toolsRoot)
    {
        RepoRoot = repoRoot;
        ToolsRoot = toolsRoot;
    }

    public string DatasetPath => Path.Combine(RepoRoot, "dataset.json");

    public string ToolDir(string slug) => Path.Combine(ToolsRoot, slug);

    public static RepoPaths Discover()
    {
        var root = Environment.GetEnvironmentVariable("REPO_ROOT");
        if (string.IsNullOrEmpty(root) || !File.Exists(Path.Combine(root, "dataset.json")))
        {
            root = FindUp(AppContext.BaseDirectory) ?? FindUp(Directory.GetCurrentDirectory())
                ?? throw new InvalidOperationException(
                    "cannot find the repository root: no dataset.json above "
                    + AppContext.BaseDirectory + " or the current directory; set REPO_ROOT");
        }

        root = Path.GetFullPath(root);
        var folder = Dataset.Load(Path.Combine(root, "dataset.json"))
            .Root["toolData"]?["folder"]?.GetValue<string>()
            ?? throw new InvalidOperationException("dataset.json declares no toolData.folder");
        return new RepoPaths(root, Path.Combine(root, folder));
    }

    private static string? FindUp(string start)
    {
        for (var dir = new DirectoryInfo(start); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "dataset.json")))
            {
                return dir.FullName;
            }
        }

        return null;
    }

    /// <summary>Repository-relative path with forward slashes, as dataset.json spells them.</summary>
    public string Rel(string path) =>
        Path.GetRelativePath(RepoRoot, path).Replace(Path.DirectorySeparatorChar, '/');
}
