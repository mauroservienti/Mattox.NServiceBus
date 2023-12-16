# Configuration

Mattox.NServiceBus endpoints can be configured through the [`Microsoft.Extensions.Configuration`](https://www.nuget.org/packages/Microsoft.Extensions.Configuration). All settings are defined in the `NServiceBus:EndpointConfiguration` section. Different endpoints supporting different transport may define additional settings. Refer to the endpoint-specific documentation for more details.

Root section: `NServiceBus:EndpointConfiguration`.

## Endpoint name

- `EndpointName` configures the endpoint name. This setting is mandatory unless specified through the endpoint class constructor.

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
