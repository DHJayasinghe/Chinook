using Chinook.ClientModels;

namespace Chinook.Services;

public interface IPlaylistService
{
    event EventHandler PlayListAddedEvent;
    const string FAVORITE_PLAYLIST_NAME = "My favorite tracks";

    long Add(string name, string userId);
    void AddFavoritePlaylistIfNotExist(string userId);
    bool AddToFavorite(string userId, long trackId);
    bool RemoveFromFavorite(string userId, long trackId);
    List<Playlist> GetByUser(string userId);
    Playlist Get(long id);
}