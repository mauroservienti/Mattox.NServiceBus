using Microsoft.Extensions.Configuration;

namespace Maddox.NServiceBus;

public class LearningEndpoint : NServiceBusEndpoint<LearningEndpointConfigurationManager, LearningTransport>
{
    public LearningEndpoint(IConfiguration configuration) : base(configuration)
    {
    }

    public LearningEndpoint(string endpointName, IConfiguration? configuration = null) : base(endpointName, configuration)
    {
    }
}