# Configuration

NServiceBoXes endpoints can be configured through the [`Microsoft.Extensions.Configuration`](https://www.nuget.org/packages/Microsoft.Extensions.Configuration). All settings are defined in the `NServiceBus:EndpointConfiguration` section. Different endpoints supporting different transport may define additional settings. Refer to the endpoint-specific documentation for more details.

## Endpoint name

- `NServiceBus:EndpointConfiguration:EndpointName` configures the endpoint name. This setting is mandatory unless specified through the endpoint class constructor.

## Auditing

The `NServiceBus:EndpointConfiguration:Auditing` section allows controlling the [endpoint audit capability](https://docs.particular.net/nservicebus/operations/auditing).

- `NServiceBus:EndpointConfiguration:Auditing:Enabled` (`True`/`False`, defaults to `True`) allows enabling or disabling the endpoint auditing functionalities. By default, NServiceBoXes endpoints have auditing enabled.
- `NServiceBus:EndpointConfiguration:Auditing:AuditQueue` defines the audit queue name; if omitted, a default value of `audit` is used.

## Recoverability

The `NServiceBus:EndpointConfiguration:Recoverability` section allows controlling the [endpoint recoverability capability](https://docs.particular.net/nservicebus/recoverability/).

- `NServiceBus:EndpointConfiguration:Recoverability:ErrorQueue` defines the error queue name; if omitted, a default value of `error` is used.

### Immediate retries

The `NServiceBus:EndpointConfiguration:Recoverability:Immediate` section allows controlling [immediate retry settings](https://docs.particular.net/nservicebus/recoverability/#immediate-retries):

- `NServiceBus:EndpointConfiguration:Recoverability:Immediate:NumberOfRetries` defines the number of times a failing message is retried immediately without delay.

### Delayed retries

The `NServiceBus:EndpointConfiguration:Recoverability:Delayed` section allows controlling [delayed retry settings](https://docs.particular.net/nservicebus/recoverability/#delayed-retries):

- `NServiceBus:EndpointConfiguration:Recoverability:Delayed:NumberOfRetries` defines the number of times a failing message is retried in a delayed fashion.
- `NServiceBus:EndpointConfiguration:Recoverability:Delayed:TimeIncrease` (format: `TimeStamp`) defines how much delay is used between delayed retries
