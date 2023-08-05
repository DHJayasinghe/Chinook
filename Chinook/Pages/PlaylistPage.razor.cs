using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using Chinook.Services;
using Chinook.ClientModels;

namespace Chinook.Pages;

public partial class PlaylistPage
{
    [Parameter] public long PlaylistId { get; set; }
    [Inject] TrackService TrackService { get; set; }
    [Inject] PlaylistService PlaylistService { get; set; }

    [CascadingParameter] private Task<AuthenticationState> authenticationState { get; set; }

    private Playlist Playlist;
    private string CurrentUserId;
    private string InfoMessage;
    private bool SuccessMessage = true;
    private long _currentPlayListId=0;

    protected override async Task OnInitializedAsync()
    {
        CurrentUserId = await GetUserId();

        await InvokeAsync(StateHasChanged);
    }

    protected override void OnParametersSet()
    {
        if (_currentPlayListId != PlaylistId)
        {
            _currentPlayListId = PlaylistId;

            Playlist = PlaylistService.Get(PlaylistId);
            Playlist.Tracks = TrackService.GetByPlaylistWithUserFavorite(PlaylistId, CurrentUserId);
        }
    }

    private async Task<string> GetUserId()
    {
        var user = (await authenticationState).User;
        var userId = user.FindFirst(u => u.Type.Contains(ClaimTypes.NameIdentifier))?.Value;
        return userId;
    }

    private void FavoriteTrack(long trackId)
    {
        var track = Playlist.Tracks.First(t => t.TrackId == trackId);
        var success = PlaylistService.AddToFavorite(CurrentUserId, trackId);

        if (!success)
        {
            DisplayFailedToAddToFavoriteMsg(track);
            return;
        }

        track.IsFavorite = true;
        DisplayAddedToFavoriteMsg(track);
    }

    private void UnfavoriteTrack(long trackId)
    {
        var track = Playlist.Tracks.First(t => t.TrackId == trackId);
        var success = PlaylistService.RemoveFromFavorite(CurrentUserId, trackId);

        if (!success)
        {
            DisplayFailedToRemoveFromFavoriteMsg(track);
            return;
        }

        track.IsFavorite = false;
        DisplayRemovedFromFavoriteMsg(track);
    }

    private void DisplayRemovedFromFavoriteMsg(PlaylistTrack track)
    {
        InfoMessage = $"Track {track.ArtistName} - {track.AlbumTitle} - {track.TrackName} removed from playlist Favorites.";
        SuccessMessage = true;
    }

    private void DisplayFailedToRemoveFromFavoriteMsg(PlaylistTrack track)
    {
        InfoMessage = $"Track {track.ArtistName} - {track.AlbumTitle} - {track.TrackName} failed to remove from playlist Favorites.";
        SuccessMessage = false;
    }
    private void DisplayAddedToFavoriteMsg(PlaylistTrack track)
    {
        InfoMessage = $"Track {track.ArtistName} - {track.AlbumTitle} - {track.TrackName} added to playlist Favorites.";
        SuccessMessage = true;
    }

    private void DisplayFailedToAddToFavoriteMsg(PlaylistTrack track)
    {
        InfoMessage = $"Track {track.ArtistName} - {track.AlbumTitle} - {track.TrackName} failed to added to playlist Favorites.";
        SuccessMessage = false;
    }

    private void RemoveTrack(long trackId)
    {
        CloseInfoMessage();

    }

    private void CloseInfoMessage()
    {
        InfoMessage = "";
        SuccessMessage = true;
    }
}
