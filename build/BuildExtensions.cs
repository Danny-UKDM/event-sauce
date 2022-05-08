using System.Text.RegularExpressions;

public static class BuildExtensions
{
    public static string ToKebabCase(this string input) =>
        Regex.Replace(input, @"([a-z0–9])([A-Z])", "$1-$2").ToLower();
}
