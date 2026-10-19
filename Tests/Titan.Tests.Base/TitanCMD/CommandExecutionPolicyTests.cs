using NUnit.Framework;
using ProjectCooker;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace EngineNS.Tests.TitanCMD;

[TestFixture]
public class CommandExecutionPolicyTests
{
    [TestCase("SaveAsLastest", false)]
    [TestCase("MakeSln", false)]
    [TestCase("StartDS", true)]
    [TestCase("LoadPlugin", true)]
    [TestCase("StartRobot", true)]
    [TestCase("StartRootServer", true)]
    [TestCase("StartLoginServer", true)]
    [TestCase("StartLevelServer", true)]
    [TestCase("StartGateServer", true)]
    public void IsServiceCommand_KnownCommand_ReturnsExpectedLifetime(string command, bool expected)
    {
        Assert.That(TtCommandExecutionPolicy.IsServiceCommand(command), Is.EqualTo(expected));
    }

    [Test]
    public void IsServiceCommand_UnknownCommand_IsOneShot()
    {
        Assert.That(TtCommandExecutionPolicy.IsServiceCommand("Unknown"), Is.False);
    }

    [TestCase("debug", "release", "debug")]
    [TestCase(null, "debug", "debug")]
    [TestCase(null, null, "release")]
    public void ResolveNativeConfiguration_CommandLineTakesPrecedence(string requested, string cached, string expected)
    {
        Assert.That(TtCommandExecutionPolicy.ResolveNativeConfiguration(requested, cached), Is.EqualTo(expected));
    }

    [Test]
    public void IsEngineConfigurationAvailable_MissingFile_ReturnsFalse()
    {
        var missingConfig = GetRepositoryPath("content", "config", "missing-test-config.jscfg");

        Assert.That(TtCommandExecutionPolicy.IsEngineConfigurationAvailable(missingConfig), Is.False);
    }

    [Test]
    public void Main_ReturnsProcessExitCode()
    {
        var programType = typeof(TtCommandExecutionPolicy).Assembly.GetType("ProjectCooker.Program");
        var main = programType?.GetMethod("Main", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

        Assert.That(main, Is.Not.Null);
        Assert.That(main!.ReturnType, Is.EqualTo(typeof(int)));
    }

    [Test]
    public void ExecuteCommandAsync_ReturnsAwaitableExitCode()
    {
        var programType = typeof(TtCommandExecutionPolicy).Assembly.GetType("ProjectCooker.Program");
        var execute = programType?.GetMethod("ExecuteCommandAsync", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

        Assert.That(execute, Is.Not.Null);
        Assert.That(execute!.ReturnType, Is.EqualTo(typeof(System.Threading.Tasks.Task<int>)));
    }

    [Test]
    public void CookCommand_ReturnsExitCode()
    {
        var commandType = typeof(TtCommandExecutionPolicy).Assembly.GetType("ProjectCooker.TtCookCommand");
        var execute = commandType?.GetMethod("ExecuteCommand");

        Assert.That(execute, Is.Not.Null);
        Assert.That(execute!.ReturnType, Is.EqualTo(typeof(EngineNS.Thread.Async.TtTask<int>)));
    }

    [Test]
    public void RunTestsCommand_ExistsAndReturnsExitCode()
    {
        var commandType = typeof(TtCommandExecutionPolicy).Assembly.GetType("ProjectCooker.Command.TtRunTests");
        var execute = commandType?.GetMethod("ExecuteCommand");

        Assert.Multiple(() =>
        {
            Assert.That(commandType, Is.Not.Null);
            Assert.That(execute?.ReturnType, Is.EqualTo(typeof(EngineNS.Thread.Async.TtTask<int>)));
        });
    }

    [Test]
    public void DataCopyerDescriptor_DoesNotRequireGamePlugins()
    {
        var descriptorPath = GetRepositoryPath("Plugins", "DataCopyer", "DataCopyer.plugin");
        using var descriptor = JsonDocument.Parse(File.ReadAllText(descriptorPath));
        var dependencies = descriptor.RootElement.GetProperty("Dependencies");

        Assert.That(dependencies.GetArrayLength(), Is.Zero,
            "DataCopyer must start without game-specific plugin modules.");
    }

    [Test]
    public void L2Manifest_BuildsRequiredDataCopyerPlugin()
    {
        var manifestPath = GetRepositoryPath("content", "config", "quality-manifest.json");
        using var manifest = JsonDocument.Parse(File.ReadAllText(manifestPath));
        var l2Suite = manifest.RootElement.GetProperty("suites")
            .EnumerateArray()
            .Single(suite => suite.GetProperty("name").GetString() == "L2.Engine");
        var l2Projects = l2Suite.GetProperty("projects");

        Assert.That(l2Projects.ToString(), Does.Contain("Plugins/DataCopyer/DataCopyer.All/DataCopyer.All.csproj"));
    }

    [Test]
    public void L1Manifest_RunsLeanClrGeneratorTestsInCi()
    {
        var manifestPath = GetRepositoryPath("content", "config", "quality-manifest.json");
        using var manifest = JsonDocument.Parse(File.ReadAllText(manifestPath));
        var generatorSuite = manifest.RootElement.GetProperty("suites")
            .EnumerateArray()
            .SingleOrDefault(suite => suite.GetProperty("name").GetString() == "L1.LeanClrGenerator");

        Assert.Multiple(() =>
        {
            Assert.That(generatorSuite.ValueKind, Is.EqualTo(JsonValueKind.Object));
            Assert.That(generatorSuite.GetProperty("level").GetString(), Is.EqualTo("L1"));
            Assert.That(generatorSuite.GetProperty("projects").ToString(),
                Does.Contain("Tests/Titan.Tests.LeanClrGenerator/Titan.Tests.LeanClrGenerator.csproj"));
            Assert.That(generatorSuite.GetProperty("frequency").ToString(), Does.Contain("pr"));
        });
    }

    private static string GetRepositoryPath(params string[] pathParts)
    {
        var root = Path.GetFullPath(Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "..", "..", "..", "..", ".."));
        return Path.Combine([root, .. pathParts]);
    }
}
