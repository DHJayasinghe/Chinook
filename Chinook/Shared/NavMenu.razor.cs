﻿using Chinook.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Chinook.ClientModels;
using System.Security.Claims;

namespace Chinook.Shared;

public partial class NavMenu
{
    [CascadingParameter] private Task<AuthenticationState> authenticationState { get; set; }
    [CascadingParameter] private IPlaylistService dataService { get; set; }

    [Inject] IPlaylistService PlaylistService { get; set; }
    private string CurrentUserId;
    private List<Playlist> PlayLists;

    protected override async Task OnInitializedAsync()
    {
        await InvokeAsync(StateHasChanged);
        CurrentUserId = await GetUserId();

        if (CurrentUserId is not null)
        {
            PlaylistService.AddFavoritePlaylistIfNotExist(CurrentUserId);
            PlayLists = GetCurrentUserPlayList();
        }
    }

    protected override void OnParametersSet()
    {
        dataService.PlayListAddedEvent += RefreshPlaylistAsync;
    }


    private async void RefreshPlaylistAsync(object sender, EventArgs e)
    {
        PlayLists = GetCurrentUserPlayList();
        await InvokeAsync(StateHasChanged);
    }

    private List<Playlist> GetCurrentUserPlayList()
    {
        return PlaylistService.GetByUser(CurrentUserId)
            .OrderBy(up => up.Name != IPlaylistService.FAVORITE_PLAYLIST_NAME)
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
