using Chinook.Models;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Chinook.Services;

namespace Chinook.Pages;

public partial class Index
{
    private List<Artist> Artists;
    private string searchedArtistName = string.Empty;
    [Inject] IArtistService ArtistService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await InvokeAsync(StateHasChanged);
        Artists = GetArtists();
    }

    public List<Artist> GetArtists(string artistName = null) => ArtistService.Search(artistName);

    public List<Album> GetAlbumsForArtist(int artistId) => ArtistService.GetAlbums(artistId);

    private void OnKeyPress(KeyboardEventArgs e)
    {
        if (e.Key != "Enter") return;

        Artists = GetArtists(searchedArtistName);
    }
}
