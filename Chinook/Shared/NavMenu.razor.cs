using Chinook.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Chinook.ClientModels;
using System.Security.Claims;

namespace Chinook.Shared;

public partial class NavMenu
{
    [CascadingParameter] private Task<AuthenticationState> authenticationState { get; set; }
    [CascadingParameter] private PlaylistService dataService { get; set; }
    [Inject] IDbContextFactory<ChinookContext> DbFactory { get; set; }
    private string CurrentUserId;
    private List<Playlist> PlayLists;
    private ChinookContext DbContext;

    protected override async Task OnInitializedAsync()
    {
        await InvokeAsync(StateHasChanged);
        CurrentUserId = await GetUserId();
        DbContext = await DbFactory.CreateDbContextAsync();

        PlayLists = await GetCurrentUserPlayListAsync();
    }

    protected override void OnParametersSet()
    {
        dataService.OnDataUpdated += OnDataUpdated;
    }


    private async void OnDataUpdated(object sender, EventArgs e)
    {
        PlayLists = GetCurrentUserPlayListAsync().Result;
        await InvokeAsync(StateHasChanged);
    }

    private async Task<List<Playlist>> GetCurrentUserPlayListAsync()
    {
        if (!DbContext.UserPlaylists.Any(up => up.UserId == CurrentUserId && up.Playlist.Name == "My favorite tracks"))
        {
            var myFavoritePlaylist = new Models.Playlist() { Name = "My favorite tracks" };
            var userPlaylist = new Models.UserPlaylist() { Playlist = myFavoritePlaylist, UserId = CurrentUserId };
            DbContext.UserPlaylists.Add(userPlaylist);
            DbContext.SaveChanges();
        }

        return DbContext.UserPlaylists.Include(up => up.Playlist)
            .Where(up => up.UserId == CurrentUserId)
            .OrderBy(up => up.Playlist.Name != "My favorite tracks")
            .Select(up => new Playlist { Name = up.Playlist.Name })
            .ToList();
    }

    private async Task<string> GetUserId()
    {
        var user = (await authenticationState).User;
        var userId = user.FindFirst(u => u.Type.Contains(ClaimTypes.NameIdentifier))?.Value;
        return userId;
    }

    private bool collapseNavMenu = true;

    private string? NavMenuCssClass => collapseNavMenu ? "collapse" : null;

    private void ToggleNavMenu()
    {
        collapseNavMenu = !collapseNavMenu;
    }
}
