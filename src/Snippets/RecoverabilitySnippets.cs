using Mattox.NServiceBus.Tests;
using NServiceBus;

namespace Snippets;

public class RecoverabilitySnippets
{
    public void AutoRateLimiting(LearningEndpoint endpoint)
    {
        // begin-snippet:  AutoRateLimitingCallbacks
        endpoint.Recoverability.OnRateLimitStarted(token => Task.CompletedTask);
        endpoint.Recoverability.OnRateLimitEnded(token => Task.CompletedTask);
        // end-snippet
    }
    
    public void Failed(LearningEndpoint endpoint)
    {
        // begin-snippet:  FailedMessageCustomization
        endpoint.Recoverability.OnFailedMessage(settings =>
        {
            settings.HeaderCustomization(headers =>
            {
                // Customize failed message headers
            });
        });
        // end-snippet
    }

    public void CustomPolicy(LearningEndpoint endpoint)
    {
        // begin-snippet: CustomRecoverabilityPolicy
        endpoint.Recoverability.UseCustomRecoverabilityPolicy(((config, context) =>
        {
            // Use the configuration and the context
            // to decide how to handle the failing message
            return RecoverabilityAction.ImmediateRetry();
        }));
        // end-snippet
    }
}
