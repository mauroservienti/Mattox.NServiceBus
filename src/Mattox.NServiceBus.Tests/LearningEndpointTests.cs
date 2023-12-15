using Microsoft.Extensions.Configuration;
using NServiceBus;
using NServiceBus.Configuration.AdvancedExtensibility;

namespace Mattox.NServiceBus.Tests;

public class LearningEndpointTests
{
    [Fact]
    public void Basic_endpoint_respect_name_and_default_values()
    {
        const string expectedEndpointName = "my-endpoint";
        var endpoint = new LearningEndpoint(expectedEndpointName);
        EndpointConfiguration endpointConfiguration = endpoint;
        
        var actualEndpointName = endpointConfiguration.GetSettings().EndpointName();
        
        Assert.Equal(expectedEndpointName, actualEndpointName);
    }
    
    [Fact]
    public void Using_json_configuration_respect_settings()
    {
        const string expectedEndpointName = "my-endpoint";
     
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
        const string expectedEndpointName = "my-endpoint";
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                {"NServiceBus:EndpointConfiguration:EndpointName", expectedEndpointName}        
            })
            .Build();
        
        var endpoint = new LearningEndpoint(config);
        EndpointConfiguration endpointConfiguration = endpoint;
        
        var actualEndpointName = endpointConfiguration.GetSettings().EndpointName();
        
        Assert.Equal(expectedEndpointName, actualEndpointName);
    }
    
    [Fact]
    public void When_auditing_is_disabled_no_audit_queue_is_configured()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                {"NServiceBus:EndpointConfiguration:Auditing:Enabled", "False"}        
            })
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
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                {"NServiceBus:EndpointConfiguration:Recoverability:ErrorQueue", expectedErrorQueue}        
            })
            .Build();
        
        var endpoint = new LearningEndpoint("my-endpoint", config);
        EndpointConfiguration endpointConfiguration = endpoint;

        var settings = endpointConfiguration.GetSettings();
        var actualErrorQueue = settings.GetOrDefault<string>("errorQueue");

        Assert.Equal(expectedErrorQueue, actualErrorQueue);
    }

    [Fact]
    public void When_setting_endpoint_config_preview_delegate_is_invoked_when_retrieving_config()
    {
        var handlerInvoked = false;
        var endpoint = new LearningEndpoint("my-endpoint");
        endpoint.PreviewConfiguration(configuration =>
        {
            handlerInvoked = true;
        });
        EndpointConfiguration endpointConfiguration = endpoint;
        
        Assert.True(handlerInvoked);
    }
    
    [Fact]
    public async Task When_setting_endpoint_config_preview_delegate_is_invoked_when_starting_endpoint()
    {
        var handlerInvoked = false;
        var endpoint = new LearningEndpoint("my-endpoint");
        endpoint.PreviewConfiguration(configuration =>
        {
            handlerInvoked = true;
        });

        IEndpointInstance? instance = null;
        try
        {
            instance = await endpoint.Start();
        }
        finally
        {
            await instance?.Stop()!;
        }
        
        Assert.True(handlerInvoked);
    }

    [Fact]
    public void Setting_send_only_creates_send_only_endpoint()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                {"NServiceBus:EndpointConfiguration:SendOnly", true.ToString()}        
            })
            .Build();
        
        var endpoint = new LearningEndpoint("my-endpoint", config);
        EndpointConfiguration endpointConfiguration = endpoint;
        
        var settings = endpointConfiguration.GetSettings();
        var isSendOnly = settings.GetOrDefault<bool>("Endpoint.SendOnly");
        
        Assert.True(isSendOnly);
    }
    
    [Fact]
    public void Setting_send_only_to_invalid_bool_throws()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                {"NServiceBus:EndpointConfiguration:SendOnly", "cannot be parsed to a bool"}        
            })
            .Build();

        Assert.Throws<ArgumentException>(() =>
        {
            var endpoint = new LearningEndpoint("my-endpoint", config);
            EndpointConfiguration endpointConfiguration = endpoint; 
        });
    }
    
    [Fact]
    public void Enabling_installers_creates_endpoint_with_installers()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                {"NServiceBus:EndpointConfiguration:Installers:Enable", true.ToString()}
            })
            .Build();
        
        var endpoint = new LearningEndpoint("my-endpoint", config);
        EndpointConfiguration endpointConfiguration = endpoint;
        
        var settings = endpointConfiguration.GetSettings();
        var installersEnabled = settings.GetOrDefault<bool>("Installers.Enable");
        
        Assert.True(installersEnabled);
    }
    
    [Fact]
    public void Enabling_installers_with_non_parsable_value_throws()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                {"NServiceBus:EndpointConfiguration:Installers:Enable", "cannot be parsed to a bool"}        
            })
            .Build();

        Assert.Throws<ArgumentException>(() =>
        {
            var endpoint = new LearningEndpoint("my-endpoint", config);
            EndpointConfiguration endpointConfiguration = endpoint; 
        });
    }
}