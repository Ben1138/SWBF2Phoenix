public class Path
{
    string P;

    public Path(string path)
    {
        P = path;

        // always ensure forward slash
        P.Replace('\\', '/');
    }

    public static implicit operator string(Path p) => p.P;
    public static implicit operator Path(string p) => new Path(p);

    public static Path operator /(Path lhs, Path rhs) => Concat(lhs, rhs);
    public static Path operator /(string lhs, Path rhs) => Concat(lhs, rhs);
    public static Path operator /(Path lhs, string rhs) => Concat(lhs, rhs);

    public bool Exists() => System.IO.File.Exists(P) || System.IO.Directory.Exists(P);

    public override string ToString()
    {
        return P;
    }

    public static Path Concat(Path lhs, Path rhs)
    {
        Path path = new Path(lhs);
        if (!path.P.EndsWith("/"))
        {
            path.P += '/';
        }
        path.P += rhs.P;
        return path;
    }

    public static Path Concat(Path lhs, string rhs)
    {
        Path path = new Path(lhs);
        if (!path.P.EndsWith("/"))
        {
            path.P += '/';
        }
        path.P += rhs;
        return path;
    }

    public bool IsFile()
    {
        System.IO.FileAttributes attr = System.IO.File.GetAttributes(P);
        return attr != System.IO.FileAttributes.Directory;
    }
}