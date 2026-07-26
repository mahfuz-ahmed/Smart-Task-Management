using System.Text.RegularExpressions;

namespace SmartTaskManagement.Application.Common.Extensions;

public static class StringSanitizationExtensions
{
    public static string SanitizeTitle(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var cleaned = Regex.Replace(value.Trim(), @"[\x00-\x1F\x7F]+", string.Empty);
        return Regex.Replace(cleaned, @"\s+", " ");
    }

    public static string SanitizeDescription(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var cleaned = Regex.Replace(value.Trim(), @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]+", string.Empty);
        return Regex.Replace(cleaned, @"[^\S\r\n]+", " ");
    }
}