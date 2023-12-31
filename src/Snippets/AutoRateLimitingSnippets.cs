using Mattox.NServiceBus.Tests;

namespace Snippets;

public class AutoRateLimitingSnippets
{
    public void Callbacks(LearningEndpoint endpoint)
    {
        // begin-snippet:  AutoRateLimitingCallbacks
        endpoint.Recoverability.OnRateLimitStarted(token => Task.CompletedTask);
        endpoint.Recoverability.OnRateLimitEnded(token => Task.CompletedTask);
        // end-snippet
    }
}
