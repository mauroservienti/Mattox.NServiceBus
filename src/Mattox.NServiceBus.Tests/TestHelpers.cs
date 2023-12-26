using NServiceBus.Configuration.AdvancedExtensibility;
using NServiceBus.Transport;

namespace Mattox.NServiceBus.Tests;

public static class TestHelpers
{
    public static LearningTransport GetTransportDefinition(this LearningEndpoint endpoint)
    {
        var settings = ((EndpointConfiguration)endpoint).GetSettings();
        var transportDefinition = settings.Get<TransportDefinition>();
        
        return (LearningTransport)transportDefinition;
    }
}