using Newtonsoft.Json;
using PokemonApp.Models;

namespace PokemonApp.Services
{
    public class PokemonService : IPokemonService
    {
        private readonly HttpClient _httpClient;

        public PokemonService(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<List<PokemonModel>> GetPokemonList(int limit, int offset)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"pokemon?limit={limit}&offset={offset}");
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            PokemonResponse? result = JsonConvert.DeserializeObject<PokemonResponse>(json);
            return result?.Results ?? [];//posiblenull
        }

        public async Task<PokemonSpeciesModel> GetPokemonSpecies(string pokemonUrl)
        {
            string pokemonId = pokemonUrl.Split('/').Reverse().Skip(1).First(); // Extrae el ID del Pokémon
            HttpResponseMessage response = await _httpClient.GetAsync($"pokemon-species/{pokemonId}");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            PokemonSpeciesModel? speciesData = JsonConvert.DeserializeObject<PokemonSpeciesModel>(json);

            if (speciesData != null)
            {
                return new PokemonSpeciesModel
                {
                    Name = speciesData.Name,
                    Id = int.Parse(pokemonId)
                };
            }
            else
            {
                return new PokemonSpeciesModel
                {
                    Name = "Desconocido", // defecto
                    Id = int.Parse(pokemonId)
                };
            }
        }
    }
}
