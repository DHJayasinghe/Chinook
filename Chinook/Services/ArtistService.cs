﻿using Chinook.Models;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Services;

public class ArtistService
{
    private readonly ChinookContext _dbContext;

    public ArtistService(IDbContextFactory<ChinookContext> dbContext)
    {
        _dbContext = dbContext.CreateDbContext();
    }

    public Artist Get(long id) => id > 0 ? _dbContext.Artists.SingleOrDefault(a => a.ArtistId == id) : null;

    public List<Artist> Search(string artistName) => string.IsNullOrEmpty(artistName) ? _dbContext.Artists.ToList() : _dbContext.Artists.Where(artist => artist.Name.Contains(artistName)).ToList();

    public List<Album> GetAlbums(long artistId) => _dbContext.Albums.Where(a => a.ArtistId == artistId).ToList();
}

