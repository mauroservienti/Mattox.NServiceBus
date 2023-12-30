using Microsoft.Extensions.Configuration;
using NServiceBus.Configuration.AdvancedExtensibility;

namespace Mattox.NServiceBus.Tests;

public class RecoverabilitySettingsTests
{
    [Fact]
    public void Setting_error_queue_behaves_as_expected()
    {
        const string expected = "custom-error";
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "NServiceBus:EndpointConfiguration:Recoverability:ErrorQueue", expected }
            })
            .Build();
        
        var endpoint = new LearningEndpoint("my-endpoint", config);
        EndpointConfiguration endpointConfiguration = endpoint;

        var settings = endpointConfiguration.GetSettings();
        
        Assert.Equal(expected, settings.GetOrDefault<string>("errorQueue"));
    }
    
    [Fact]
    public void Setting_rate_limiting_ConsecutiveFailures_to_invalid_value_throws()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "NServiceBus:EndpointConfiguration:Recoverability:AutomaticRateLimiting:ConsecutiveFailures", "cannot be parsed" }
            })
            .Build();

        Assert.Throws<ArgumentException>(() =>
        {
            var endpoint = new LearningEndpoint("my-endpoint", config);
            EndpointConfiguration endpointConfiguration = endpoint;
        });
    }
    
    [Fact]
    public void Setting_immediate_NumberOfRetries_to_invalid_value_throws()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "NServiceBus:EndpointConfiguration:Recoverability:Immediate:NumberOfRetries", "cannot be parsed" }
            })
            .Build();

        Assert.Throws<ArgumentException>(() =>
        {
            var endpoint = new LearningEndpoint("my-endpoint", config);
            EndpointConfiguration endpointConfiguration = endpoint;
        });
    }
    
    [Fact]
    public void Setting_immediate_NumberOfRetries_changes_the_default_value()
    {
        const int expected = 14;
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "NServiceBus:EndpointConfiguration:Recoverability:Immediate:NumberOfRetries", expected.ToString() }
            })
            .Build();
        
        var endpoint = new LearningEndpoint("my-endpoint", config);
        EndpointConfiguration endpointConfiguration = endpoint;

        var settings = endpointConfiguration.GetSettings();
        
        Assert.Equal(expected, settings.GetOrDefault<int>("Recoverability.Immediate.Retries"));
    }
}