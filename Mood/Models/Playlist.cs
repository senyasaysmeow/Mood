namespace Mood.Models
{
    public class Playlist
    {
        public string _Name { get; set; }
        public string _Description { get; set; }
        public bool _isPublic { get; set; }

        public Playlist(string Name, string Description, bool isPublic)
        {
            _Name = Name;
            _Description = Description;
            _isPublic = isPublic;
        }
    }
}