using Chinook.Models;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Pages
{
    public partial class Index
    {
        private List<Artist> Artists;
        private string searchedArtistName = string.Empty;
        [Inject] IDbContextFactory<ChinookContext> DbFactory { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(StateHasChanged);
            Artists = await GetArtists();
        }

        public async Task<List<Artist>> GetArtists(string artistName = null)
        {
            var dbContext = await DbFactory.CreateDbContextAsync();
            //var users = dbContext.Users.Include(a => a.UserPlaylists).ToList();

            return string.IsNullOrEmpty(artistName) ? dbContext.Artists.ToList() : dbContext.Artists.Where(artist => artist.Name.Contains(artistName)).ToList();
        }

        public async Task<List<Album>> GetAlbumsForArtist(int artistId)
        {
            var dbContext = await DbFactory.CreateDbContextAsync();
            return dbContext.Albums.Where(a => a.ArtistId == artistId).ToList();
        }

        private async Task OnKeyPress(KeyboardEventArgs e)
        {
            if (e.Key != "Enter") return;

            Artists = await GetArtists(searchedArtistName);
        }
    }
}
