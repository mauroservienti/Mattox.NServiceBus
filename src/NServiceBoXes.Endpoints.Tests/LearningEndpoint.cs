using Microsoft.Extensions.Configuration;
using NServiceBus;

namespace NServiceBoXes.Endpoints.Tests;

public class LearningEndpoint : NServiceBusEndpoint<LearningTransport>
{
    public LearningEndpoint(IConfiguration configuration) 
        : base(GetEndpointNameFromConfigurationOrThrow(configuration), configuration)
    {
    }
    
    public LearningEndpoint(string endpointName, IConfiguration? configuration = null) 
        : base(endpointName, configuration)
    {
    }

    protected override LearningTransport CreateTransport(IConfigurationSection? endpointConfigurationSection)
    {
        return new LearningTransport();
    }
}