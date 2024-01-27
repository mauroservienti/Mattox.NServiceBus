using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using NServiceBus.Persistence;
using NServiceBus.Serialization;
using NServiceBus.Transport;

[assembly: InternalsVisibleTo("Mattox.NServiceBus.Tests")]

namespace Mattox.NServiceBus;

public abstract class NServiceBusEndpoint<TTransport> where TTransport : TransportDefinition
{
    internal static readonly Func<string, CancellationToken, Task> emptyDiagnosticWriter = (_, _) => Task.CompletedTask;
    const string NServiceBusEndpointConfigurationSectionName = "NServiceBus:EndpointConfiguration";
    readonly IConfiguration? _configuration;
    readonly EndpointConfiguration endpointConfiguration;
    readonly IConfigurationSection? endpointConfigurationSection;
    bool _useDefaultSerializer = true;
    Action<IConfiguration?,SerializationExtensions<SystemJsonSerializer>>? _serializerCustomization;
    TTransport transport;
    Action<IConfiguration?, TTransport>? _transportCustomization;
    Func<IConfiguration?, TTransport>? _transportFactory;
    Action<IConfiguration?, EndpointConfiguration>? endpointConfigurationPreview;
    public EndpointRecoverability Recoverability { get; } = new();

    protected NServiceBusEndpoint(IConfiguration configuration)
        : this(GetEndpointNameFromConfigurationOrThrow(configuration), configuration)
    {
    }

    protected NServiceBusEndpoint(string endpointName, IConfiguration? configuration = null)
    {
        if (endpointName == null) throw new ArgumentNullException(nameof(endpointName));

        _configuration = configuration;
        endpointConfiguration = new EndpointConfiguration(endpointName);
        endpointConfigurationSection = configuration?.GetSection(NServiceBusEndpointConfigurationSectionName);
    }

    protected abstract TTransport CreateTransport(IConfigurationSection? transportConfigurationSection);

    protected static void ApplyCommonTransportSettings(IConfigurationSection? transportConfigurationSection,
        TransportDefinition transport)
    {
        if (transportConfigurationSection?["TransportTransactionMode"] is { } transportTransactionMode)
        {
            if (!Enum.TryParse(transportTransactionMode, ignoreCase: false,
                    out TransportTransactionMode transportTransportTransactionMode))
            {
                throw new ArgumentException("Transport.TransportTransactionMode value cannot be parsed to a valid TransportTransactionMode.");
            }
            transport.TransportTransactionMode = transportTransportTransactionMode;
        }
    }

    static string GetEndpointNameFromConfigurationOrThrow(IConfiguration configuration)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        return configuration.GetSection(NServiceBusEndpointConfigurationSectionName)["EndpointName"]
               ?? throw new ArgumentException(
                   "EndpointName cannot be null. Make sure the " +
                   "NServiceBus:EndpointConfiguration:EndpointName configuration section is set.");
    }

    protected virtual void FinalizeConfiguration()
    {
        var transportConfigurationSection = endpointConfigurationSection?.GetSection("Transport");

        ConfigureTransport(transportConfigurationSection);
        ConfigurePurgeOnStartup(endpointConfiguration, transportConfigurationSection);
        ConfigureMessageProcessingConcurrency(endpointConfiguration, transportConfigurationSection);
        ConfigureAuditing(endpointConfiguration, endpointConfigurationSection);
        ConfigureRecoverability();
        ConfigureSendOnly(endpointConfiguration, endpointConfigurationSection);
        ConfigureInstallers(endpointConfiguration, endpointConfigurationSection);
        ConfigureSerializer();
        ConfigureDiagnostics(endpointConfiguration, endpointConfigurationSection);
        ConfigureAddressingOptions(endpointConfiguration, endpointConfigurationSection);
        
        // TODO create and configure the persistence
        // TODO Outbox

        // TODO License:Text
        // EndpointConfiguration.License();

        // TODO License:Path
        //EndpointConfiguration.LicensePath();

        // TODO
        // EndpointConfiguration.EnableOpenTelemetry();

        endpointConfigurationPreview?.Invoke(_configuration, endpointConfiguration);
    }

    static void ConfigureMessageProcessingConcurrency(EndpointConfiguration endpointConfiguration,
        IConfigurationSection? transportConfigurationSection)
    {
        if (transportConfigurationSection?["MessageProcessingConcurrency"] is { } messageProcessingConcurrencyValue)
        {
            if (!int.TryParse(messageProcessingConcurrencyValue, out var messageProcessingConcurrency))
            {
                throw new ArgumentException("MessageProcessingConcurrency value cannot be parsed to a valid integer.");
            }
            
            endpointConfiguration.LimitMessageProcessingConcurrencyTo(messageProcessingConcurrency);
        }
    }

    static void ConfigureAddressingOptions(EndpointConfiguration endpointConfiguration, IConfigurationSection? endpointConfigurationSection)
    {
        var localAddressOverride = endpointConfigurationSection?["LocalAddressOverride"];
        if (!string.IsNullOrWhiteSpace(localAddressOverride))
        {
            endpointConfiguration.OverrideLocalAddress(localAddressOverride);
        }
        
        var publicReturnAddressOverride = endpointConfigurationSection?["PublicReturnAddressOverride"];
        if (!string.IsNullOrWhiteSpace(publicReturnAddressOverride))
        {
            endpointConfiguration.OverridePublicReturnAddress(publicReturnAddressOverride);
        }

        var instanceDiscriminator = endpointConfigurationSection?["EndpointInstanceDiscriminator"];
        if (!string.IsNullOrWhiteSpace(instanceDiscriminator))
        {
            endpointConfiguration.MakeInstanceUniquelyAddressable(instanceDiscriminator);
        }
    }

    void ConfigureSerializer()
    {
        if (!_useDefaultSerializer)
        {
            return;
        }

        var serializerConfiguration = endpointConfiguration.UseSerialization<SystemJsonSerializer>();
        _serializerCustomization?.Invoke(_configuration, serializerConfiguration);
    }

    // TODO: All the Configure* are static, should this one too?
    void ConfigureTransport(IConfigurationSection? transportConfigurationSection)
    {
        transport = _transportFactory != null
            ? _transportFactory(_configuration)
            : CreateTransport(transportConfigurationSection);

        _transportCustomization?.Invoke(_configuration, transport);
        endpointConfiguration.UseTransport(transport);
    }

    static void ConfigureAuditing(EndpointConfiguration endpointConfiguration,
        IConfigurationSection? endpointConfigurationSection)
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

    void ConfigureRecoverability()
    {
        var recoverabilitySection = endpointConfigurationSection?.GetSection("Recoverability");
        var errorQueue = recoverabilitySection?["ErrorQueue"] ?? "error";
        endpointConfiguration.SendFailedMessagesTo(errorQueue);

        var recoverabilityConfiguration = endpointConfiguration.Recoverability();

        if (recoverabilitySection?.GetSection("Immediate") is { } immediateSection 
            && immediateSection["NumberOfRetries"] is { } immediateNumberOfRetriesValue)
        {
            if (!int.TryParse(immediateNumberOfRetriesValue, out var immediateNumberOfRetries))
            {
                throw new ArgumentException(
                    "Recoverability.Immediate.NumberOfRetries is cannot be parsed to an integer");
            }
            recoverabilityConfiguration.Immediate(immediate => immediate.NumberOfRetries(immediateNumberOfRetries));
        }

        if (recoverabilitySection?.GetSection("Delayed") is { } delayedSection)
        {
            recoverabilityConfiguration.Delayed(
                delayed =>
                {
                    if (delayedSection["NumberOfRetries"] is { } numberOfRetriesValue)
                    {
                        if (!int.TryParse(numberOfRetriesValue, out var numberOfRetries))
                        {
                            throw new ArgumentException(
                                "Recoverability.Delayed.NumberOfRetries cannot be parsed to an integer");
                        }
                        
                        delayed.NumberOfRetries(numberOfRetries);
                    }

                    if (delayedSection["TimeIncrease"] is { } timeIncreaseValue)
                    {
                        if (!TimeSpan.TryParse(timeIncreaseValue, out var timeIncrease))
                        {
                            throw new ArgumentException(
                                "Recoverability.Delayed.TimeIncrease cannot be parsed to a TimeSpan");
                        }
                        
                        delayed.TimeIncrease(timeIncrease);
                    }
                });
        }

        if (Recoverability.OnFailedMessageCallback != null)
        {
            recoverabilityConfiguration.Failed(Recoverability.OnFailedMessageCallback);
        }

        if (Recoverability.CustomRecoverabilityPolicy != null)
        {
            recoverabilityConfiguration.CustomPolicy(Recoverability.CustomRecoverabilityPolicy);
        }

        if (recoverabilitySection?.GetSection("AutomaticRateLimiting") is { } automaticRateLimiting)
        {
            if (automaticRateLimiting["ConsecutiveFailures"] is { } consecutiveFailuresValue)
            {
                if (!int.TryParse(consecutiveFailuresValue, out var consecutiveFailures))
                {
                    throw new ArgumentException(
                        "AutomaticRateLimit.ConsecutiveFailures is a required value and cannot be parsed to an integer");
                }

                if (!TimeSpan.TryParse(automaticRateLimiting["TimeToWaitBetweenThrottledAttempts"],
                        out var timeToWaitBetweenThrottledAttempts))
                {
                    throw new ArgumentException(
                        "AutomaticRateLimit.TimeToWaitBetweenThrottledAttempts is a required value and cannot be parsed to a TimeSpan");
                }

                recoverabilityConfiguration.OnConsecutiveFailures(consecutiveFailures,
                    new RateLimitSettings(
                        timeToWaitBetweenThrottledAttempts: timeToWaitBetweenThrottledAttempts,
                        onRateLimitStarted: Recoverability.OnRateLimitStartedCallback,
                        onRateLimitEnded: Recoverability.OnRateLimitEndedCallback));
            }
        }
    }

    static void ConfigureInstallers(EndpointConfiguration endpointConfiguration,
        IConfigurationSection? endpointConfigurationSection)
    {
        if (!bool.TryParse(endpointConfigurationSection?.GetSection("Installers")?["Enable"] ?? bool.FalseString,
                out var enableInstallers))
        {
            throw new ArgumentException("Installers.Enable value cannot be parsed to a bool.");
        }

        if (enableInstallers)
        {
            endpointConfiguration.EnableInstallers();
        }
    }

    static void ConfigureSendOnly(EndpointConfiguration endpointConfiguration,
        IConfigurationSection? endpointConfigurationSection)
    {
        if (!bool.TryParse(endpointConfigurationSection?["SendOnly"] ?? bool.FalseString, out var isSendOnly))
        {
            throw new ArgumentException("SendOnly value cannot be parsed to a bool.");
        }

        if (isSendOnly)
        {
            endpointConfiguration.SendOnly();
        }
    }

    static void ConfigurePurgeOnStartup(EndpointConfiguration endpointConfiguration,
        IConfigurationSection? transportConfigurationSection)
    {
        if (!bool.TryParse(transportConfigurationSection?["PurgeOnStartup"] ?? bool.FalseString,
                out var purgeOnStartup))
        {
            throw new ArgumentException("PurgeOnStartup value cannot be parsed to a bool.");
        }

        if (purgeOnStartup)
        {
            endpointConfiguration.PurgeOnStartup(true);
        }
    }

    static void ConfigureDiagnostics(EndpointConfiguration endpointConfiguration,
        IConfigurationSection? endpointConfigurationSection)
    {
        var diagnosticsSection = endpointConfigurationSection?.GetSection("Diagnostics");
        if (!bool.TryParse(diagnosticsSection?["Enable"] ?? bool.TrueString,
                out var enabled))
        {
            throw new ArgumentException("Diagnostics:Enable value cannot be parsed to a bool.");
        }

        if (!enabled)
        {
            endpointConfiguration.CustomDiagnosticsWriter(emptyDiagnosticWriter);
        }

        var customPath = diagnosticsSection?["Path"];
        if (!string.IsNullOrWhiteSpace(customPath))
        {
            endpointConfiguration.SetDiagnosticsPath(customPath);
        }
    }

    public static implicit operator EndpointConfiguration(NServiceBusEndpoint<TTransport> endpoint)
    {
        endpoint.FinalizeConfiguration();
        return endpoint.endpointConfiguration;
    }

    public PersistenceExtensions<T> UsePersistence<T>()
        where T : PersistenceDefinition
    {
        return endpointConfiguration.UsePersistence<T>();
    }

    public PersistenceExtensions<T, S> UsePersistence<T, S>()
        where T : PersistenceDefinition
        where S : StorageType
    {
        return endpointConfiguration.UsePersistence<T, S>();
    }

    public SerializationExtensions<T> ReplaceDefaultSerializer<T>() where T : SerializationDefinition, new()
    {
        _useDefaultSerializer = false;
        return endpointConfiguration.UseSerialization<T>();
    }

    public void CustomizeDefaultSerializer(
        Action<IConfiguration?, SerializationExtensions<SystemJsonSerializer>> serializerCustomization)
    {
        _serializerCustomization = serializerCustomization;
    }

    public void CustomizeTransport(Action<IConfiguration?, TTransport> transportCustomization)
    {
        _transportCustomization = transportCustomization;
    }

    public void OverrideTransport(Func<IConfiguration?, TTransport> transportFactory)
    {
        _transportFactory = transportFactory;
    }

    public void PreviewConfiguration(Action<IConfiguration?, EndpointConfiguration> configuration)
    {
        endpointConfigurationPreview = configuration;
    }

    public async Task<IEndpointInstance> Start()
    {
        FinalizeConfiguration();

        var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
        return endpointInstance;
    }
}