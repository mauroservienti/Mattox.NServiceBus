<img src="assets/icon.png" width="100" />

# Maddox.NServiceBus

Maddox simplifies [NServiceBus endpoints](https://docs.particular.net/nservicebus/) configuration by providing for supported transports a corresponding Maddox endpoint with sensible defaults. For example, creating and starting a RabbitMQ endpoint could be as easy as:

```csharp
var endpoint = new RabbitMqEndpoint("my-endpoint", connectionString: "host=localhost");
var endpointInstance = await endpoint.Start();
```

## Microsoft configuration extension support

Maddox.NServiceBus can be configured through the [`Microsoft.Extensions.Configuration`](https://www.nuget.org/packages/Microsoft.Extensions.Configuration). The above-presented RabbitMQ endpoint can be configured as follows:

```csharp
Host.CreateDefaultBuilder()
    .UseNServiceBus(hostBuilderContext => new RabbitMqEndpoint(hostBuilderContext.Configuration))
    .Build();
```

The endpoint will retrieve values from the `IConfiguration` object instance. For more information, refer to the [Maddox.NServiceBus configuration options docmentation](/docs).

## Supported endpoints

- [Maddox.NServiceBus.AmazonSQS](https://github.com/mauroservienti/Maddox.NServiceBus.AmazonSQS)
- [Maddox.NServiceBus.RabbitMQ](https://github.com/mauroservienti/Maddox.NServiceBus.RabbitMQ)

## How to get it

- Pre-releases are available on [Feedz.io](https://feedz.io/) ([public feed](https://f.feedz.io/mauroservienti/pre-releases/nuget/index.json))
- Releases on [NuGet.org](https://www.nuget.org/packages?q=Maddox)

## NOTE

> This package is not meant to be used directly. It serves as a base package for other Maddox.NServiceBus Endpoints, such as [Maddox.NServiceBus.AmazonSQS](https://github.com/mauroservienti/Maddox.NServiceBus.AmazonSQS) or [Maddox.NServiceBus.Endpoints.RabbitMQ](https://github.com/mauroservienti/Maddox.NServiceBus.RabbitMQ).

---

Icon — [Box by Angriawan Ditya Zulkarnain](https://thenounproject.com/icon/box-1298424/) from [Noun Project](https://thenounproject.com/browse/icons/term/box/) (CC BY 3.0)
