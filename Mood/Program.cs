﻿using System;
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
            SpotifyClient sp = new SpotifyClient();
            var client = new TelegramBotClient("6071279711:AAF75qte3X4rQnu2MRr-usON5vYSwkrjROo");
            client.StartReceiving((botClient, update, token) => Update(botClient, update, token, sp), Error);
            
            Console.ReadLine();
        }

        async static Task Update(ITelegramBotClient botClient, Update update, CancellationToken token, SpotifyClient sp)
        {
            var message = update.Message;
            if (message.Text != null)
            {
                if (message.Text.ToLower() == "/start")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"To authenticate with Spotify click the link below:\n{sp.getLink()}");
                    await sp.AUTH();
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Authorization Successful!");
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Now describe your mood on a scale from 0.0 to 1.0 :)");
                }
                else if (float.TryParse(message.Text, out float mood) && mood >= 0.0 && mood <= 1.0)
                {
                    var sentMassage = await botClient.SendTextMessageAsync(message.Chat.Id, $"Creating your playlist...");
                    var tracks = await sp.GetMoodPlaylist(mood);
                    var pl = await sp.CreatePlaylist($"mood {mood}");
                    await sp.AddTracksToPlaylist(pl, tracks);
                    await botClient.DeleteMessageAsync(message.Chat.Id, sentMassage.MessageId);
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Playlist created based on your mood:\nhttps://open.spotify.com/playlist/{pl}");
                }
                else if (message.Text.ToLower() == "/restart")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"If you want to create another mood playlist just describe your mood again on a scale from 0.0 to 1.0 :)");
                }
            }
        }
        
        private static Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            throw new NotImplementedException();
        }
    }
}