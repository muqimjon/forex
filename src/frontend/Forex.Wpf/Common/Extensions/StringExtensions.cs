namespace Forex.Wpf.Common.Extensions;

public static class StringExtensions
{
    public static string Trimmer(this string value, int length)
    {
        if (string.IsNullOrEmpty(value) || length <= 3)
            return value;

        if (value.Length <= length)
            return value;

        return string.Concat(value.AsSpan(0, length - 3), "...");
    }

    public static string WrapWithNewLines(this string text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text) || maxLength <= 0)
            return text;

        var words = text.Split(' ');
        var lines = new List<string>();
        var currentLine = "";

        foreach (var word in words)
        {
            // Agar so‘z o‘zi maxLength’dan uzun bo‘lsa, uni bo‘lib qo‘shamiz
            if (word.Length > maxLength)
            {
                if (!string.IsNullOrEmpty(currentLine))
                {
                    lines.Add(currentLine.TrimEnd());
                    currentLine = "";
                }

                for (int i = 0; i < word.Length; i += maxLength)
                {
                    var chunk = word.Substring(i, Math.Min(maxLength, word.Length - i));
                    lines.Add(chunk);
                }

                continue;
            }

            // Agar currentLine + word sig‘sa, qo‘shamiz
            if (currentLine.Length + word.Length + 1 <= maxLength)
            {
                currentLine += (currentLine.Length > 0 ? " " : "") + word;
            }
            else
            {
                // Sig‘masa, currentLine’ni qo‘shamiz va yangi qator boshlaymiz
                lines.Add(currentLine.TrimEnd());
                currentLine = word;
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
            lines.Add(currentLine.TrimEnd());

        return string.Join("\n", lines);
    }
}
