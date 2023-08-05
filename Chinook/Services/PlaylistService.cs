using Chinook.Models;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Services;

public class PlaylistService
{
    public event EventHandler PlayListAddedEvent;

    private readonly ChinookContext _dbContext;
    public const string FAVORITE_PLAYLIST_NAME = "My favorite tracks";

    public PlaylistService(ChinookContext dbContext) => _dbContext = dbContext;

    public long Add(string name, string userId)
    {
        var newPlayList = new Playlist()
        {
            Name = name,
        };
        var userPlayList = new UserPlaylist()
        {
            Playlist = newPlayList,
            UserId = userId
        };
        _dbContext.UserPlaylists.Add(userPlayList);
        _dbContext.SaveChanges();
        TriggerPlaylistAddedEvent();

        return newPlayList.PlaylistId;
    }

    protected virtual void TriggerPlaylistAddedEvent()
    {
        // Check if there are any subscribers to the event
        EventHandler handler = PlayListAddedEvent;
        if (handler != null)
        {
            var args = EventArgs.Empty;
            Task.Run(() => handler(this, args));
        }
    }

    public bool AddToFavorite(string userId, long trackId)
    {
        var favoritePlaylist = _dbContext.UserPlaylists
            .Include(up => up.Playlist)
            .Single(up => up.UserId == userId && up.Playlist.Name == FAVORITE_PLAYLIST_NAME)
        .Playlist;
        var track = _dbContext.Tracks.Single(tr => tr.TrackId == trackId);
        favoritePlaylist.Tracks.Add(track);
        return _dbContext.SaveChanges() > 0;
    }

    public bool RemoveFromFavorite(string userId, long trackId)
    {
        var favoritePlaylist = _dbContext.UserPlaylists
            .Include(up => up.Playlist)
                .ThenInclude(p => p.Tracks)
            .Single(up => up.UserId == userId && up.Playlist.Name == FAVORITE_PLAYLIST_NAME)
        .Playlist;

        var track = _dbContext.Tracks.Single(tr => tr.TrackId == trackId);
        favoritePlaylist.Tracks.Remove(track);
        return _dbContext.SaveChanges() > 0;
    }

    public List<ClientModels.Playlist> GetByUser(string userId) =>
        _dbContext.UserPlaylists.Include(up => up.Playlist)
            .Where(up => up.UserId == userId)
            .Select(up => new ClientModels.Playlist { Id = up.PlaylistId, Name = up.Playlist.Name })
            .ToList();
}