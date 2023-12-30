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
    
    [Fact]
    public void Setting_delayed_NumberOfRetries_changes_the_default_value()
    {
        const int expected = 14;
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "NServiceBus:EndpointConfiguration:Recoverability:Delayed:NumberOfRetries", expected.ToString() }
            })
            .Build();
        
        var endpoint = new LearningEndpoint("my-endpoint", config);
        EndpointConfiguration endpointConfiguration = endpoint;

        var settings = endpointConfiguration.GetSettings();
        
        Assert.Equal(expected, settings.GetOrDefault<int>("Recoverability.Delayed.DefaultPolicy.Retries"));
    }
    
    [Fact]
    public void Setting_dealyed_NumberOfRetries_to_invalid_value_throws()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "NServiceBus:EndpointConfiguration:Recoverability:Delayed:NumberOfRetries", "cannot be parsed" }
            })
            .Build();

        Assert.Throws<ArgumentException>(() =>
        {
            var endpoint = new LearningEndpoint("my-endpoint", config);
            EndpointConfiguration endpointConfiguration = endpoint;
        });
    }
    
    [Fact]
    public void Setting_delayed_TimeIncrease_changes_the_default_value()
    {
        var expected = TimeSpan.FromSeconds(14);
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "NServiceBus:EndpointConfiguration:Recoverability:Delayed:TimeIncrease", expected.ToString() }
            })
            .Build();
        
        var endpoint = new LearningEndpoint("my-endpoint", config);
        EndpointConfiguration endpointConfiguration = endpoint;

        var settings = endpointConfiguration.GetSettings();
        
        Assert.Equal(expected, settings.GetOrDefault<TimeSpan>("Recoverability.Delayed.DefaultPolicy.Timespan"));
    }
    
    [Fact]
    public void Setting_delayed_TimeIncrease_to_invalid_value_throws()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "NServiceBus:EndpointConfiguration:Recoverability:Delayed:TimeIncrease", "cannot be parsed" }
            })
            .Build();

        Assert.Throws<ArgumentException>(() =>
        {
            var endpoint = new LearningEndpoint("my-endpoint", config);
            EndpointConfiguration endpointConfiguration = endpoint;
        });
    }
}