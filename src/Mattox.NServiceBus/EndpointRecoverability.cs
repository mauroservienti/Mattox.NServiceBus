namespace Mattox.NServiceBus;

public class EndpointRecoverability
{
    internal Func<CancellationToken,Task>? OnRateLimitStartedCallback { get; private set; }
    internal Func<CancellationToken,Task>? OnRateLimitEndedCallback { get; private set; }

    public void OnRateLimitStarted(Func<CancellationToken, Task> onRateLimitStarted)
    {
        OnRateLimitStartedCallback = onRateLimitStarted;
    }

    public void OnRateLimitEnded(Func<CancellationToken, Task> onRateLimitEnded)
    {
        OnRateLimitEndedCallback = onRateLimitEnded;
    }
}