using PokemonApp.Models;

namespace PokemonApp.Services
{
    public interface IPokemonService
    {
        Task<List<PokemonModel>> GetPokemonList(int limit, int offset);

        Task<PokemonSpeciesModel> GetPokemonSpecies(string pokemonUrl);
    }
}