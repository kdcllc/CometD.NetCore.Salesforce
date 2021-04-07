# TestApp for CometD.NetCore.Salesforce

[![buymeacoffee](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/vyve0og)

## Give a Star! :star:

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

```csharp
override void ErrorExtension_ConnectionError(
            object sender,
            string e)
        {
            // authentication failure
            if (string.Equals(e, "403::Handshake denied", StringComparison.OrdinalIgnoreCase)
                || string.Equals(e, "403:denied_by_security_policy:create_denied", StringComparison.OrdinalIgnoreCase)
                || string.Equals(e, "403::unknown client", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Handled CometD Exception: {message}", e);

                // 1. Disconnect existing client.
                Disconnect();

                // 2. Invalidate the access token.
                _tokenResponse.Invalidate();

                _logger.LogDebug("Invalidate token for {name} ...", nameof(BayeuxClient));

                // 3. Recreate BayeuxClient and populate it with a new transport with new security headers.
                CreateBayeuxClient();

                // 4. Invoke the Reconnect Event
                Reconnect?.Invoke(this, true);
            }
            else if (e.StartsWith("400::The replayId ", StringComparison.OrdinalIgnoreCase)
                  && e.Contains("you provided was invalid.  Please provide a valid ID, -2 to replay all events, or -1 to replay only new events", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Handled CometD Exception: {message}", e);

                /*
                logic to persist replayId which is being reset to -1 for this particular channel.
                that value is read when the channel is initialized.
                */

                _logger.LogDebug($"Reset {channelName} with ReplayId -1");
                _logger.LogDebug($"Initiate reconnect process..");

                // 1. Disconnect existing client.
                Disconnect();

                // 2. Invalidate the access token.
                _tokenResponse.Invalidate();

                _logger.LogDebug("Invalidate token for {name} ...", nameof(BayeuxClient));

                // 3. Recreate BayeuxClient and populate it with a new transport with new security headers.
                CreateBayeuxClient();

                // 4. Invoke the Reconnect Event
                Reconnect?.Invoke(this, true);
            }
```
