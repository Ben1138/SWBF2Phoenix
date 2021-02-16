using UnityEngine;

/// <summary>
/// Path will:<br/>
/// - always use forward slashes
/// - ensure there is NO trailing slash at the end
/// - ensure there are no double slashes
/// - might or might not start with a slash
/// </summary>
public class Path
{
    string P;
    
    public Path(string path)
    {
        P = path;

        // always ensure useage of forward slash
        P = P.Replace('\\', '/');
        P = P.Replace(@"\\", "/");

        // Paths never end with a slash '/'
        if (P.EndsWith("/"))
        {
            P = P.Substring(0, P.Length - 1);
        }
    }

    public static implicit operator string(Path p) => p.P;
    public static implicit operator Path(string p) => new Path(p);

    public static Path operator /(Path lhs, Path rhs) => Concat(lhs, rhs);
    public static Path operator -(Path lhs, Path rhs) => Remove(lhs, rhs);

    public bool Exists() => System.IO.File.Exists(P) || System.IO.Directory.Exists(P);

    public override string ToString()
    {
        return P;
    }

    public static Path Concat(Path lhs, Path rhs)
    {
        Path path = new Path(lhs);
        if (!rhs.P.StartsWith("/"))
        {
            path.P += '/';
        }
        path.P += rhs.P;
        return path;
    }

    public static Path Remove(Path lhs, Path rhs)
    {
        Path path = new Path(lhs);
        path.P = path.P.Replace(rhs.P, "");
        if (path.P.StartsWith("/"))
        {
            path.P = path.P.Substring(1, path.P.Length - 1);
        }
        if (path.P.EndsWith("/"))
        {
            path.P = path.P.Substring(0, path.P.Length - 1);
        }
        return path;
    }

    public bool IsFile()
    {
        System.IO.FileAttributes attr = System.IO.File.GetAttributes(P);
        return attr != System.IO.FileAttributes.Directory;
    }

    // nodeCount: how many nodes to return, starting counting from leaf node
    public Path GetLeafs(int nodeCount)
    {
        Debug.Assert(nodeCount > 0);
        string[] nodes = P.Split('/');
        nodeCount = Mathf.Min(nodeCount, nodes.Length);
        Path result = "";
        for (int i = nodes.Length - nodeCount; i < nodes.Length; ++i)
        {
            result /= nodes[i];
        }
        if (result.P.StartsWith("/"))
        {
            result.P = result.P.Substring(1, result.P.Length - 1);
        }
        return result;
    }

    public Path GetLeaf()
    {
        return GetLeafs(1);
    }
}