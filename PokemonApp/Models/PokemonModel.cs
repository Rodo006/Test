namespace PokemonApp.Models
{
    public class PokemonModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public string Species { get; set; } = string.Empty;

        public int SpeciesId { get; set; }
    }
}