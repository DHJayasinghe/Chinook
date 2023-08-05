using Chinook.Models;
using Chinook.Services;
using Chinook.Shared.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using Chinook.ClientModels;

namespace Chinook.Pages;

public partial class ArtistPage
{
    [Parameter] public long ArtistId { get; set; }
    [CascadingParameter] private Task<AuthenticationState> authenticationState { get; set; }

    [Inject] TrackService TrackService { get; set; }
    [Inject] PlaylistService PlaylistService { get; set; }

    private Modal PlaylistDialog { get; set; }
    private Artist Artist;
    private List<PlaylistTrack> Tracks;
    private PlaylistTrack SelectedTrack;
    private string InfoMessage;
    private bool SuccessMessage = true;
    private string CurrentUserId;
    private List<ClientModels.Playlist> PlayLists;

    private long SelectedPlayListId = 0;
    private string NewPlaylistName = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await InvokeAsync(StateHasChanged);
        CurrentUserId = await GetUserId();

        Artist = TrackService.GetArtist(ArtistId);
        Tracks = TrackService.GetByArtistWithUserFavorite(ArtistId, CurrentUserId);
        PlayLists = PlaylistService.GetByUser(CurrentUserId);
    }

    private async Task<string> GetUserId()
    {
        var user = (await authenticationState).User;
        var userId = user.FindFirst(u => u.Type.Contains(ClaimTypes.NameIdentifier))?.Value;
        return userId;
    }

    private void FavoriteTrack(long trackId)
    {
        var track = Tracks.First(t => t.TrackId == trackId);
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
        var track = Tracks.First(t => t.TrackId == trackId);
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

    private void DisplayAddedToPlaylistMsg()
    {
        var selectedPlayListName = PlayLists.First(playlist => playlist.Id == SelectedPlayListId).Name;
        InfoMessage = $"Track {Artist.Name} - {SelectedTrack.AlbumTitle} - {SelectedTrack.TrackName} added to playlist {selectedPlayListName}.";
        PlaylistDialog.Close();
    }

    private void OpenPlaylistDialog(long trackId)
    {
        CloseInfoMessage();
        SelectedTrack = Tracks.First(t => t.TrackId == trackId);
        PlaylistDialog.Open();
    }

    private async Task AddTrackToPlaylist()
    {
        CloseInfoMessage();
        if (NewPlayListNameProvided)
        {
            SavePlaylistAndAddToTheExistingList();
            NewPlaylistName = string.Empty;
        }
        TrackService.AddToPlaylist(SelectedPlayListId, SelectedTrack.TrackId);
        DisplayAddedToPlaylistMsg();
    }

    private void SavePlaylistAndAddToTheExistingList()
    {
        var newPlaylistId = PlaylistService.Add(NewPlaylistName, CurrentUserId);
        PlayLists.Add(new ClientModels.Playlist { Id = newPlaylistId, Name = NewPlaylistName });
        SelectedPlayListId = newPlaylistId;
        NewPlaylistName = string.Empty;
    }

    private bool NewPlayListNameProvided => !string.IsNullOrEmpty(NewPlaylistName);

    private void CloseInfoMessage()
    {
        InfoMessage = "";
        SuccessMessage = true;
    }
}