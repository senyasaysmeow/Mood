using System;
using System.Threading.Tasks;

namespace Mood
{
    class Program
    {
        static async Task Main(string[] args)
        {
            SpotifyClient sp = new SpotifyClient();
            var tracks = await sp.GetTopTracks();
            
            Console.WriteLine("Top Tracks:");
            foreach (var track in tracks)
            {
                Console.WriteLine($"{track.Name} by {string.Join(", ", track.Artist)} {string.Join(", ", track.Valence)}");
            }
        }
    }
}