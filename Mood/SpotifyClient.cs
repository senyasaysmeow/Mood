using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Mood.Models;
using Newtonsoft.Json;

namespace Mood
{
    public class SpotifyClient
    {
        public string getLink()
        {
            var parameters = new Dictionary<string, string>
            {
                {"client_id", Constants.CLIENT_ID},
                {"response_type", "code"},
                {"redirect_uri", Constants.REDIRECT_URI},
                {"scope", string.Join(" ", Constants.SCOPES)}
            };

            var queryString = ToQueryString(parameters);

            return $"{Constants.AUTHORIZATION_ENDPOINT}?{queryString}";
        }

        public async Task AUTH()
        {
            var authorizationCode = await GetAuthorizationCodeAsync();
            var accessToken = await GetAccessTokenAsync(authorizationCode);

            Constants.ACCESS_TOKEN = accessToken;
        }
        
        public async Task<string> GetAuthorizationCodeAsync()
        {
            var authorizationUrl = getLink();
            
            var httpListener = new HttpListener();
            httpListener.Prefixes.Add(Constants.REDIRECT_URI + "/");
            httpListener.Start();

            var context = await httpListener.GetContextAsync();
            var authorizationCode = context.Request.QueryString["code"];
            
            var responseString = "<html><head><title>Authorization Successful</title></head><body><h1>Authorization Successful!</h1></body></html>";
            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            context.Response.ContentLength64 = buffer.Length;
            var responseOutput = context.Response.OutputStream;
            responseOutput.Write(buffer, 0, buffer.Length);
            responseOutput.Close();
            httpListener.Stop();

            return authorizationCode;
        }
        
        public async Task<string> GetAccessTokenAsync(string authorizationCode)
        {
            var httpClient = new HttpClient();

            var tokenRequestBody = new Dictionary<string, string>
            {
                {"client_id", Constants.CLIENT_ID},
                {"client_secret", Constants.CLIENT_SECRET},
                {"grant_type", "authorization_code"},
                {"redirect_uri", Constants.REDIRECT_URI},
                {"code", authorizationCode}
            };

            var tokenRequestContent = new FormUrlEncodedContent(tokenRequestBody);

            var tokenResponse = await httpClient.PostAsync(Constants.TOKEN_ENDPOINT, tokenRequestContent);
            var tokenString = await tokenResponse.Content.ReadAsStringAsync();
            dynamic tokenData = JsonConvert.DeserializeObject(tokenString);

            return tokenData.access_token;
        }
        
        public string ToQueryString(Dictionary<string, string> parameters)
        {
            var keyValuePairs = new List<string>();

            foreach (var parameter in parameters)
            {
                keyValuePairs.Add($"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value)}");
            }

            return string.Join("&", keyValuePairs);
        }
        
        public async Task<List<Track>> GetMoodPlaylist(float mood)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Constants.ACCESS_TOKEN);
                var topTracksUrl = "https://api.spotify.com/v1/playlists/1zL3yEfVKNkCfdLjqL38Zo/tracks";
                var topTracksResponse = await client.GetAsync(topTracksUrl);
                var topTracksString = await topTracksResponse.Content.ReadAsStringAsync();
                dynamic topTracksData = JsonConvert.DeserializeObject(topTracksString);

                var tracks = new List<Track>();

                foreach (var track in topTracksData.items)
                {
                    var audioFeaturesUrl = $"https://api.spotify.com/v1/audio-features/{track.track.id}";
                    var audioFeaturesResponse = await client.GetAsync(audioFeaturesUrl);
                    var audioFeaturesString = await audioFeaturesResponse.Content.ReadAsStringAsync();
                    dynamic audioFeaturesData = JsonConvert.DeserializeObject(audioFeaturesString);
                    
                    var newTrack = new Track
                    {
                        Name = track.track.name,
                        Artist = track.track.artists[0].name,
                        ID = track.track.id,
                        Valence = audioFeaturesData.valence
                    };

                    if (Math.Abs(newTrack.Valence - mood) < 0.05)
                    {
                        tracks.Add(newTrack);
                        Console.WriteLine($"+ {newTrack.Name} by {newTrack.Artist} {newTrack.Valence}");
                    }
                    else
                    {
                        Console.WriteLine($"- {newTrack.Name} by {newTrack.Artist} {newTrack.Valence}");
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