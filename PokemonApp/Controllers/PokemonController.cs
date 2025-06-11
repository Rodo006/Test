using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using PokemonApp.Services;
using PokemonApp.Models;
using MimeKit;
using MailKit.Net.Smtp;

namespace PokemonApp.Controllers
{
    public class PokemonController(IPokemonService pokemonService) : Controller
    {
        public async Task<IActionResult> Index(string nameFilter = "", int speciesFilter = 0, int page = 1)
        {
            List<PokemonModel> allPokemons = await pokemonService.GetPokemonList(50, 0);//50 pkmn

            foreach (PokemonModel pokemon in allPokemons)
            {
                PokemonSpeciesModel speciesData = await pokemonService.GetPokemonSpecies(pokemon.Url);
                pokemon.Species = speciesData.Name;
                pokemon.SpeciesId = speciesData.Id;

                string pokemonId = pokemon.Url.Split('/').Reverse().Skip(1).First();
                pokemon.Id = int.Parse(pokemonId);
            }

            //by name & specie
            List<PokemonModel> filteredPokemons = allPokemons.Where(p =>
                (string.IsNullOrEmpty(nameFilter) || p.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase)) &&
                (speciesFilter == 0 || p.SpeciesId == speciesFilter)).ToList();

            //paginacion
            ViewBag.TotalPages = (int)Math.Ceiling((double)filteredPokemons.Count / 5);
            ViewBag.CurrentPage = page;

            List<PokemonModel> paginatedPokemons = filteredPokemons.Skip((page - 1) * 5).Take(5).ToList();

            //FFill
            ViewBag.SpeciesList = allPokemons.Select(s => new PokemonSpeciesModel
            {
                Id = s.Id,
                Name = s.Name
            }).ToList();

            return View(paginatedPokemons);
        }

        public async Task<IActionResult> ExportToExcel()
        {
            List<PokemonModel> pokemons = await pokemonService.GetPokemonList(50, 0);//50 pkmn

            using XLWorkbook workbook = CreateExcel(pokemons);

            //wrtng
            using MemoryStream stream = new();
            workbook.SaveAs(stream);
            byte[] content = stream.ToArray();

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Pokemon.xlsx");
        }

        public async Task<IActionResult> SendEmail()
        {
            //mejor un settings, pero es ejemplo :b (borrar comentario)
            string smtpUser = string.Empty;
            string smtpPassword = string.Empty;
            string smtpServer = string.Empty;
            string toName = string.Empty;
            string toEmail = string.Empty;

            //valid conf
            if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPassword) || string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(toName) || string.IsNullOrEmpty(toEmail))
            {
                TempData["EmailWarning"] = "⚠️ Advertencia: No se ha configurado correctamente el correo. Por favor, actualiza los valores SMTP.";
                return RedirectToAction("Index");
            }

            List<PokemonModel> pokemons = await pokemonService.GetPokemonList(50, 0);//50 pkmn

            using XLWorkbook workbook = CreateExcel(pokemons);

            using MemoryStream stream = new();
            workbook.SaveAs(stream);
            byte[] fileBytes = stream.ToArray();

            MimeMessage message = new();
            message.From.Add(new MailboxAddress("Pokémon App", smtpUser));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = "Lista de Pokémon en Excel";

            TextPart body = new("plain") { Text = "Adjunto el archivo Excel con la lista de Pokémon." };
            MimePart attachment = new("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                Content = new MimeContent(new MemoryStream(fileBytes)),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = "Pokemon.xlsx"
            };

            Multipart multipart = new("mixed") { body, attachment };
            message.Body = multipart;

            //snd
            using (SmtpClient client = new())
            {
                client.Connect(smtpServer, 587, false);
                client.Authenticate(smtpUser, smtpPassword);
                client.Send(message);
                client.Disconnect(true);
            }

            TempData["EmailSuccess"] = "✅ Correo enviado correctamente.";
            return RedirectToAction("Index");
        }

        private static XLWorkbook CreateExcel(List<PokemonModel> pokemons)
        {
            XLWorkbook workbook = new();
            IXLWorksheet worksheet = workbook.Worksheets.Add("Pokémon");

            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Nombre";

            int row = 2;
            foreach (var pokemon in pokemons.Select((p, i) => new { Index = i + 1, Data = p }))
            {
                worksheet.Cell(row, 1).Value = pokemon.Index;
                worksheet.Cell(row, 2).Value = pokemon.Data.Name;
                row++;
            }

            IXLRange range = worksheet.Range(1, 1, row - 1, 2);
            IXLTable table = range.CreateTable();
            table.Theme = XLTableTheme.TableStyleMedium9;
            worksheet.Columns().AdjustToContents();

            return workbook;
        }
    }
}