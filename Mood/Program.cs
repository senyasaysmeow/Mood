using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mood
{
    class Program
    {
        static async Task Main(string[] args)
        {
            SpotifyClient sp = new SpotifyClient();
            Dictionary<string, string> token = await sp.Authorize();
            var tracks = await sp.GetTopTracks(token);
            Console.WriteLine("Top Tracks:");
            foreach (var track in tracks)
            {
                Console.WriteLine($"{track.Name} by {track.Artist}  {track.Valence}");
            }
        }
    }
}