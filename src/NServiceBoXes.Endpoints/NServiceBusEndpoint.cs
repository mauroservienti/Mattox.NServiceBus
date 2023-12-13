using Microsoft.Extensions.Configuration;
using NServiceBus.Persistence;
using NServiceBus.Serialization;
using NServiceBus.Transport;

namespace NServiceBoXes.Endpoints;

public abstract class NServiceBusEndpoint<TEndpointConfigurationManager, TTransport> 
    where TTransport : TransportDefinition 
    where TEndpointConfigurationManager : EndpointConfigurationManager<TTransport>, new()
{
    readonly IConfiguration? _configuration;
    protected EndpointConfiguration EndpointConfiguration{ get; }
    
    
    Action<SerializationExtensions<SystemJsonSerializer>>? _serializerCustomization;
    bool _useDefaultSerializer = true;

    protected TTransport Transport { get; private set; } = null!;
    Action<TTransport>? _transportCustomization;
    Func<IConfiguration?, TTransport>? _transportFactory;

    protected NServiceBusEndpoint(IConfiguration? configuration = null)
    {
    }

    protected NServiceBusEndpoint(string endpointName, IConfigurationSection? endpointConfigurationSection = null)
    {
        if (endpointName == null) throw new ArgumentNullException(nameof(endpointName));

        var manager = new TEndpointConfigurationManager();
        EndpointConfiguration = new EndpointConfiguration(endpointName);
        manager.Customize(EndpointConfiguration, endpointConfigurationSection);
    }
    
   

    
    
    protected virtual void FinalizeConfiguration()
    {
        ConfigureAuditing();
        ConfigureRecoverability();
        
        if (_useDefaultSerializer)
        {
            var serializerConfiguration = EndpointConfiguration.UseSerialization<SystemJsonSerializer>();
            _serializerCustomization?.Invoke(serializerConfiguration);
        }
        
        Transport = _transportFactory != null ? _transportFactory(_configuration) : CreateTransport(EndpointConfigurationSection);

        _transportCustomization?.Invoke(Transport);
        EndpointConfiguration.UseTransport(Transport);
    }
    
    public static implicit operator EndpointConfiguration(NServiceBusEndpoint<TEndpointConfigurationManager, TTransport> endpoint)
    {
        endpoint.FinalizeConfiguration();
        return endpoint.EndpointConfiguration;
    } 
    
    public PersistenceExtensions<T> UsePersistence<T>()
        where T : PersistenceDefinition
    {
        return EndpointConfiguration.UsePersistence<T>();
    }
    
    public PersistenceExtensions<T, S> UsePersistence<T, S>()
        where T : PersistenceDefinition
        where S : StorageType
    {
        return EndpointConfiguration.UsePersistence<T, S>();
    }
    
    public SerializationExtensions<T> ReplaceDefaultSerializer<T>() where T : SerializationDefinition, new()
    {
        _useDefaultSerializer = false;
        return EndpointConfiguration.UseSerialization<T>();
    }
    
    public void CustomizeDefaultSerializer(Action<SerializationExtensions<SystemJsonSerializer>> serializerCustomization)
    {
        _serializerCustomization = serializerCustomization;
    }
    
    public void CustomizeTransport(Action<TTransport> transportCustomization)
    {
        _transportCustomization = transportCustomization;
    }
    
    public void OverrideTransport(Func<IConfiguration?, TTransport> transportFactory)
    {
        _transportFactory = transportFactory;
    }
    
    public async Task<IEndpointInstance> Start()
    {
        FinalizeConfiguration();

        var endpointInstance = await Endpoint.Start(EndpointConfiguration).ConfigureAwait(false);
        return endpointInstance;
    }
}

public abstract class EndpointConfigurationManager<TTransport>
{
    const string NServiceBusEndpointConfigurationSectionName = "NServiceBus:EndpointConfiguration";
    
    static void ConfigureAuditing(EndpointConfiguration endpointConfiguration, IConfigurationSection? endpointConfigurationSection)
    {
        var auditSection = endpointConfigurationSection?.GetSection("Auditing");
        var enableAuditing = bool.Parse(auditSection?["Enabled"] ?? true.ToString());
        if (!enableAuditing)
        {
            return;
        }

        var auditQueue = auditSection?["AuditQueue"] ?? "audit";
        endpointConfiguration.AuditProcessedMessagesTo(auditQueue);
    }

    static void ConfigureRecoverability(EndpointConfiguration endpointConfiguration, IConfigurationSection? endpointConfigurationSection)
    {
        var recoverabilitySection = endpointConfigurationSection?.GetSection("Recoverability");
        
        var errorQueue = recoverabilitySection?["ErrorQueue"] ?? "error";
        endpointConfiguration.SendFailedMessagesTo(errorQueue);
        
        var recoverabilityConfiguration = endpointConfiguration.Recoverability();

        if (recoverabilitySection?.GetSection("Immediate") is { } immediateSection)
        {
            recoverabilityConfiguration.Immediate(
                immediate =>
                {
                    if(immediateSection["NumberOfRetries"] is {} numberOfRetries)
                    {
                        immediate.NumberOfRetries(int.Parse(numberOfRetries));
                    }
                }); 
        }
        
        if(recoverabilitySection?.GetSection("Delayed") is { } delayedSection)
        {
            recoverabilityConfiguration.Delayed(
                delayed =>
                {
                    if(delayedSection["NumberOfRetries"] is { } numberOfRetries)
                    {
                        delayed.NumberOfRetries(int.Parse(numberOfRetries));
                    }
                    
                    if (delayedSection["TimeIncrease"] is {} timeIncrease)
                    {;
                        delayed.TimeIncrease(TimeSpan.Parse(timeIncrease));
                    }
                });
        }
    }
    
    static string GetEndpointNameFromConfigurationOrThrow(IConfigurationSection endpointConfigurationSection)
    {
        if (endpointConfigurationSection == null) throw new ArgumentNullException(nameof(endpointConfigurationSection));
        
        return endpointConfigurationSection["EndpointName"]
               ?? throw new ArgumentException(
                   "EndpointName cannot be null. Make sure the " +
                   "NServiceBus:EndpointConfiguration:EndpointName configuration section/value is set.");
    }

    protected abstract TTransport CreateTransport(IConfigurationSection? transportConfigurationSection);
    
    protected internal virtual void Customize(EndpointConfiguration endpointConfiguration,
        IConfigurationSection? endpointConfigurationSection)
    {
        ConfigureAuditing(endpointConfiguration, endpointConfigurationSection);
        ConfigureRecoverability(endpointConfiguration, endpointConfigurationSection);
        
        // create the transport
        var transport = _transportFactory != null ? _transportFactory(_configuration) : CreateTransport(endpointConfigurationSection?.GetSection("Transport"));
        endpointConfiguration.UseTransport(transport);
        //create persistence

    }
    
    public void CustomizeEndpointConfiguration(EndpointConfiguration endpointConfiguration, IConfiguration configuration)
    {
        var endpointConfigurationSection = configuration.GetSection(NServiceBusEndpointConfigurationSectionName);
        Customize(endpointConfiguration, endpointConfigurationSection);
    }
    
    public EndpointConfiguration CreateEndpointConfigurationFrom(IConfiguration configuration)
    {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        var endpointConfigurationSection = configuration.GetSection(NServiceBusEndpointConfigurationSectionName);
        var endpointName = GetEndpointNameFromConfigurationOrThrow(endpointConfigurationSection);
        var config = new EndpointConfiguration(endpointName);

        Customize(config, endpointConfigurationSection);
        
        return config;
    }
}