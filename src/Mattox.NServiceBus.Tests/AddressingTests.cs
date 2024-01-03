using Microsoft.Extensions.Configuration;
using NServiceBus.Configuration.AdvancedExtensibility;

namespace Mattox.NServiceBus.Tests;

public class AddressingTests
{
    [Fact]
    public void Overriding_local_address_sets_desired_value()
    {
        var expected = "local-address-override";
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "NServiceBus:EndpointConfiguration:LocalAddressOverride", expected.ToString() }
            })
            .Build();

        var endpoint = new LearningEndpoint("my-endpoint", config);
        EndpointConfiguration endpointConfiguration = endpoint;

        var settings = endpointConfiguration.GetSettings();

        Assert.Equal(expected, settings.GetOrDefault<string>("CustomQueueNameBase"));
    }
    
    [Fact]
    public void Overriding_public_return_address_sets_desired_value()
    {
        var expected = "return-address-override";
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "NServiceBus:EndpointConfiguration:PublicReturnAddressOverride", expected.ToString() }
            })
            .Build();

        var endpoint = new LearningEndpoint("my-endpoint", config);
        EndpointConfiguration endpointConfiguration = endpoint;

        var settings = endpointConfiguration.GetSettings();

        Assert.Equal(expected, settings.GetOrDefault<string>("PublicReturnAddress"));
    }
    
    [Fact]
    public void Setting_instance_discriminator_sets_desired_value()
    {
        var expected = "A";
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "NServiceBus:EndpointConfiguration:EndpointInstanceDiscriminator", expected.ToString() }
            })
            .Build();

        var endpoint = new LearningEndpoint("my-endpoint", config);
        EndpointConfiguration endpointConfiguration = endpoint;

        var settings = endpointConfiguration.GetSettings();

        Assert.Equal(expected, settings.GetOrDefault<string>("EndpointInstanceDiscriminator"));
    }
}