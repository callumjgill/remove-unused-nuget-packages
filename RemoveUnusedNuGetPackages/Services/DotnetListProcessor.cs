using System.Diagnostics;

namespace RemoveUnusedNuGetPackages.Services;

public static class DotnetListProcessor
{
    public static async Task<string> GetUsedPackageVersionsAsJsonString(string packagesPropsSlnFilePath)
    {
        Process dotnetListProcess = new ();
        dotnetListProcess.StartInfo.UseShellExecute = false;
        dotnetListProcess.StartInfo.CreateNoWindow = true;
        dotnetListProcess.StartInfo.RedirectStandardOutput = true;
        dotnetListProcess.StartInfo.RedirectStandardError = true;
        dotnetListProcess.StartInfo.FileName = "dotnetlist.bat";
        dotnetListProcess.StartInfo.ArgumentList.Add(packagesPropsSlnFilePath);
        dotnetListProcess.Start();
        string dotnetListProcessRawOutput = await dotnetListProcess.StandardOutput.ReadToEndAsync();
        string error = await dotnetListProcess.StandardError.ReadToEndAsync();
        dotnetListProcess.Close();
        if (error != string.Empty)
            throw new InvalidOperationException("An error occurred running the dotnet list command: " + error);

        int firstLineEndLine = dotnetListProcessRawOutput.IndexOf('{');
        string packagesRawOutput = dotnetListProcessRawOutput[firstLineEndLine..];
        return packagesRawOutput;
    }
}
