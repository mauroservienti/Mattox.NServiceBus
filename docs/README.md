# Configuration

Mattox.NServiceBus endpoints can be configured through the [`Microsoft.Extensions.Configuration`](https://www.nuget.org/packages/Microsoft.Extensions.Configuration). All settings are defined in the `NServiceBus:EndpointConfiguration` section. Different endpoints supporting different transport may define additional settings. Refer to the endpoint-specific documentation for more details.

Root section: `NServiceBus:EndpointConfiguration`.

## Endpoint name

- `EndpointName` configures the endpoint name. This setting is mandatory unless specified through the endpoint class constructor.

## SendOnly

- `SendOnly` (`True`/`False`, defaults to `False`) configures the endpoint as a [send only endpoint](https://docs.particular.net/nservicebus/hosting/#self-hosting-send-only-hosting).

## Installers

Section full name: `NServiceBus:EndpointConfiguration:Installers`

- `Enable` (`True`/`False`, defaults to `False`) enables the [endpoint installers](https://docs.particular.net/nservicebus/operations/installers).

## Startup diagnostics

Section full name:  `NServiceBus:EndpointConfiguration:Diagnostics`

- `Enable` (`True`/`False`, defaults to `True`) enables the [endpoint startup diagnostics](https://docs.particular.net/nservicebus/hosting/startup-diagnostics).

## Auditing

Section full name: `NServiceBus:EndpointConfiguration:Auditing`

The `Auditing` section allows controlling the [endpoint audit capability](https://docs.particular.net/nservicebus/operations/auditing).

- `Enabled` (`True`/`False`, defaults to `True`) allows enabling or disabling the endpoint auditing functionalities. By default, NServiceBoXes endpoints have auditing enabled.
- `AuditQueue` defines the audit queue name; if omitted, a default value of `audit` is used.

## Recoverability

Section full name: `NServiceBus:EndpointConfiguration:Recoverability`

The `Recoverability` section allows controlling the [endpoint recoverability capability](https://docs.particular.net/nservicebus/recoverability/).

- `ErrorQueue` defines the error queue name; if omitted, a default value of `error` is used.

### Immediate retries

Section full name: `NServiceBus:EndpointConfiguration:Recoverability:Immediate`

The Recoverability `Immediate` sub-section allows controlling [immediate retry settings](https://docs.particular.net/nservicebus/recoverability/#immediate-retries):

- `NumberOfRetries` defines the number of times a failing message is retried immediately without delay.

### Delayed retries

Section full name: `NServiceBus:EndpointConfiguration:Recoverability:Delayed`

The `Delayed` sub-section allows controlling [delayed retry settings](https://docs.particular.net/nservicebus/recoverability/#delayed-retries):

- `NumberOfRetries` defines the number of times a failing message is retried in a delayed fashion.
- `TimeIncrease` (format: `TimeStamp`) defines how much delay is used between delayed retries

### Automatic Rate Limiting

Section full name: `NServiceBus:EndpointConfiguration:Recoverability:AutomaticRateLimiting`

The `AutomaticRateLimiting` section allows configuring the endpoint [automatic rate limiting](https://docs.particular.net/nservicebus/recoverability/#automatic-rate-limiting) feature.

- `ConsecutiveFailures` defines the number of failure that trigger the rate limiting feature
- `TimeToWaitBetweenThrottledAttempts` (optional, format: `TimeSpan`, defaults to 1 second) defines the time to wait between throttled attempts.

It's also possible to define code callbacks that will be triggered when rate limiting starts and ends:

<!-- snippet: AutoRateLimitingCallbacks -->
<a id='snippet-autoratelimitingcallbacks'></a>
```cs
endpoint.EndpointRecoverability.OnRateLimitStarted(token => Task.CompletedTask);
endpoint.EndpointRecoverability.OnRateLimitEnded(token => Task.CompletedTask);
```
<sup><a href='/src/Snippets/AutoRateLimitingSnippets.cs#L9-L12' title='Snippet source file'>snippet source</a> | <a href='#snippet-autoratelimitingcallbacks' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Transport

Section full name: `NServiceBus:EndpointConfiguration:Transport`

### PurgeOnStartup

NOTE: It's suggested to not discard messages in production.

- `PurgeOnStartup` (`True`/`False`, defaults to `False`) configures the endpoint to [discard input queue messages](https://docs.particular.net/nservicebus/messaging/discard-old-messages#discarding-messages-at-startup) a startup and start fresh.

### Transport Transaction

- `TransportTransactionMode` (TransactionScope, SendsAtomicWithReceive, ReceiveOnly, None. The default value depends on the transport of choice) allows defining the [endpoint message processing transaction guarantees](https://docs.particular.net/transports/transactions).
