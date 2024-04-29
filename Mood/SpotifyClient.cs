using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Mood.Models;

namespace Mood
{
    public class SpotifyClient
    {
        private static string _CLIENT_ID;
        private static string _REDIRECT_URI;
        private static string _AUTH_URL;
        private static string _TOKEN_URL;

        public SpotifyClient()
        {
            _CLIENT_ID = Constants.CLIENT_ID;
            _REDIRECT_URI = Constants.REDIRECT_URI;
            _AUTH_URL = Constants.AUTH_URL;
            _TOKEN_URL = Constants.TOKEN_URL;
        }

        public async Task<List<Track>> GetTopTracks()
        {
            var codeVerifier = GenerateCodeVerifier();
            var codeChallenge = GenerateCodeChallenge(codeVerifier);

            var authParams = new Dictionary<string, string>
            {
                { "client_id", _CLIENT_ID },
                { "response_type", "code" },
                { "redirect_uri", _REDIRECT_URI },
                { "scope", "user-top-read" },
                { "code_challenge_method", "S256" },
                { "code_challenge", codeChallenge }
            };

            var authUrl = $"{_AUTH_URL}?{ToQueryString(authParams)}";
            Console.WriteLine("Please go to this URL and authorize the application: " + authUrl);

            var listener = new HttpListener();
            listener.Prefixes.Add(_REDIRECT_URI + "/");
            listener.Start();

            var context = await listener.GetContextAsync();
            var authorizationCode = context.Request.QueryString["code"];
            Console.WriteLine("Authorization code retrieved successfully: " + authorizationCode);

            listener.Stop();

            var tokenData = new Dictionary<string, string>
            {
                { "client_id", _CLIENT_ID },
                { "grant_type", "authorization_code" },
                { "code_verifier", codeVerifier },
                { "code", authorizationCode },
                { "redirect_uri", _REDIRECT_URI }
            };

            var client = new HttpClient();
            var response = await client.PostAsync(_TOKEN_URL, new FormUrlEncodedContent(tokenData));
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseData = Newtonsoft.Json.JsonConvert.DeserializeObject(responseString);

            var accessToken = responseData.access_token;

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

            var topTracksUrl = "https://api.spotify.com/v1/me/top/tracks?limit=50";
            var topTracksResponse = await client.GetAsync(topTracksUrl);
            var topTracksString = await topTracksResponse.Content.ReadAsStringAsync();
            dynamic topTracksData = Newtonsoft.Json.JsonConvert.DeserializeObject(topTracksString);

            var tracks = new List<Track>();

            foreach (var track in topTracksData.items)
            {
                var tracksFeaturesUrl = $"https://api.spotify.com/v1/audio-features/{track.id}";
                var tracksFeaturesResponse = await client.GetAsync(tracksFeaturesUrl);
                var tracksFeaturesString = await tracksFeaturesResponse.Content.ReadAsStringAsync();
                dynamic tracksFeaturesData = Newtonsoft.Json.JsonConvert.DeserializeObject(tracksFeaturesString);
                var newTrack = new Track
                {
                    Name = track.name,
                    Artist = track.artists[0].name,
                    ID = track.id,
                    Valence = tracksFeaturesData.valence
                };
                tracks.Add(newTrack);
            }

            return tracks;
        }

        static string ToQueryString(Dictionary<string, string> dict)
        {
            var array = new List<string>();
            foreach (var kvp in dict)
            {
                array.Add($"{kvp.Key}={kvp.Value}");
            }
            return string.Join("&", array);
        }

        static string GenerateCodeVerifier()
        {
            var random = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);
            }
            return Base64UrlEncode(random);
        }

        static string GenerateCodeChallenge(string codeVerifier)
        {
            using (var sha256 = SHA256.Create())
            {
                var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                return Base64UrlEncode(challengeBytes);
            }
        }

        static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }
    }
}



