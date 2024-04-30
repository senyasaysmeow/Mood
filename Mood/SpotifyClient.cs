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
        public async Task<List<Track>> GetMoodPlaylist(float mood)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Constants.ACCESS_TOKEN);
                var topTracksUrl = "https://api.spotify.com/v1/me/top/tracks?limit=20";
                var topTracksResponse = await client.GetAsync(topTracksUrl);
                var topTracksString = await topTracksResponse.Content.ReadAsStringAsync();
                dynamic topTracksData = JsonConvert.DeserializeObject(topTracksString);

                var tracks = new List<Track>();

                foreach (var track in topTracksData.items)
                {
                    var audioFeaturesUrl = $"https://api.spotify.com/v1/audio-features/{track.id}";
                    var audioFeaturesResponse = await client.GetAsync(audioFeaturesUrl);
                    var audioFeaturesString = await audioFeaturesResponse.Content.ReadAsStringAsync();
                    dynamic audioFeaturesData = JsonConvert.DeserializeObject(audioFeaturesString);
                    
                    var newTrack = new Track
                    {
                        Name = track.name,
                        Artist = track.artists[0].name,
                        ID = track.id,
                        Valence = audioFeaturesData.valence
                    };

                    if (mood >= 0.5)
                    {
                        if (newTrack.Valence >= 0.5)
                        {
                            tracks.Add(newTrack);
                            Console.WriteLine($"+ {newTrack.Name} by {newTrack.Artist} {newTrack.Valence}");
                        }
                        else
                        {
                            Console.WriteLine($"- {newTrack.Name} by {newTrack.Artist} {newTrack.Valence}");
                        }
                    }
                    else
                    {
                        if (newTrack.Valence <= 0.5)
                        {
                            tracks.Add(newTrack);
                            Console.WriteLine($"+ {newTrack.Name} by {newTrack.Artist} {newTrack.Valence}");
                        }
                        else
                        {
                            Console.WriteLine($"- {newTrack.Name} by {newTrack.Artist} {newTrack.Valence}");
                        }
                    }
                    
                }

                return tracks;
            }
        }

        public async Task<string> CreatePlaylist(string playlistName)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Constants.ACCESS_TOKEN);
                var userInfoUrl = "https://api.spotify.com/v1/me/";
                var userInfoResponse = await client.GetAsync(userInfoUrl);
                var userInfoString = await userInfoResponse.Content.ReadAsStringAsync();
                dynamic userInfoData = JsonConvert.DeserializeObject(userInfoString);
                
                var playlistData = new Dictionary<string, string>
                {
                    { "name", playlistName },
                    { "description", "New playlist description" },
                    { "public", "false" }
                };
                
                var jsonContent = JsonConvert.SerializeObject(playlistData);
                
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"https://api.spotify.com/v1/users/{userInfoData.id}/playlists", content);
                var responseString = await response.Content.ReadAsStringAsync();
                dynamic responseData = JsonConvert.DeserializeObject(responseString);
                
                return responseData.id;
            }
        }
        
        public async Task AddTracksToPlaylist(string playlist_id, List<Track> tracks)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Constants.ACCESS_TOKEN);
                var trackUris = new List<string>();
                foreach (var track in tracks)
                {
                    trackUris.Add($"spotify:track:{track.ID}");
                }
                var requestData = new { uris = trackUris };
                var response = await client.PostAsync($"https://api.spotify.com/v1/playlists/{playlist_id}/tracks", new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
            }
        }
    }
}