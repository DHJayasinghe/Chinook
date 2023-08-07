using Chinook.ClientModels;
using Chinook.Models;

namespace Chinook.Services;

public interface ITrackService
{
    List<PlaylistTrack> GetByArtistWithUserFavorite(long artistId, string requestedUserId);
    List<PlaylistTrack> GetByPlaylistWithUserFavorite(long playlistId, string requestedUserId);
    Artist GetArtist(long artistId);
    bool AddToPlaylist(long playlistId, long trackId);
}