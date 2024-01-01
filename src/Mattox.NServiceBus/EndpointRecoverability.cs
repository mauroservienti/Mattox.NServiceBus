using NServiceBus.Transport;

namespace Mattox.NServiceBus;

public class EndpointRecoverability
{
    internal Func<CancellationToken,Task>? OnRateLimitStartedCallback { get; private set; }
    internal Func<CancellationToken,Task>? OnRateLimitEndedCallback { get; private set; }
    internal Action<RetryFailedSettings>? OnFailedMessageCallback { get; private set; }
    internal Func<RecoverabilityConfig, ErrorContext, RecoverabilityAction>? CustomRecoverabilityPolicy { get; private set; }

    public void OnRateLimitStarted(Func<CancellationToken, Task> onRateLimitStarted)
    {
        OnRateLimitStartedCallback = onRateLimitStarted;
    }

    public void OnRateLimitEnded(Func<CancellationToken, Task> onRateLimitEnded)
    {
        OnRateLimitEndedCallback = onRateLimitEnded;
    }

    public void OnFailedMessage(Action<RetryFailedSettings> retrySettings)
    {
        OnFailedMessageCallback = retrySettings;
    }

    public void UseCustomRecoverabilityPolicy(Func<RecoverabilityConfig, ErrorContext, RecoverabilityAction> policy)
    {
        CustomRecoverabilityPolicy = policy;
    }
}