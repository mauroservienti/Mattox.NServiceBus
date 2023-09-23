using NServiceBus;
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
}