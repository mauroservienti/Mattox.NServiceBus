using Microsoft.Extensions.Configuration;

namespace Maddox.NServiceBus;

public class LearningEndpointConfigurationManager : EndpointConfigurationManager<LearningTransport>
{
    protected override LearningTransport CreateTransport(IConfigurationSection? transportConfigurationSection)
    {
        LearningTransport transport = new ();
        
        ApplyCommonTransportSettings(transportConfigurationSection, transport);
        
        if (transportConfigurationSection?["StorageDirectory"] is { } storageDirectory)
        {
            transport.StorageDirectory = storageDirectory;
        }
        
        if (transportConfigurationSection?["RestrictPayloadSize"] is { } restrictPayloadSize)
        {
            transport.RestrictPayloadSize =  bool.Parse(restrictPayloadSize);
        }

        return transport;
    }
}