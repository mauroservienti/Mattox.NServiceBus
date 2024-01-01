using Microsoft.Extensions.Configuration;
using NServiceBus.Configuration.AdvancedExtensibility;
using NServiceBus.Transport;

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
    public void Setting_rate_limiting_TimeToWaitBetweenThrottledAttempts_to_invalid_value_throws()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "NServiceBus:EndpointConfiguration:Recoverability:AutomaticRateLimiting:ConsecutiveFailures", "10" },
                { "NServiceBus:EndpointConfiguration:Recoverability:AutomaticRateLimiting:TimeToWaitBetweenThrottledAttempts", "cannot be parsed" }
            })
            .Build();

        Assert.Throws<ArgumentException>(() =>
        {
            var endpoint = new LearningEndpoint("my-endpoint", config);
            EndpointConfiguration endpointConfiguration = endpoint;
        });
    }
    
    [Fact]
    public void Setting_rate_limiting_changes_the_default_values()
    {
        const int expectedConsecutiveFailures = 14;
        var expectedTimeToWaitBetweenThrottledAttempts = TimeSpan.FromMinutes(10);
        Func<CancellationToken, Task> expectedOnRateLimitStarted = _ => Task.CompletedTask;
        Func<CancellationToken, Task> expectedOnRateLimitEnded = _ => Task.CompletedTask;
        
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "NServiceBus:EndpointConfiguration:Recoverability:AutomaticRateLimiting:ConsecutiveFailures", expectedConsecutiveFailures.ToString() },
                { "NServiceBus:EndpointConfiguration:Recoverability:AutomaticRateLimiting:TimeToWaitBetweenThrottledAttempts", expectedTimeToWaitBetweenThrottledAttempts.ToString() }
            })
            .Build();
        
        var endpoint = new LearningEndpoint("my-endpoint", config);
        endpoint.Recoverability.OnRateLimitStarted(expectedOnRateLimitStarted);
        endpoint.Recoverability.OnRateLimitEnded(expectedOnRateLimitEnded);
        EndpointConfiguration endpointConfiguration = endpoint;

        var settings = endpointConfiguration.GetSettings();
        var consecutiveFailureConfiguration =
            settings.GetOrDefault<object>("NServiceBus.ConsecutiveFailuresConfiguration");

        var numberOfConsecutiveFailuresBeforeArming = (int)consecutiveFailureConfiguration!
            .GetType()
            .GetProperty("NumberOfConsecutiveFailuresBeforeArming")!
            .GetValue(consecutiveFailureConfiguration)!;

        var rateLimitSettings = (RateLimitSettings)consecutiveFailureConfiguration!
            .GetType()
            .GetProperty("RateLimitSettings")!
            .GetValue(consecutiveFailureConfiguration)!;

        Assert.Equal(expectedConsecutiveFailures, numberOfConsecutiveFailuresBeforeArming);
        Assert.Equal(expectedTimeToWaitBetweenThrottledAttempts, rateLimitSettings.TimeToWaitBetweenThrottledAttempts);
        Assert.Equal(expectedOnRateLimitStarted, rateLimitSettings.OnRateLimitStarted);
        Assert.Equal(expectedOnRateLimitEnded, rateLimitSettings.OnRateLimitEnded);
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
    
    [Fact]
    public void Setting_failed_policy_changes_the_default_value()
    {
        Action<Dictionary<string, string>> expected = _ => { };
        
        var endpoint = new LearningEndpoint("my-endpoint");
        endpoint.Recoverability.OnFailedMessage(failedSettings => failedSettings.HeaderCustomization(expected));
        EndpointConfiguration endpointConfiguration = endpoint;

        var settings = endpointConfiguration.GetSettings();
        
        Assert.Equal(expected, settings.GetOrDefault<Action<Dictionary<string, string>>>("Recoverability.Failed.FaultHeaderCustomization"));
    }
    
    [Fact]
    public void Setting_recoverability_policy_changes_the_default_value()
    {
        Func<RecoverabilityConfig, ErrorContext, RecoverabilityAction> expected = (config, context) => RecoverabilityAction.MoveToError("q");
        
        var endpoint = new LearningEndpoint("my-endpoint");
        endpoint.Recoverability.UseCustomRecoverabilityPolicy(expected);
        EndpointConfiguration endpointConfiguration = endpoint;

        var settings = endpointConfiguration.GetSettings();
        
        Assert.Equal(expected, settings.GetOrDefault<Func<RecoverabilityConfig, ErrorContext, RecoverabilityAction>>("Recoverability.CustomPolicy"));
    }
}