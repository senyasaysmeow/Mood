using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Mood
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // var client = new TelegramBotClient("7015725647:AAFLN9deP1QITD5dtcTwLxWfsW31d76pcpM");
            // client.StartReceiving(Update, Error);
            // Console.ReadLine();

            SpotifyClient sp = new SpotifyClient();
            await sp.AUTH();
            Console.Write("Enter your mood(0.0 -> 1.0): ");
            float mood = float.Parse(Console.ReadLine());
            var tracks = await sp.GetMoodPlaylist(mood);
            var pl = await sp.CreatePlaylist($"mood {mood}");
            Console.WriteLine($"https://open.spotify.com/playlist/{pl}");
            await sp.AddTracksToPlaylist(pl, tracks);
        }

        async static Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            var message = update.Message;
            if (message.Text != null)
            {
                if (message.Text.ToLower().Contains("hello"))
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "hi!");
                    return;
                }
            }
        }
        
        private static Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            throw new NotImplementedException();
        }
    }
}