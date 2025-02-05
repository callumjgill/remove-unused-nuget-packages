using System.Text.Json;

namespace RemoveUnusedNuGetPackages.Services;

public static class JsonPackagesProcessor
{
    public static HashSet<string> GetUsedPackages(string packagesJsonString)
    {
        using JsonDocument? packagesJson = JsonSerializer.Deserialize<JsonDocument>(packagesJsonString);
        if (packagesJson is null)
            throw new InvalidOperationException("Deserialized packages JSON output was null!");

        HashSet<string> usedPackages = [];
        IEnumerable<JsonElement> frameworks = packagesJson.RootElement
            .GetProperty("projects")
            .EnumerateArray()
            .SelectMany(project => project.GetProperty("frameworks").EnumerateArray());
        foreach (JsonElement project in frameworks)
        {
            HashSet<string> topLevelPackages = project
                .GetProperty("topLevelPackages")
                .EnumerateArray()
                .Select(topLevelPackage => topLevelPackage.GetProperty("id").GetString() ?? string.Empty)
                .Distinct()
                .ToHashSet();
            HashSet<string> transitivePackages = project
                .GetProperty("transitivePackages")
                .EnumerateArray()
                .Select(topLevelPackage => topLevelPackage.GetProperty("id").GetString() ?? string.Empty)
                .Distinct()
                .ToHashSet();
            usedPackages.UnionWith(topLevelPackages);
            usedPackages.UnionWith(transitivePackages);
        }

        return usedPackages;
    }
}
