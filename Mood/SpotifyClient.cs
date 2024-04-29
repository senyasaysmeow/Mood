using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Mood.Models;
using Newtonsoft.Json;

namespace Mood
{
    public class SpotifyClient
    {
        private static string _ADD_TRACKS_TO_PLAYLIST_URL;
        
        public async Task<List<Track>> GetMoodPlaylist()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Constants.ACCESS_TOKEN);
            
            var topTracksUrl = Constants.TOP_TRACKS_URL;
            var topTracksResponse = await client.GetAsync(topTracksUrl);
            var topTracksString = await topTracksResponse.Content.ReadAsStringAsync();
            dynamic topTracksData = JsonConvert.DeserializeObject(topTracksString);

            var tracks = new List<Track>();

            foreach (var track in topTracksData.items)
            {
                var newTrack = new Track
                {
                    Name = track.name,
                    Artist = track.artists[0].name,
                    ID = track.id
                };
                tracks.Add(newTrack);
            }

            return tracks;
        }

        public async Task<string> CreatePlaylist(string playlistName)
        {
            Console.WriteLine(Constants.CREATE_PLAYLIST_URL);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Constants.ACCESS_TOKEN);
                var playlistData = new Dictionary<string, string>
                {
                    { "name", playlistName },
                    { "description", "New playlist description" },
                    { "public", "false" }
                };
                
                var jsonContent = JsonConvert.SerializeObject(playlistData);
                
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(Constants.CREATE_PLAYLIST_URL, content);
                var responseString = await response.Content.ReadAsStringAsync();
                dynamic responseData = JsonConvert.DeserializeObject(responseString);
                
                return responseData.id;
            }
        }
        
        public async Task AddTracksToPlaylist(string playlist_id, List<Track> tracks)
        {
            _ADD_TRACKS_TO_PLAYLIST_URL = $"https://api.spotify.com/v1/playlists/{playlist_id}/tracks";
            
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Constants.ACCESS_TOKEN);
                var trackUris = new List<string>();
                foreach (var track in tracks)
                {
                    trackUris.Add($"spotify:track:{track.ID}");
                }
                var requestData = new { uris = trackUris };
                var response = await client.PostAsync(_ADD_TRACKS_TO_PLAYLIST_URL, new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
            }
        }
    }
}