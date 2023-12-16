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

    // TODO: Why do inheritors need the EndpointConfiguration?
    protected EndpointConfiguration EndpointConfiguration { get; }

    // TODO: Why do inheritors need the EndpointConfigurationSection?
    protected IConfigurationSection? EndpointConfigurationSection { get; }

    Action<SerializationExtensions<SystemJsonSerializer>>? _serializerCustomization;
    bool _useDefaultSerializer = true;

    // TODO: Why do inheritors need the Transport?
    protected TTransport Transport { get; private set; } = null!;
    Action<TTransport>? _transportCustomization;
    Func<IConfiguration?, TTransport>? _transportFactory;
    Action<EndpointConfiguration>? endpointConfigurationPreview;

    protected NServiceBusEndpoint(IConfiguration configuration)
        : this(GetEndpointNameFromConfigurationOrThrow(configuration), configuration)
    {
    }

    protected NServiceBusEndpoint(string endpointName, IConfiguration? configuration = null)
    {
        if (endpointName == null) throw new ArgumentNullException(nameof(endpointName));

        _configuration = configuration;
        EndpointConfiguration = new EndpointConfiguration(endpointName);
        EndpointConfigurationSection = configuration?.GetSection(NServiceBusEndpointConfigurationSectionName);
    }

    protected abstract TTransport CreateTransport(IConfigurationSection? transportConfigurationSection);

    protected static void ApplyCommonTransportSettings(IConfigurationSection? transportConfigurationSection,
        TransportDefinition transport)
    {
        if (transportConfigurationSection?["TransportTransactionMode"] is { } transportTransactionMode)
        {
            // TODO throw if cannot parse
            // TODO test
            Enum.TryParse(transportTransactionMode, ignoreCase: false,
                out TransportTransactionMode transportTransportTransactionMode);
            transport.TransportTransactionMode = transportTransportTransactionMode;
        }
    }

    // TODO this should not need to be protected
    protected static string GetEndpointNameFromConfigurationOrThrow(IConfiguration configuration)
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
        var transportConfigurationSection = EndpointConfigurationSection?.GetSection("Transport");

        ConfigureTransport(transportConfigurationSection);
        ConfigurePurgeOnStartup(EndpointConfiguration, transportConfigurationSection);
        ConfigureAuditing(EndpointConfiguration, EndpointConfigurationSection);
        ConfigureRecoverability(EndpointConfiguration, EndpointConfigurationSection);
        ConfigureSendOnly(EndpointConfiguration, EndpointConfigurationSection);
        ConfigureInstallers(EndpointConfiguration, EndpointConfigurationSection);
        ConfigureSerializer();
        ConfigureDiagnostics(EndpointConfiguration, EndpointConfigurationSection);

        // TODO create and configure the persistence
        // TODO Outbox

        // TODO - default not set 
        // EndpointConfiguration.LimitMessageProcessingConcurrencyTo();

        // TODO License:Text
        // EndpointConfiguration.License();

        // TODO License:Path
        //EndpointConfiguration.LicensePath();

        // TODO
        // EndpointConfiguration.EnableOpenTelemetry();

        // TODO
        // EndpointConfiguration.OverrideLocalAddress();

        // TODO
        // EndpointConfiguration.OverridePublicReturnAddress();

        // TODO
        // EndpointConfiguration.MakeInstanceUniquelyAddressable();

        // TODO
        // EndpointConfiguration.UniquelyIdentifyRunningInstance();

        endpointConfigurationPreview?.Invoke(EndpointConfiguration);
    }

    // TODO: All the Configure* are static, should this one too?
    void ConfigureSerializer()
    {
        if (!_useDefaultSerializer)
        {
            return;
        }

        var serializerConfiguration = EndpointConfiguration.UseSerialization<SystemJsonSerializer>();
        _serializerCustomization?.Invoke(serializerConfiguration);
    }

    // TODO: All the Configure* are static, should this one too?
    void ConfigureTransport(IConfigurationSection? transportConfigurationSection)
    {
        Transport = _transportFactory != null
            ? _transportFactory(_configuration)
            : CreateTransport(transportConfigurationSection);

        _transportCustomization?.Invoke(Transport);
        EndpointConfiguration.UseTransport(Transport);
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

    static void ConfigureRecoverability(EndpointConfiguration endpointConfiguration,
        IConfigurationSection? endpointConfigurationSection)
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
                    if (immediateSection["NumberOfRetries"] is { } numberOfRetries)
                    {
                        immediate.NumberOfRetries(int.Parse(numberOfRetries));
                    }
                });
        }

        if (recoverabilitySection?.GetSection("Delayed") is { } delayedSection)
        {
            recoverabilityConfiguration.Delayed(
                delayed =>
                {
                    if (delayedSection["NumberOfRetries"] is { } numberOfRetries)
                    {
                        delayed.NumberOfRetries(int.Parse(numberOfRetries));
                    }

                    if (delayedSection["TimeIncrease"] is { } timeIncrease)
                    {
                        ;
                        delayed.TimeIncrease(TimeSpan.Parse(timeIncrease));
                    }
                });
        }

        // TODO allow to customize with a delegate the Failed policy
        // recoverabilityConfiguration.Failed()

        // TODO allow to register with a delegate a custom retry policy 
        // recoverabilityConfiguration.CustomPolicy()

        // TODO Automatic rate limiting
        // https://docs.particular.net/nservicebus/recoverability/#automatic-rate-limiting
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

    public void CustomizeDefaultSerializer(
        Action<SerializationExtensions<SystemJsonSerializer>> serializerCustomization)
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

    public void PreviewConfiguration(Action<EndpointConfiguration> endpointConfiguration)
    {
        endpointConfigurationPreview = endpointConfiguration;
    }

    public async Task<IEndpointInstance> Start()
    {
        FinalizeConfiguration();

        var endpointInstance = await Endpoint.Start(EndpointConfiguration).ConfigureAwait(false);
        return endpointInstance;
    }
}