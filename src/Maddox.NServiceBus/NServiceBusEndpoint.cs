using Microsoft.Extensions.Configuration;
using NServiceBus.Persistence;
using NServiceBus.Serialization;
using NServiceBus.Transport;

namespace Maddox.NServiceBus;

public abstract class NServiceBusEndpoint<TEndpointConfigurationManager, TTransport> 
    where TTransport : TransportDefinition 
    where TEndpointConfigurationManager : EndpointConfigurationManager<TTransport>, new()
{
    readonly EndpointConfiguration _endpointConfiguration;
    readonly TEndpointConfigurationManager _configurationManager;

    protected NServiceBusEndpoint(IConfiguration configuration)
    {
        _configurationManager = new TEndpointConfigurationManager();
        _endpointConfiguration = _configurationManager.CreateEndpointConfiguration(configuration);
    }

    protected NServiceBusEndpoint(string endpointName, IConfiguration? configuration = null)
    {
        if (endpointName == null) throw new ArgumentNullException(nameof(endpointName));

        _configurationManager = new TEndpointConfigurationManager();
        _endpointConfiguration = _configurationManager.CreateEndpointConfiguration(endpointName, configuration);
    }
    
    public static implicit operator EndpointConfiguration(NServiceBusEndpoint<TEndpointConfigurationManager, TTransport> endpoint)
    {
        return endpoint._endpointConfiguration;
    } 
    
    public PersistenceExtensions<T> UsePersistence<T>()
        where T : PersistenceDefinition
    {
        return _endpointConfiguration.UsePersistence<T>();
    }
    
    public PersistenceExtensions<T, S> UsePersistence<T, S>()
        where T : PersistenceDefinition
        where S : StorageType
    {
        return _endpointConfiguration.UsePersistence<T, S>();
    }
    
    public SerializationExtensions<T> ReplaceDefaultSerializer<T>() where T : SerializationDefinition, new()
    {
        return _configurationManager.ReplaceDefaultSerializer<T>();
    }
    
    public void CustomizeDefaultSerializer(Action<SerializationExtensions<SystemJsonSerializer>>? serializerCustomization)
    {
        _configurationManager.CustomizeDefaultSerializer(serializerCustomization);
    }
    
    public void CustomizeTransport(Action<TTransport> transportCustomization)
    {
        _configurationManager.CustomizeTransport(transportCustomization);
    }
    
    public void OverrideTransport(Func<IConfiguration?, TTransport> transportFactory)
    {
        _configurationManager.OverrideTransport(transportFactory);
    }
    
    public async Task<IEndpointInstance> Start()
    {
        var endpointInstance = await Endpoint.Start(_endpointConfiguration).ConfigureAwait(false);
        return endpointInstance;
    }
}