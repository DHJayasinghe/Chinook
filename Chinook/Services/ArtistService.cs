using Chinook.ClientModels;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Services;

public class ArtistService : IArtistService
{
    private readonly ChinookContext _dbContext;

    public ArtistService(IDbContextFactory<ChinookContext> dbContext)
    {
        _dbContext = dbContext.CreateDbContext();
    }

    public Artist Get(long id) => id > 0
        ? _dbContext.Artists
            .Select(a => new Artist { Name = a.Name, ArtistId = a.ArtistId })
            .SingleOrDefault(a => a.ArtistId == id)
        : null;

    public List<Artist> Search(string artistName) => string.IsNullOrEmpty(artistName)
        ? _dbContext.Artists
            .Select(a => new Artist
            {
                Name = a.Name,
                ArtistId = a.ArtistId,
                AlbumCount = a.Albums.Count()
            }).ToList()
        : _dbContext.Artists.Include(a => a.Albums).Select(a => new Artist { Name = a.Name, ArtistId = a.ArtistId })
        .Where(artist => artist.Name.Contains(artistName)).ToList();

    public List<Album> GetAlbums(long artistId) => _dbContext.Albums
        .Select(a => new Album { Title = a.Title, ArtistId = a.ArtistId, AlbumId = a.AlbumId })
        .Where(a => a.ArtistId == artistId).ToList();
}