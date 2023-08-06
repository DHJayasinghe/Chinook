using Chinook.ClientModels;
using Chinook.Models;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Services;

public class TrackService
{
    private readonly ChinookContext _dbContext;

    public TrackService(IDbContextFactory<ChinookContext> dbContext)
    {
        _dbContext = dbContext.CreateDbContext();
    }

    public List<ClientModels.PlaylistTrack> GetByArtistWithUserFavorite(long artistId, string requestedUserId) => artistId > 0
        ? _dbContext.Tracks.Where(a => a.Album.ArtistId == artistId)
            .Include(a => a.Album)
             .Select(t => new PlaylistTrack()
             {
                 AlbumTitle = (t.Album == null ? "-" : t.Album.Title),
                 TrackId = t.TrackId,
                 TrackName = t.Name,
                 IsFavorite = t.Playlists
                        .Where(p => p.UserPlaylists
                            .Any(up => up.UserId == requestedUserId && up.Playlist.Name == PlaylistService.FAVORITE_PLAYLIST_NAME))
                        .Any()
             })
            .ToList()
        : default;

    public List<PlaylistTrack> GetByPlaylistWithUserFavorite(long playlistId, string requestedUserId) => playlistId > 0
        ? _dbContext.Playlists
            .Include(a => a.Tracks)
                .ThenInclude(a => a.Album)
                    .ThenInclude(a => a.Artist)
            .Where(p => p.PlaylistId == playlistId)
            .SelectMany(p => p.Tracks)
            .Select(t => new PlaylistTrack()
            {
                AlbumTitle = t.Album.Title,
                ArtistName = t.Album.Artist.Name,
                TrackId = t.TrackId,
                TrackName = t.Name,
                IsFavorite = t.Playlists
                        .Where(p => p.UserPlaylists
                            .Any(up => up.UserId == requestedUserId && up.Playlist.Name == PlaylistService.FAVORITE_PLAYLIST_NAME))
                        .Any()
            }).ToList()
       : default;

    public Artist GetArtist(long artistId) => artistId > 0 ? _dbContext.Artists.SingleOrDefault(a => a.ArtistId == artistId) : null;

    public bool AddToPlaylist(long playlistId, long trackId)
    {
        // enable lazy loading here
        var playlist = _dbContext.Playlists
            .Include(playlist => playlist.Tracks)
            .First(playlist => playlist.PlaylistId == playlistId);

        if (playlist.Tracks.Any(pt => pt.TrackId == trackId)) return false;

        var track = _dbContext.Tracks.Find(trackId);
        playlist.Tracks.Add(track);
        return _dbContext.SaveChanges() > 0;
    }
}