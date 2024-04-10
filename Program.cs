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
    // Console.WriteLine($"{user.FirstName} wrote {text}");

    // When we get a command, we react accordingly
    if (text.StartsWith("/"))
    {
        await HandleCommand(user.Id, text);
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
