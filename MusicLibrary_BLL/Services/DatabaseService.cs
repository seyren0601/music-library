﻿using MusicLibrary_BLL.Models;
using MusicLibrary_DAL;
using MusicLibrary_DAL.Entities;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicLibrary_BLL.Services
{
    public class DatabaseService
    {
        static DatabaseService instance;
        public static DatabaseService GetInstance()
        {
            if (instance == null) return instance = new DatabaseService();
            return instance;
        }

        public dbo_Album FindAlbum(dbo_Album Album)
        {
            using (MusicLibraryDbContext dbContext = new MusicLibraryDbContext())
            {
                return dbContext.Albums.FirstOrDefault(a => a.Equals(Album));
            }
        }

        public dbo_Album FindAlbum(string AlbumName)
        {
            using (MusicLibraryDbContext dbContext = new MusicLibraryDbContext())
            {
                return dbContext.Albums.FirstOrDefault(a => a.Title == AlbumName);
            }

        }
        public dbo_Artist FindArtist(dbo_Artist Artist)
        {
            using (MusicLibraryDbContext dbContext = new MusicLibraryDbContext())
            {
                return dbContext.Artists.FirstOrDefault(a => a.Equals(Artist));
            }
        }

        public void AddAlbum(dbo_Album Album)
        {
            using(MusicLibraryDbContext dbContext = new MusicLibraryDbContext())
            {
                dbContext.Albums.Add(Album);
                dbContext.SaveChanges();
            }
        }

        public bool RemoveAlbum(string albumName)
        {
            using(MusicLibraryDbContext dbContext = new MusicLibraryDbContext())
            {
                dbo_Album albumToDelete = dbContext.Albums.FirstOrDefault(a => a.Title == albumName);
                if(albumToDelete == null)
                {
                    return false;
                }
                var songsInAlbum = from a in dbContext.Albums
                                   join i in dbContext.AlbumInfos on a.AlbumID equals i.AlbumID
                                   join m in dbContext.MusicFiles on i.SongID equals m.SongID
                                   where a.Title == albumToDelete.Title
                                   select m;
                foreach(var songs in songsInAlbum) // Remove data in musicfiles
                {
                    dbContext.MusicFiles.Remove(songs);
                }
                dbContext.Albums.Remove(albumToDelete); // Remove data in album
                dbContext.SaveChanges();
                return true;
            }
        }

        public void AddArtist(dbo_Artist Artist)
        {
            using (MusicLibraryDbContext dbContext = new MusicLibraryDbContext())
            {
                dbContext.Artists.Add(Artist);
                dbContext.SaveChanges();
            }
        }

        public bool RemoveArtist(string artistName)
        {
            using(var dbContext = new MusicLibraryDbContext())
            {
                var artistToDelete = dbContext.Artists.FirstOrDefault(a => a.ArtistName == artistName);
                if(artistToDelete == null)
                {
                    return false;
                }
                dbContext.Artists.Remove(artistToDelete);
                dbContext.SaveChanges();
                return true;
            }
        }

        public async void AddFiles(List<dbo_MusicFile> files, dbo_Album album)
        {
            using (MusicLibraryDbContext dbContext = new MusicLibraryDbContext())
            {
                foreach (var file in files)
                {
                    if (dbContext.MusicFiles.FirstOrDefault(x => x.SongID == file.SongID) == null)
                        await dbContext.MusicFiles.AddAsync(file);
                    await dbContext.AlbumInfos.AddAsync(new dbo_AlbumInfo()
                    {
                        AlbumID = album.AlbumID,
                        SongID = file.SongID
                    });
                }
                dbContext.SaveChanges();
            }
        }

        public void RemoveFile(dbo_MusicFile file)
        {
            using(MusicLibraryDbContext dbContext = new MusicLibraryDbContext())
            {
                var playlistRecords = from m in dbContext.MusicFiles
                                        join pi in dbContext.PlaylistInfo on m.SongID equals pi.SongID
                                        where m.SongID == file.SongID
                                        select pi;
                foreach (MusicList musicList in playlistRecords) // Remove playlist records that have deleted songs
                {
                    dbContext.PlaylistInfo.Remove(musicList);
                }
                dbContext.MusicFiles.Remove(file);
                dbContext.SaveChanges();
            }
        }

        public async void AddPlaylist(Playlist playlist)
        {
            using (MusicLibraryDbContext dbContext = new MusicLibraryDbContext())
            {
                await dbContext.Playlists.AddAsync(playlist);
                if (playlist.PlaylistInfo != null)
                {
                    foreach (MusicFile song in playlist.PlaylistInfo)
                    {
                        await dbContext.PlaylistInfo.AddAsync(new MusicList()
                        {
                            SongID = song.MusicBrainzID,
                            PlaylistID = playlist.PlaylistID,
                        });
                    }
                }
                dbContext.SaveChanges();
            }
        }

        public void RemovePlaylistSong(Playlist playlist, MusicFile file)
        {
            using(MusicLibraryDbContext dbContext = new MusicLibraryDbContext())
            {
                dbContext.PlaylistInfo.Remove(new MusicList()
                {
                    SongID = file.MusicBrainzID,
                    PlaylistID = playlist.PlaylistID
                });
                dbContext.SaveChanges();
            }
        }

        public void RemovePlaylist(Playlist playlist, string username) 
        {
            using(MusicLibraryDbContext dbContext = new MusicLibraryDbContext())
            {
                IQueryable<MusicList> playlistSongs = from s in dbContext.PlaylistInfo
                                                      join p in dbContext.Playlists on s.PlaylistID equals p.PlaylistID
                                                      where s.PlaylistID == playlist.PlaylistID && p.username == username
                                                      select s;
                foreach(MusicList song in playlistSongs)
                {
                    dbContext.PlaylistInfo.Remove(song);
                }
                dbContext.Playlists.Remove(playlist);
                dbContext.SaveChanges();
            }
        }

        public async void UpdatePlaylist(Playlist playlist, MusicList musiclist)
        {
            using (MusicLibraryDbContext dbContext = new MusicLibraryDbContext())
            {
                if (musiclist != null)
                {
                    foreach (MusicFile song in musiclist)
                    {
                        if (dbContext.PlaylistInfo.Count(e => e.SongID == song.MusicBrainzID && e.PlaylistID == playlist.PlaylistID) == 0)
                        {
                            await dbContext.PlaylistInfo.AddAsync(new MusicList()
                            {
                                SongID = song.MusicBrainzID,
                                PlaylistID = playlist.PlaylistID,
                            });
                        }
                    }
                    dbContext.SaveChanges();
                }
            }
        }

        public List<Playlist> FetchUserPlaylists(string input)
        {
            using (MusicLibraryDbContext dbContext = new MusicLibraryDbContext())
            {
                ParallelQuery<Playlist> queryResult = dbContext.Playlists.Where(p=>p.username == input).AsParallel();
                if(queryResult.Count() > 0)
                {
                    List<Playlist> PlaylistList = new List<Playlist>();
                    foreach(Playlist item in queryResult)
                    {
                        PlaylistList.Add(new Playlist()
                        {
                            PlaylistID = item.PlaylistID,
                            PlaylistName = item.PlaylistName,
                            username = input
                        });
                    }
                    return PlaylistList;
                }
                return null;
            }
        }

        public BindingList<MusicFile> FetchPlaylistSongs(int playlistID, string username)
        {
            using (MusicLibraryDbContext dbContext = new MusicLibraryDbContext())
            {
                var queryResult = from p in dbContext.Playlists
                                  join info in dbContext.PlaylistInfo on p.PlaylistID equals info.PlaylistID
                                  join s in dbContext.MusicFiles on info.SongID equals s.SongID
                                  join a in dbContext.Albums on s.Albums.ToList().First().AlbumID equals a.AlbumID
                                  orderby s.FilePath
                                  where p.PlaylistID == playlistID && p.username == username
                                  select new MusicFile()
                                  {
                                        MusicBrainzID = s.SongID,
                                        Artist = s.Artist.ArtistName,
                                        Album = a.Title,
                                        FilePath = s.FilePath,
                                        Title = s.Title,
                                        PlayTime = TimespanToString(new AudioFileReader(s.FilePath).TotalTime),
                                        Number = s.TrackOffset
                                  };
                BindingList<MusicFile> Songs = new BindingList<MusicFile>(queryResult.ToList());
                return Songs;
            }
        }

        public static string TimespanToString(TimeSpan timespan)
        {
            return timespan.Minutes.ToString("D2") + ":" + timespan.Seconds.ToString("D2");
        }
    }
}
