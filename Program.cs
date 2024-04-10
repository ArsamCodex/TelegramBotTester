using NBitcoin;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

var botToken = "REPLACE WHIT YOUR BOT TOKEN ";
// You need a TelegramBotClient instance if you want access to the Bot API methods.
var bot = new TelegramBotClient(botToken);

using var cts = new CancellationTokenSource();
var screaming = true;


// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool, so we use cancellation token
bot.StartReceiving(
    updateHandler: HandleUpdate,
    pollingErrorHandler: HandleError,
    cancellationToken: cts.Token
);
// Tell the user the bot is online
Console.WriteLine("Start listening for updates. Press enter to stop");
Console.ReadLine();
// Send cancellation request to stop the bot
cts.Cancel();
async Task HandleUpdate(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
{
    switch (update.Type)
    {
        // A message was received
        case UpdateType.Message:
            await HandleMessage(update.Message!);
            break;

        // A button was pressed
        case UpdateType.CallbackQuery:
            // await HandleButton(update.CallbackQuery!);
            break;
    }
}
async Task HandleError(ITelegramBotClient _, Exception exception, CancellationToken cancellationToken)
{
    await Console.Error.WriteLineAsync(exception.Message);
}
async Task HandleMessage(Message msg)
{
    var user = msg.From;
    var text = msg.Text ?? string.Empty;

    if (user is null)
        return;

    // Print to console
    if (text.Contains("bigdaddy"))
    {
        Console.WriteLine($"This Question is invalid");
    }
   

    if (text.StartsWith("/"))
    {
        await HandleCommand(user.Id, text);
    }
    if (text=="BTC")
    {
        var x = GetBTCPrice();
        var y = x.ToString();
        await bot.SendTextMessageAsync(user.Id, y, entities: msg.Entities);
    }
    if (text == "BTCWALLET")
    {
        var x = GenerateBitcoinWallet;
        var y = x.ToString();
        await bot.SendTextMessageAsync(user.Id, y, entities: msg.Entities);
    }
    else if (screaming && text.Length > 0)
    {
        // To preserve the markdown, we attach entities (bold, italic..)
        await bot.SendTextMessageAsync(user.Id, text.ToUpper(), entities: msg.Entities);
    }
    else
    {   // This is equivalent to forwarding, without the sender's name
        await bot.CopyMessageAsync(user.Id, user.Id, msg.MessageId);
    }
}
async Task HandleCommand(long userId, string command)
{
    switch (command)
    {
        case "/scream":
            screaming = true;
            break;

        case "/whisper":
            screaming = false;
            break;
            
        case "/menu":
           // await SendMenu(userId);
            break;
    }

    await Task.CompletedTask;
}

static async Task<string> GetBTCPrice()
{
    // Binance API endpoint for getting current price
    string endpoint = "https://www.binance.com/api/v3/ticker/price?symbol=BTCUSDT";

    // Create HttpClient instance
    using (var client = new HttpClient())
    {
        try
        {
            // Send GET request to Binance API
            var response = await client.GetAsync(endpoint);

            // Check if request was successful
            if (response.IsSuccessStatusCode)
            {
                // Read response content as string
                string responseBody = await response.Content.ReadAsStringAsync();

                // Parse JSON response to get BTC price
                string btcPriceString = responseBody.Split(':')[2].Split(',')[0].Trim('"');

                return btcPriceString;
            }
            else
            {
                // If request failed, throw an exception
                throw new Exception("Error: " + response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            // Rethrow the exception
            throw new Exception("Error: " + ex.Message);
        }
    }
}
  string GenerateBitcoinWallet()
{
    // Generate a new random private key
    Key privateKey = new Key();

    // Get the public key and Bitcoin address from the private key
    BitcoinPubKeyAddress address =(BitcoinPubKeyAddress) privateKey.PubKey.GetAddress(ScriptPubKeyType.Legacy,Network.Main);

    // Format the private key, public key, and Bitcoin address as a string
    string walletInfo = $"Private Key: {privateKey.GetWif(Network.Main)}\n" +
                        $"Public Key: {privateKey.PubKey}\n" +
                        $"Bitcoin Address: {address}";

    return walletInfo;
}
