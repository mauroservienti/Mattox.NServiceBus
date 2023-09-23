<img src="assets/icon.png" width="100" />

# NServiceBoXes.Endpoints

NServiceBoXes Endpoints simplify [NServiceBus endpoints](https://docs.particular.net/nservicebus/) configuration by providing for supported transports a corresponding NServiceBoXes endpoint with sensible defaults. For example, creating and starting a RabbitMQ endpoint could be as easy as:

```csharp
var endpoint = new RabbitMqEndpoint("my-endpoint", connectionString: "host=localhost");
var endpointInstance = await endpoint.Start();
```

## Microsoft configuration extension support

NServiceBoXes endpoints can be configured through the [`Microsoft.Extensions.Configuration`](https://www.nuget.org/packages/Microsoft.Extensions.Configuration). The above-presented RabbitMQ endpoint can be configured as follows:

```
Host.CreateDefaultBuilder()
    .UseNServiceBus(hostBuilderContext => new RabbitMqEndpoint(hostBuilderContext.Configuration))
    .Build();
```

The endpoint will retrieve values from the `IConfiguration` object instance. For more information, refer to the [NServiceBoXes.Endpoints.RabbitMQ](https://github.com/mauroservienti/NServiceBoXes.Endpoints.RabbitMQ) documentation.

## Supported endpoints

- [NServiceBoXes.Endpoints.AmazonSQS](https://github.com/mauroservienti/NServiceBoXes.Endpoints.AmazonSQS)
- [NServiceBoXes.Endpoints.RabbitMQ](https://github.com/mauroservienti/NServiceBoXes.Endpoints.RabbitMQ)

## NOTE

> This package is not meant to be used directly. It serves as a base package for other NServiceBoXes Endpoints, such as [NServiceBoXes.Endpoints.AmazonSQS](https://github.com/mauroservienti/NServiceBoXes.Endpoints.AmazonSQS), [NServiceBoXes.Endpoints.RabbitMQ](https://github.com/mauroservienti/NServiceBoXes.Endpoints.RabbitMQ), and many more.

---

Icon — [Box by Angriawan Ditya Zulkarnain](https://thenounproject.com/icon/box-1298424/) from [Noun Project](https://thenounproject.com/browse/icons/term/box/) (CC BY 3.0)
