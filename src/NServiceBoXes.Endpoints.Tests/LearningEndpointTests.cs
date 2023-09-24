using Microsoft.Extensions.Configuration;
using NServiceBus.Configuration.AdvancedExtensibility;

namespace NServiceBoXes.Endpoints.Tests;

public class LearningEndpointTests
{
    [Fact]
    public void Basic_endpoint_respect_name_and_default_values()
    {
        var expectedEndpointName = "my-endpoint";
        var endpoint = new LearningEndpoint(expectedEndpointName);
        EndpointConfiguration endpointConfiguration = endpoint;
        
        var actualEndpointName = endpointConfiguration.GetSettings().EndpointName();
        
        Assert.Equal(expectedEndpointName, actualEndpointName);
    }
    
    [Fact]
    public void Using_json_configuration_respect_settings()
    {
        var expectedEndpointName = "my-endpoint";
     
        var config = new ConfigurationBuilder()
            .AddJsonFile("endpoint.settings.json")
            .Build();
        
        var endpoint = new LearningEndpoint(config);
        EndpointConfiguration endpointConfiguration = endpoint;

        var settings = endpointConfiguration.GetSettings();
        var actualEndpointName = settings.EndpointName();
        
        var auditSettingsResult = settings.GetOrDefault<object>("NServiceBus.AuditConfigReader+Result");

        var auditSettingsResultType = auditSettingsResult.GetType();
        var auditQueueAddress = auditSettingsResultType.GetField("Address")?.GetValue(auditSettingsResult) as string;
        
        Assert.Equal(expectedEndpointName, actualEndpointName);
        Assert.Equal("my-audit-queue", auditQueueAddress);
    }
    
    [Fact]
    public void Using_env_var_configuration_respect_endpoint_name()
    {
        var expectedEndpointName = "my-endpoint";
        Environment.SetEnvironmentVariable("NServiceBus:EndpointConfiguration:EndpointName", expectedEndpointName);
        
        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
        
        var endpoint = new LearningEndpoint(config);
        EndpointConfiguration endpointConfiguration = endpoint;
        
        var actualEndpointName = endpointConfiguration.GetSettings().EndpointName();
        
        Assert.Equal(expectedEndpointName, actualEndpointName);
    }
    
    [Fact]
    public void When_auditing_is_disabled_no_audit_queue_is_configured()
    {
        Environment.SetEnvironmentVariable("NServiceBus:EndpointConfiguration:Auditing:Disabled", "True");
        
        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
        
        var endpoint = new LearningEndpoint("my-endpoint", config);
        EndpointConfiguration endpointConfiguration = endpoint;

        var settings = endpointConfiguration.GetSettings();
        var auditSettingsResult = settings.GetOrDefault<object>("NServiceBus.AuditConfigReader+Result");

        Assert.Null(auditSettingsResult);
    }
    
    [Fact]
    public void When_setting_error_queue_name_is_set_as_expected()
    {
        const string expectedErrorQueue = "my-error_queue";
        Environment.SetEnvironmentVariable("NServiceBus:EndpointConfiguration:Recoverability:ErrorQueue", expectedErrorQueue);
        
        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
        
        var endpoint = new LearningEndpoint("my-endpoint", config);
        EndpointConfiguration endpointConfiguration = endpoint;

        var settings = endpointConfiguration.GetSettings();
        var actualErrorQueue = settings.GetOrDefault<string>("errorQueue");

        Assert.Equal(expectedErrorQueue, actualErrorQueue);
    }
}