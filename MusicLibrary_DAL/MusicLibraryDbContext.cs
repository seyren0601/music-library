﻿using System.Configuration;
using Microsoft.EntityFrameworkCore;
using MusicLibrary_DAL.Entities;
using MySql.EntityFrameworkCore.Extensions;
using MySqlConnector;

namespace MusicLibrary_DAL
{
    public class MusicLibraryDbContext:DbContext
    {
        string ConnectionString = "server=localhost;database=musiclibrary;user=root;password=porsche0601";
        public DbSet<dbo_User> Users { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<dbo_Artist> Artists { get; set; }
        public DbSet<dbo_Album> Albums { get; set; }
        public DbSet<dbo_MusicFile> MusicFiles { get; set; }
        public DbSet<dbo_AlbumInfo> AlbumInfos { get; set; }
        public DbSet<MusicList> PlaylistInfo { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Playlist>()
                .Navigation(e => e.MusicLists);

            modelBuilder.Entity<dbo_Artist>()
                .Navigation(e => e.ALBUMS);


            modelBuilder.Entity<dbo_Album>()
                .Navigation(e => e.Artist);
            modelBuilder.Entity<dbo_Album>()
                .Navigation(e => e.Recordings);


            modelBuilder.Entity<dbo_MusicFile>()
                .Navigation(e => e.Artist);
            modelBuilder.Entity<dbo_MusicFile>()
                .Navigation(e => e.Albums);
            modelBuilder.Entity<dbo_MusicFile>()
                .Navigation(e => e.Playlists);
        }
    }
}
