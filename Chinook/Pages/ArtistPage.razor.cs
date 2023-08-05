using Chinook.Models;
using Chinook.Services;
using Chinook.Shared.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Chinook.Pages
{
    public partial class ArtistPage
    {
        [Parameter] public long ArtistId { get; set; }
        [CascadingParameter] private Task<AuthenticationState> authenticationState { get; set; }
        [Inject] IDbContextFactory<ChinookContext> DbFactory { get; set; }
        [Inject] PlaylistService playlistService { get; set; }
        private Modal PlaylistDialog { get; set; }

        private Artist Artist;
        private List<ClientModels.PlaylistTrack> Tracks;
        private ChinookContext DbContext;
        private ClientModels.PlaylistTrack SelectedTrack;
        private string InfoMessage;
        private string CurrentUserId;
        private List<ClientModels.Playlist> PlayLists;
        private long SelectedPlayListId = 0;
        private string NewPlaylistName = string.Empty;
        private const string myFavoriteTrackTitle = "My favorite tracks";

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(StateHasChanged);
            CurrentUserId = await GetUserId();
            DbContext = await DbFactory.CreateDbContextAsync();

            Artist = DbContext.Artists.SingleOrDefault(a => a.ArtistId == ArtistId);

            Tracks = DbContext.Tracks.Where(a => a.Album.ArtistId == ArtistId)
                .Include(a => a.Album)
                .Select(t => new ClientModels.PlaylistTrack()
                {
                    AlbumTitle = (t.Album == null ? "-" : t.Album.Title),
                    TrackId = t.TrackId,
                    TrackName = t.Name,
                    IsFavorite = t.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == CurrentUserId && up.Playlist.Name == myFavoriteTrackTitle)).Any()
                })
                .ToList();

            PlayLists = await GetCurrentUserPlayListAsync();
        }

        private async Task<List<ClientModels.Playlist>> GetCurrentUserPlayListAsync()
        {
            return DbContext.UserPlaylists.Include(up => up.Playlist)
                .Where(up => up.UserId == CurrentUserId)
                .Select(up => new ClientModels.Playlist { Id = up.PlaylistId, Name = up.Playlist.Name })
                .ToList();
        }

        private async Task<string> GetUserId()
        {
            var user = (await authenticationState).User;
            var userId = user.FindFirst(u => u.Type.Contains(ClaimTypes.NameIdentifier))?.Value;
            return userId;
        }

        private void FavoriteTrack(long trackId)
        {
            var track = Tracks.FirstOrDefault(t => t.TrackId == trackId);
            InfoMessage = $"Track {track.ArtistName} - {track.AlbumTitle} - {track.TrackName} added to playlist Favorites.";

            var myFavoritePlaylist = DbContext.UserPlaylists
                .Include(up => up.Playlist)
                .SingleOrDefault(up => up.UserId == CurrentUserId && up.Playlist.Name == myFavoriteTrackTitle)
                .Playlist;

            var t = DbContext.Tracks.Single(tr => tr.TrackId == trackId);
            myFavoritePlaylist.Tracks.Add(t);
            var success = DbContext.SaveChanges() > 0;
            if (success) track.IsFavorite = true;
        }

        private void UnfavoriteTrack(long trackId)
        {
            var track = Tracks.FirstOrDefault(t => t.TrackId == trackId);
            InfoMessage = $"Track {track.ArtistName} - {track.AlbumTitle} - {track.TrackName} removed from playlist Favorites.";

            var myFavoritePlaylist = DbContext.UserPlaylists
                .Include(up => up.Playlist)
                    .ThenInclude(p => p.Tracks)
                .SingleOrDefault(up => up.UserId == CurrentUserId && up.Playlist.Name == myFavoriteTrackTitle)
                ?.Playlist;

            var t = DbContext.Tracks.Single(tr => tr.TrackId == trackId);
            myFavoritePlaylist.Tracks.Remove(t);
            var success = DbContext.SaveChanges() > 0;

            if (success) track.IsFavorite = false;
        }

        private void OpenPlaylistDialog(long trackId)
        {
            CloseInfoMessage();
            SelectedTrack = Tracks.FirstOrDefault(t => t.TrackId == trackId);
            PlaylistDialog.Open();
        }

        private async Task AddTrackToPlaylist()
        {
            CloseInfoMessage();
            if (!string.IsNullOrEmpty(NewPlaylistName))
            {
                var newPlayList = new Models.Playlist()
                {
                    Name = NewPlaylistName,
                };
                var userPlayList = new Models.UserPlaylist()
                {
                    Playlist = newPlayList,
                    UserId = CurrentUserId
                };
                DbContext.UserPlaylists.Add(userPlayList);
                DbContext.SaveChanges();

                playlistService.AddItem();
                NewPlaylistName = string.Empty;

                SelectedPlayListId = newPlayList.PlaylistId;
                PlayLists.Add(new ClientModels.Playlist { Id = newPlayList.PlaylistId, Name = newPlayList.Name });
            }
            var selectedPlayListName = PlayLists.First(playlist => playlist.Id == SelectedPlayListId).Name;
            InfoMessage = $"Track {Artist.Name} - {SelectedTrack.AlbumTitle} - {SelectedTrack.TrackName} added to playlist {selectedPlayListName}.";
            PlaylistDialog.Close();


            // enable lazy loading here
            var playlist = DbContext.Playlists
                .Include(playlist => playlist.Tracks)
                .First(playlist => playlist.PlaylistId == SelectedPlayListId);
            if (playlist.Tracks.Any(pt => pt.TrackId == SelectedTrack.TrackId)) return;

            var track = DbContext.Tracks.Find(SelectedTrack.TrackId);
            playlist.Tracks.Add(track);
            DbContext.SaveChanges();
        }

        private void CloseInfoMessage()
        {
            InfoMessage = "";
        }
    }
}
