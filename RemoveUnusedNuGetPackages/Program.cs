using RemoveUnusedNuGetPackages.Services;

try
{
    Console.Write("Enter the full file path of the Directory.Packages.props file: ");
    string? packagesPropsFilePath = Console.ReadLine();
    if (packagesPropsFilePath is null)
        throw new InvalidOperationException("No file path was provided!");

    Console.Write("Enter the full file path of the .sln file: ");
    string? packagesPropsSlnFilePath = Console.ReadLine();
    if (packagesPropsSlnFilePath is null)
        throw new InvalidOperationException("No file path was provided!");

    Console.WriteLine("Finding used packages...");
    string packagesJsonString = await DotnetListProcessor.GetUsedPackageVersionsAsJsonString(packagesPropsSlnFilePath);
    HashSet<string> usedPackages = JsonPackagesProcessor.GetUsedPackages(packagesJsonString);
    Console.WriteLine("Found {0} used packages.", usedPackages.Count);
    Console.WriteLine("Finding unused packages...");
    HashSet<string> unusedPackages = await DirectoryPackagesPropsProcessor.GetUnusedPackages(packagesPropsFilePath, usedPackages);
    Console.WriteLine("Found {0} unused packages.", unusedPackages.Count);
    if (unusedPackages.Count < 1)
        return;

    Console.WriteLine("Modifying the file at {0}...", packagesPropsFilePath);
    await DirectoryPackagesPropsProcessor.WriteNewDirectoryPropsFile(packagesPropsFilePath, unusedPackages);
    Console.WriteLine("Successfully overwritten {0}.", packagesPropsFilePath);
}
catch (Exception exception)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("An exception was thrown! {0}", exception);
}
