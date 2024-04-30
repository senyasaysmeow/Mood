using System;
using System.Threading.Tasks;

namespace Mood
{
    class Program
    {
        static async Task Main(string[] args)
        {
            SpotifyClient sp = new SpotifyClient();
            Console.Write("Enter your mood(0.0 -> 1.0): ");
            float mood = float.Parse(Console.ReadLine());
            var tracks = await sp.GetMoodPlaylist(mood);
            var pl = await sp.CreatePlaylist($"mood {mood}");
            Console.WriteLine($"https://open.spotify.com/playlist/{pl}");
            await sp.AddTracksToPlaylist(pl, tracks);
        }
    }
}