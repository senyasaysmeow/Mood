using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Mood
{
    class Program
    {
        static async Task Main(string[] args)
        {
            SpotifyClient sp = new SpotifyClient();
            var client = new TelegramBotClient(Constants.TELEGRAM_BOT_TOKEN);
            client.StartReceiving((botClient, update, token) => Update(botClient, update, token, sp), Error);
            
            Console.ReadLine();
        }

        async static Task Update(ITelegramBotClient botClient, Update update, CancellationToken token, SpotifyClient sp)
        {
            var message = update.Message;
            if (message?.Text != null)
            {
                if (message.Text.ToLower() == "/start")
                {
                    var sentMassage = await botClient.SendTextMessageAsync(message.Chat.Id, $"Вітаю! \nЩоб авторизуватися, перейдіть за посиланням нижче:\n{sp.getLink()}");
                    await sp.AUTH();
                    await botClient.DeleteMessageAsync(message.Chat.Id, sentMassage.MessageId);
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Успішно авторизовано!");
                    await botClient.SendPhotoAsync(
                        chatId: message.Chat.Id,
                        photo: InputFile.FromUri("https://i.pinimg.com/564x/48/1d/c7/481dc7e0c20835bcb3fc9abcd921b27b.jpg"), 
                        caption: "Тепер опишіть свій настрій від 0.0 до 1.0 :)"
                    );
                }
                else if (float.TryParse(message.Text, out float mood) && mood >= 0.0 && mood <= 1.0)
                {
                    var sentMassage = await botClient.SendTextMessageAsync(message.Chat.Id, $"Готуємо плейлист для вас...");
                    var tracks = await sp.GetMoodPlaylist(mood);
                    var pl = await sp.CreatePlaylist($"mood {mood}");
                    await sp.AddTracksToPlaylist(pl, tracks);
                    await botClient.DeleteMessageAsync(message.Chat.Id, sentMassage.MessageId);
                    await botClient.SendPhotoAsync(
                        chatId: message.Chat.Id,
                        photo: InputFile.FromUri("https://i.pinimg.com/564x/8b/eb/5a/8beb5ade3368528534e1ff6cd9d50724.jpg"), 
                        caption: $"Ось плейлист підібраний спеціально під ваш настрій:\nhttps://open.spotify.com/playlist/{pl}"
                    );
                }
                else if (message.Text.ToLower() == "/restart")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Якщо ви хочете створити ще один плейлист за настроєм просто опишіть цей настрій від 0.0 до 1.0 :)");
                }
            }
        }
        
        private static Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            throw new NotImplementedException();
        }
    }
}