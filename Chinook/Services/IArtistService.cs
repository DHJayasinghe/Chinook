using Chinook.ClientModels;

namespace Chinook.Services;

public interface IArtistService
{
    Artist Get(long id);
    List<Artist> Search(string artistName);
    List<Album> GetAlbums(long artistId);
}