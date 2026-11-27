using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkyCD.Plugin.Sample.Config.Configuration;
using SkyCD.SDK.Capabilities.Configuration;
using SkyCD.Testing;
using Xunit;

namespace SkyCD.Plugin.Sample.Config.Tests;

public sealed class SampleConfigurationPluginTests
{
    [Fact]
    public void AssemblyDeclaresConfigurationCapability()
    {
        var metadata = PluginAssemblyMetadata.Inspect(typeof(SampleConfigurationPlugin).Assembly);

        Assert.Equal("SkyCD.Plugin.Sample.Configuration", metadata.Id);
        Assert.Contains(typeof(SampleConfigurationPlugin), metadata.CapabilityTypes);
    }

    [Fact]
    public void ProvidesGeneralConfigurationDescriptor()
    {
        var descriptor = Assert.Single(new SampleConfigurationPlugin().Configurations);

        Assert.Equal("sample.configuration.general", descriptor.ConfigurationId);
        Assert.Equal("Sample Configuration", descriptor.Title);
    }

    [Theory]
    [InlineData(null, null, SampleConfigurationEditor.DefaultGreeting, SampleConfigurationEditor.DefaultSignature)]
    [InlineData("Custom greeting", "Custom signature", "Custom greeting", "Custom signature")]
    public async Task AcceptReturnsSettingsWithoutCreatingTheView(
        string? greeting, string? signature, string expectedGreeting, string expectedSignature)
    {
        var plugin = new SampleConfigurationPlugin();
        using var page = await plugin.CreatePageAsync(new PluginConfigurationRequest
        {
            ConfigurationId = "sample.configuration.general",
            CurrentSettings = new Dictionary<string, string?>
            {
                [SampleConfigurationEditor.GreetingKey] = greeting,
                [SampleConfigurationEditor.SignatureKey] = signature
            }
        });

        var result = await page.AcceptAsync();

        Assert.True(result.Success, result.Error);
        Assert.Equal(expectedGreeting, result.Settings![SampleConfigurationEditor.GreetingKey]);
        Assert.Equal(expectedSignature, result.Settings[SampleConfigurationEditor.SignatureKey]);
    }

    [Fact]
    public async Task DisposedPageRejectsAcceptanceWithoutCreatingTheView()
    {
        var plugin = new SampleConfigurationPlugin();
        using var page = await plugin.CreatePageAsync(new PluginConfigurationRequest
        {
            ConfigurationId = "sample.configuration.general"
        });
        page.Dispose();

        Assert.Throws<ObjectDisposedException>(() => { _ = page.AcceptAsync(); });
    }
}
