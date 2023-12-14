using Microsoft.Extensions.Configuration;
using NServiceBus.Serialization;
using NServiceBus.Transport;

namespace NServiceBoXes.Endpoints;

public abstract class EndpointConfigurationManager<TTransport>
    where TTransport : TransportDefinition
{
    const string NServiceBusEndpointConfigurationSectionName = "NServiceBus:EndpointConfiguration";
    Action<TTransport>? transportCustomization;
    Func<IConfigurationSection?, TTransport>? transportFactory;
    bool _useDefaultSerializer = true;
    Action<SerializationExtensions<SystemJsonSerializer>>? _serializerCustomization;
    EndpointConfiguration _endpointConfiguration = null!;

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
    
    protected abstract TTransport CreateTransport(IConfigurationSection? transportConfigurationSection);
    
    protected static void ApplyCommonTransportSettings(IConfigurationSection? transportConfigurationSection,
        TransportDefinition transport)
    {
        if (transportConfigurationSection?["TransportTransactionMode"] is { } transportTransactionMode)
        {
            Enum.TryParse(transportTransactionMode, ignoreCase: false, out TransportTransactionMode ttm);
            transport.TransportTransactionMode = ttm;
        }
    }
    
    public void CustomizeTransport(Action<TTransport> transport)
    {
        this.transportCustomization = transport;
    }
    
    public void OverrideTransport(Func<IConfigurationSection?, TTransport> factory)
    {
        this.transportFactory = factory;
    }
    
    public SerializationExtensions<T> ReplaceDefaultSerializer<T>() where T : SerializationDefinition, new()
    {
        _useDefaultSerializer = false;
        return _endpointConfiguration.UseSerialization<T>();
    }
    
    public void CustomizeDefaultSerializer(Action<SerializationExtensions<SystemJsonSerializer>>? serializerCustomization)
    {
        _serializerCustomization = serializerCustomization;
    }
    
    void Customize(EndpointConfiguration endpointConfiguration,
        IConfigurationSection? endpointConfigurationSection)
    {
        ConfigureAuditing(endpointConfiguration, endpointConfigurationSection);
        ConfigureRecoverability(endpointConfiguration, endpointConfigurationSection);
        
        // create and configure the transport
        var transport = transportFactory != null ? transportFactory(endpointConfigurationSection) : CreateTransport(endpointConfigurationSection?.GetSection("Transport"));
        transportCustomization?.Invoke(transport);
        endpointConfiguration.UseTransport(transport);
        
        // TODO create and configure the persistence

        if (_useDefaultSerializer)
        {
            var serializerConfiguration = endpointConfiguration.UseSerialization<SystemJsonSerializer>();
            _serializerCustomization?.Invoke(serializerConfiguration);
        }
    }

    static IConfigurationSection GetMandatoryEndpointConfigurationSection(IConfiguration configuration)
    {
        var endpointConfigurationSection = configuration.GetSection(NServiceBusEndpointConfigurationSectionName);
        if (endpointConfigurationSection == null)
            throw new Exception($"Cannot find the required '{NServiceBusEndpointConfigurationSectionName}' configuration section");
        
        return endpointConfigurationSection;
    }
    
    static string GetMandatoryEndpointName(IConfigurationSection endpointConfigurationSection)
    {
        return endpointConfigurationSection["EndpointName"]
               ?? throw new ArgumentException(
                   "EndpointName cannot be null. Make sure the " +
                   $"{NServiceBusEndpointConfigurationSectionName}:EndpointName configuration value is set.");
    }

    public EndpointConfiguration CreateEndpointConfiguration(string endpointName, IConfiguration? configuration)
    {
        if (string.IsNullOrWhiteSpace(endpointName))
            throw new ArgumentException("Endpoint name is required and cannot be empty", nameof(endpointName));
        
        _endpointConfiguration = new EndpointConfiguration(endpointName);
        
        var endpointConfigurationSection = configuration?.GetSection(NServiceBusEndpointConfigurationSectionName);
        Customize(_endpointConfiguration, endpointConfigurationSection);

        return _endpointConfiguration;
    }
    
    public EndpointConfiguration CreateEndpointConfiguration(IConfiguration configuration)
    {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        var endpointConfigurationSection = GetMandatoryEndpointConfigurationSection(configuration);
        var endpointName = GetMandatoryEndpointName(endpointConfigurationSection);

        _endpointConfiguration = new EndpointConfiguration(endpointName);
        
        Customize(_endpointConfiguration, endpointConfigurationSection);

        return _endpointConfiguration;
    }
}