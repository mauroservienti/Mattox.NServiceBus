using Microsoft.Extensions.Configuration;

namespace NServiceBoXes.Endpoints;

public class LearningEndpointConfigurationManager : EndpointConfigurationManager<LearningTransport>
{
    protected override LearningTransport CreateTransport(IConfigurationSection? transportConfigurationSection)
    {
        
    }

    protected internal override void Customize(EndpointConfiguration endpointConfiguration, IConfigurationSection? endpointConfigurationSection)
    {
        base.Customize(endpointConfiguration, endpointConfigurationSection);
    }
}

public class Snippets
{
    public static void Usage(IConfiguration configuration)
    {
        var endpointConfig = new LearningEndpointConfigurationManager().CreateEndpointConfigurationFrom(configuration);
    }
}