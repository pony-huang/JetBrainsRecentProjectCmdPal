using JetBrainsRecentProjectCmdPal.Helper;
using Xunit.Abstractions;

namespace JetBrainsRecentProjectCmdPal.Tests.Helper;

public class ShellHelperTest
{
    private readonly ITestOutputHelper _output;

    public ShellHelperTest(ITestOutputHelper output)
    {
        _output = output;
    }

    #region RecentProjectsParser Tests

    [Fact]
    public void test1()
    {
        string msg = string.Empty;
        // ShellHelper.OpenInShell("E:\\Jetbrains\\shell\\idea.cmd", ref msg, "E:\\Jetbrains\\shell", null);
        var combine = Path.Combine("E:/Jetbrains/shell", "idea").Replace("\\", "/");
        _output.WriteLine(combine);
        ShellHelper.OpenInShell(combine, ref msg, "E:\\Jetbrains\\shell", null,"runAs");
    }
    
    [Fact]
    public void test2()
    {
        
        string msg = string.Empty;
        var bestShellPath = Query.FindBestShellFile("E:/Jetbrains/shell", "idea");
        ShellHelper.OpenInShell(bestShellPath, ref msg, "E:\\Jetbrains\\shell", null,"runAs");
        _output.WriteLine(bestShellPath.Replace("\\", "/"));
        
    }

    #endregion
}