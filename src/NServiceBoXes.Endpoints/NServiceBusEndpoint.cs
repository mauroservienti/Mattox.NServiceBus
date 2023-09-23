using Microsoft.Extensions.Configuration;
using NServiceBus;
using NServiceBus.Persistence;
using NServiceBus.Serialization;
using NServiceBus.Transport;

namespace NServiceBoXes.Endpoints;

public abstract class NServiceBusEndpoint<TTransport> where TTransport : TransportDefinition
{
    readonly IConfiguration? _configuration;
    protected EndpointConfiguration EndpointConfiguration{ get; }
    protected IConfigurationSection? EndpointConfigurationSection { get; }
    
    Action<SerializationExtensions<NewtonsoftJsonSerializer>>? _serializerCustomization;
    bool _useDefaultSerializer = true;

    protected TTransport Transport { get; private set; } = null!;
    Action<TTransport>? _transportCustomization;
    Func<IConfiguration?, TTransport>? _transportFactory;

    protected NServiceBusEndpoint(string endpointName, IConfiguration? configuration = null)
    {
        if (endpointName == null) throw new ArgumentNullException(nameof(endpointName));
        
        _configuration = configuration;
        EndpointConfiguration = new EndpointConfiguration(endpointName);
        EndpointConfigurationSection = configuration?.GetSection("NServiceBus:EndpointConfiguration");
    }
    
    protected abstract TTransport CreateTransport(IConfigurationSection? endpointConfigurationSection);

    void ConfigureAuditing()
    {
        var auditSection = EndpointConfigurationSection?.GetSection("Auditing");
        var disableAuditing = bool.Parse(auditSection!["Disabled"] ?? false.ToString());
        if (disableAuditing)
        {
            return;
        }

        var auditQueue = auditSection["Queue"] ?? "audit";
        EndpointConfiguration.AuditProcessedMessagesTo(auditQueue);
    }

    void ConfigureRecoverability()
    {
        var recoverabilitySection = EndpointConfigurationSection?.GetSection("Recoverability");
        
        var errorQueue = recoverabilitySection?["Queue"] ?? "error";
        EndpointConfiguration.SendFailedMessagesTo(errorQueue);
        
        var recoverabilityConfiguration = EndpointConfiguration.Recoverability();

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
    
    
    protected static string GetEndpointNameFromConfigurationOrThrow(IConfiguration? configuration)
    {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));
        
        return configuration.GetSection("NServiceBus:EndpointConfiguration")["EndpointName"]
               ?? throw new ArgumentException(
                   "EndpointName cannot be null. Make sure the " +
                   "NServiceBus:EndpointConfiguration:EndpointName configuration section is set.");
    }
    
    protected virtual void FinalizeConfiguration()
    {
        ConfigureAuditing();
        ConfigureRecoverability();
        
        if (_useDefaultSerializer)
        {
            var serializerConfiguration = EndpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();
            _serializerCustomization!.Invoke(serializerConfiguration);
        }
        
        Transport = _transportFactory != null ? _transportFactory(_configuration) : CreateTransport(EndpointConfigurationSection);

        _transportCustomization?.Invoke(Transport);
        EndpointConfiguration.UseTransport(Transport);
    }
    
    public static implicit operator EndpointConfiguration(NServiceBusEndpoint<TTransport> endpoint)
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
    
    public void CustomizeDefaultSerializer(Action<SerializationExtensions<NewtonsoftJsonSerializer>> serializerCustomization)
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