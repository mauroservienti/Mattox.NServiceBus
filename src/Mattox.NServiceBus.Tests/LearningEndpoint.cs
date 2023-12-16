using Microsoft.Extensions.Configuration;

namespace Mattox.NServiceBus.Tests;

public class LearningEndpoint : NServiceBusEndpoint<LearningTransport>
{
    public LearningEndpoint(IConfiguration configuration)
        : base(configuration)
    {
    }

    public LearningEndpoint(string endpointName, IConfiguration? configuration = null)
        : base(endpointName, configuration)
    {
    }

    protected override LearningTransport CreateTransport(IConfigurationSection? transportConfigurationSection)
    {
        LearningTransport transport = new();

        ApplyCommonTransportSettings(transportConfigurationSection, transport);

        if (transportConfigurationSection?["StorageDirectory"] is { } storageDirectory)
        {
            transport.StorageDirectory = storageDirectory;
        }

        if (transportConfigurationSection?["RestrictPayloadSize"] is { } restrictPayloadSize)
        {
            transport.RestrictPayloadSize = bool.Parse(restrictPayloadSize);
        }

        return transport;
    }
}