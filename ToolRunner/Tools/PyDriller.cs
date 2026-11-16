namespace ToolRunner.Tools;

/// <summary>
/// PyDriller. Python, reads git only. Class C: never touches the source language.
///
/// PyDriller is a Python LIBRARY, not a command, so the only way to drive it is
/// through a Python interpreter. The driver below is handed to <c>python -c</c>;
/// everything around it (the probe, the paths, the exit-code contract) is C#.
/// </summary>
public sealed class PyDriller : ITool
{
    private const string Driver = """
        import json, os
        from pydriller import Repository
        root = os.environ["REPO_ROOT"]
        commits, authors = 0, {}
        for c in Repository(root).traverse_commits():
            commits += 1
            authors[c.author.name] = authors.get(c.author.name, 0) + 1
            for line in (c.msg or "").splitlines():
                if line.lower().startswith("co-authored-by:"):
                    name = line.split(":", 1)[1].split("<")[0].strip()
                    authors[name] = authors.get(name, 0) + 1
        json.dump({"tool": "pydriller", "commits": commits, "contributors": len(authors),
                   "by_author": authors},
                  open(os.path.join(root, "reports", "pydriller.json"), "w"), indent=2)
        """;

    public string Slug => "pydriller";

    public int Run(ToolContext ctx)
    {
        var python = ctx.RequirePythonModule("pydriller");
        ctx.ReportDir();
        return ctx.Run(python, ["-c", Driver]) == 0 ? ExitCodes.Ok : ExitCodes.Fail;
    }
}
