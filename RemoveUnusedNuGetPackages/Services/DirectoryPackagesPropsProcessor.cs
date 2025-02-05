using System.Xml;

namespace RemoveUnusedNuGetPackages.Services;

public static class DirectoryPackagesPropsProcessor
{
    private const string PackageVersionStartLine = "<PackageVersion Include=\"";

    public static async Task<HashSet<string>> GetUnusedPackages(string packagesPropsFilePath, HashSet<string> usedPackages)
    {
        HashSet<string> unusedPackages = [];
        XmlReaderSettings readerSettings = new XmlReaderSettings
        {
            Async = true
        };
        using XmlReader reader = XmlReader.Create(packagesPropsFilePath, readerSettings);
        while (await reader.ReadAsync())
        {
            if (reader is not { NodeType: XmlNodeType.Element, Name: "PackageVersion" })
                continue;

            string? packageName = reader.GetAttribute("Include");
            if (packageName is null)
                continue;

            if (!usedPackages.Contains(packageName))
                unusedPackages.Add(packageName);
        }

        return unusedPackages;
    }

    public static async Task WriteNewDirectoryPropsFile(string packagesPropsFilePath, IReadOnlySet<string> unusedPackages)
    {
        string newPropsFileText = await GetNewDirectoryPropsFileText(packagesPropsFilePath, unusedPackages);
        await using StreamWriter fileWriter = new(packagesPropsFilePath);
        await fileWriter.WriteAsync(newPropsFileText);
    }

    private static async Task<string> GetNewDirectoryPropsFileText(string packagesPropsFilePath, IReadOnlySet<string> unusedPackages)
    {
        using StreamReader fileReader = new(packagesPropsFilePath);
        string propsFileText = await fileReader.ReadToEndAsync();
        List<string> propsFileTextLines = propsFileText.Split(Environment.NewLine).ToList();
        List<string> newPropsFileTextLines = [];
        foreach (string line in propsFileTextLines)
        {
            string trimmedLine = line.TrimStart();
            if (trimmedLine.StartsWith(PackageVersionStartLine))
            {
                string packageName = trimmedLine.Replace(PackageVersionStartLine, string.Empty);
                int indexOfLastQuotes = packageName.IndexOf('"');
                packageName = packageName[..indexOfLastQuotes];
                if (unusedPackages.Contains(packageName))
                {
                    Console.WriteLine("Removing package: " + packageName);
                    continue;
                }
            }

            newPropsFileTextLines.Add(line);
        }

        return string.Join(Environment.NewLine, newPropsFileTextLines);
    }
}
