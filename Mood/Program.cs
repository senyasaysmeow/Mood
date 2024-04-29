using System;
using System.Threading.Tasks;

namespace Mood
{
    class Program
    {
        static async Task Main(string[] args)
        {
            SpotifyClient sp = new SpotifyClient();
            
            var tracks = await sp.GetMoodPlaylist();
            foreach (var track in tracks)
            {
                Console.WriteLine($"{track.Name} by {track.Artist}");
            }
            var pl = await sp.CreatePlaylist("my");
            await sp.AddTracksToPlaylist(pl, tracks);
        }
    }
}