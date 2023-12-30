using Mattox.NServiceBus.Tests;

namespace Snippets;

public class AutoRateLimitingSnippets
{
    public void Callbacks(LearningEndpoint endpoint)
    {
        // begin-snippet:  AutoRateLimitingCallbacks
        endpoint.EndpointRecoverability.OnRateLimitStarted(token => Task.CompletedTask);
        endpoint.EndpointRecoverability.OnRateLimitEnded(token => Task.CompletedTask);
        // end-snippet
    }
}
