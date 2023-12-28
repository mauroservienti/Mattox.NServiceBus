using Mattox.NServiceBus.Tests;

namespace Snippets;

public class AutoRateLimitingSnippets
{
    public void Callbacks(LearningEndpoint endpoint)
    {
        // begin-snippet:  AutoRateLimitingCallbacks
        endpoint.ConfigureRateLimitStartedCallback(token => Task.CompletedTask);
        endpoint.ConfigureRateLimitEndedCallback(token => Task.CompletedTask);
        // end-snippet
    }
}
