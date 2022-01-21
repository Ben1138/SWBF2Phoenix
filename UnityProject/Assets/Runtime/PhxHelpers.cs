using System;

public static class PhxHelpers
{
	// format string using SWBFs C-style printf format (%s, ...)
	public static string Format(string fmt, params object[] args)
	{
		fmt = ConvertFormat(fmt);
		return string.Format(fmt, args);
	}

	// convert C-style printf format to C# format
	static string ConvertFormat(string swbfFormat)
	{
		int GetNextIndex(string format)
		{
			int idx = format.IndexOf("%s");
			if (idx >= 0) return idx;

			idx = format.IndexOf("%i");
			if (idx >= 0) return idx;

			idx = format.IndexOf("%d");
			if (idx >= 0) return idx;

			idx = format.IndexOf("%f");
			if (idx >= 0) return idx;

			return -1;
		}

		// convert C-style printf format to C# format
		string format = swbfFormat;
		int idx = GetNextIndex(format);
		for (int i = 0; idx >= 0; idx = GetNextIndex(format), ++i)
		{
			string sub = format.Substring(0, idx);
			sub += "{" + i + "}";
			sub += format.Substring(idx + 2, format.Length - idx - 2);
			format = sub;
		}
		return format;
	}
}