using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ConsoleApp1
{
    class Program
    {
        private static ITelegramBotClient _botClient;
        private static ReceiverOptions _receiverOptions;

        static async Task Main()
        {
            // Load the configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddUserSecrets<Program>()
                .Build();

            // Get the bot token from the configuration
            var botToken = configuration["BotToken"];

            _botClient = new TelegramBotClient(botToken);
            _receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[]
                {
                    UpdateType.Message,
                    UpdateType.CallbackQuery
                },
                ThrowPendingUpdates = true
            };

            using var cts = new CancellationTokenSource();

            _botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, _receiverOptions, cts.Token);

            var me = await _botClient.GetMeAsync();
            Console.WriteLine($"Бот стартовал: {me.Username}");

            Console.ReadLine();

            async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {
                if (update.Message is not null)
                {
                    var chatId = update.Message.Chat.Id;
                    var messageText = update.Message.Text;

                    if (messageText == "/start")
                    {
                        // Создаем клавиатуру с двумя кнопками
                        var keyboard = new ReplyKeyboardMarkup(
                            new[]
                            {
                                new KeyboardButton("Button 1"),
                                new KeyboardButton("Button 2")
                            })
                        {
                            ResizeKeyboard = true
                        };

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Welcome to the bot!",
                            replyMarkup: keyboard,
                            cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: messageText,
                            cancellationToken: cancellationToken);
                    }
                }
            }

            Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
            {
                Console.WriteLine($"Ошибка: {exception.Message}");
                return Task.CompletedTask;
            }
        }
    }
}
