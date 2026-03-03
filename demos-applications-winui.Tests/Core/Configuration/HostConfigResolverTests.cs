using System;
using FluentAssertions;
using demos_applications_winui.Core.Configuration;
using Xunit;

namespace demos_applications_winui.Tests.Core.Configuration;

[Collection("EnvironmentVariables")]
public class HostConfigResolverTests : IDisposable
{
    public HostConfigResolverTests()
    {
        ClearEnvVars();
    }

    public void Dispose()
    {
        ClearEnvVars();
    }

    [Fact]
    public void Resolve_NoEnvVars_ReturnsLocalDev()
    {
        var config = HostConfigResolver.Resolve();

        config.Stage.Should().Be(AppStage.Local);
        config.Environment.Should().Be(AppEnvironment.Dev);
    }

    [Fact]
    public void Resolve_StageSet_ParsesCorrectly()
    {
        SetStage("Stage");

        var config = HostConfigResolver.Resolve();

        config.Stage.Should().Be(AppStage.Stage);
    }

    [Fact]
    public void Resolve_ProdStage_ParsesCorrectly()
    {
        SetStage("Prod");

        var config = HostConfigResolver.Resolve();

        config.Stage.Should().Be(AppStage.Prod);
    }

    [Fact]
    public void Resolve_EnvironmentSet_ParsesCorrectly()
    {
        SetEnvironment("QA");

        var config = HostConfigResolver.Resolve();

        config.Environment.Should().Be(AppEnvironment.QA);
    }

    [Fact]
    public void Resolve_ProdEnvironment_ParsesCorrectly()
    {
        SetEnvironment("Prod");

        var config = HostConfigResolver.Resolve();

        config.Environment.Should().Be(AppEnvironment.Prod);
    }

    [Fact]
    public void Resolve_InvalidStage_FallsBackToLocal()
    {
        SetStage("InvalidValue");

        var config = HostConfigResolver.Resolve();

        config.Stage.Should().Be(AppStage.Local);
    }

    [Fact]
    public void Resolve_InvalidEnvironment_FallsBackToDev()
    {
        SetEnvironment("InvalidValue");

        var config = HostConfigResolver.Resolve();

        config.Environment.Should().Be(AppEnvironment.Dev);
    }

    [Fact]
    public void Resolve_CaseInsensitive_ParsesCorrectly()
    {
        SetStage("stage");
        SetEnvironment("prod");

        var config = HostConfigResolver.Resolve();

        config.Stage.Should().Be(AppStage.Stage);
        config.Environment.Should().Be(AppEnvironment.Prod);
    }

    [Fact]
    public void Resolve_BothSet_ParsesBothCorrectly()
    {
        SetStage("Prod");
        SetEnvironment("Prod");

        var config = HostConfigResolver.Resolve();

        config.Stage.Should().Be(AppStage.Prod);
        config.Environment.Should().Be(AppEnvironment.Prod);
    }

    private static void SetStage(string value) =>
        Environment.SetEnvironmentVariable(HostConfigResolver.StageEnvVar, value);

    private static void SetEnvironment(string value) =>
        Environment.SetEnvironmentVariable(HostConfigResolver.EnvironmentEnvVar, value);

    private static void ClearEnvVars()
    {
        Environment.SetEnvironmentVariable(HostConfigResolver.StageEnvVar, null);
        Environment.SetEnvironmentVariable(HostConfigResolver.EnvironmentEnvVar, null);
    }
}

[CollectionDefinition("EnvironmentVariables", DisableParallelization = true)]
public class EnvironmentVariablesCollection;
