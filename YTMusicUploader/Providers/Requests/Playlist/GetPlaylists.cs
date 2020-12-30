﻿using JBToolkit.StreamHelpers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using YTMusicUploader.Providers.DataModels;
using YTMusicUploader.Providers.RequestModels;
using static YTMusicUploader.Providers.RequestModels.ArtistCache;

namespace YTMusicUploader.Providers
{
    /// <summary>
    /// YouTube Music API Request Methods
    /// 
    /// Thanks to: sigma67: 
    ///     https://ytmusicapi.readthedocs.io/en/latest/ 
    ///     https://github.com/sigma67/ytmusicapi
    /// </summary>
    public partial class Requests
    {
        public partial class Playlists
        {
            public static PlaylistCollection GetPlaylists(
                string cookieValue)
            {
                var playListCol = new PlaylistCollection();

                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(Global.YTMusicBaseUrl +
                                                                    "browse?" + Global.YTMusicParams);

                    request = AddStandardHeaders(request, cookieValue);

                    request.ContentType = "application/json; charset=UTF-8";
                    request.Headers["X-Goog-AuthUser"] = "0";
                    request.Headers["x-origin"] = "https://music.youtube.com";
                    request.Headers["X-Goog-Visitor-Id"] = Global.GoogleVisitorId;
                    request.Headers["Authorization"] = GetAuthorisation(GetSAPISIDFromCookie(cookieValue));

                    byte[] postBytes = GetPostBytes(
                                            SafeFileStream.ReadAllText(
                                                    Path.Combine(Global.WorkingDirectory, @"AppData\get_playlists_context.json")));

                    request.ContentLength = postBytes.Length;
                    using (var requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(postBytes, 0, postBytes.Length);
                        requestStream.Close();
                    }

                    postBytes = null;
                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        string result;
                        using (var brotli = new Brotli.BrotliStream(response.GetResponseStream(),
                                                                    System.IO.Compression.CompressionMode.Decompress,
                                                                    true))
                        {
                            var streamReader = new StreamReader(brotli);
                            result = streamReader.ReadToEnd();

                            var playListsResultContext = JsonConvert.DeserializeObject<BrowsePlaylistsResultsContext>(result);
                            var playListResults = playListsResultContext.contents
                                                                        .singleColumnBrowseResultsRenderer
                                                                        .tabs[0]
                                                                        .tabRenderer
                                                                        .content
                                                                        .sectionListRenderer
                                                                        .contents[1]
                                                                        .itemSectionRenderer
                                                                        .contents[0]
                                                                        .gridRenderer
                                                                        .items;

                            foreach (var item in playListResults)
                            {
                                if (item.musicTwoRowItemRenderer.title.runs[0].text != "New playlist" &&
                                    item.musicTwoRowItemRenderer.title.runs[0].text != "Your likes")
                                {
                                    try
                                    {
                                        playListCol.Add(new Playlist
                                        {
                                            Title = item.musicTwoRowItemRenderer.title.runs[0].text,
                                            Subtitle = item.musicTwoRowItemRenderer.subtitle.runs[0].text +
                                                   item.musicTwoRowItemRenderer.subtitle.runs[1].text +
                                                   item.musicTwoRowItemRenderer.subtitle.runs[2].text,
                                            BrowseId = item.musicTwoRowItemRenderer.navigationEndpoint.browseEndpoint.browseId,
                                            CoverArtUrl = item.musicTwoRowItemRenderer.thumbnailRenderer.musicThumbnailRenderer.thumbnail.thumbnails[0].url
                                        });
                                    }
                                    catch(Exception e) 
                                    {
                                        Logger.Log(e, "GetPlaylists - Error fetching a playlist", Log.LogTypeEnum.Error);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    var _ = e;
#if DEBUG
                    Console.Out.WriteLine("GetPlaylists: " + e.Message);
#endif
                }

                return playListCol;
            }
        }
    }
}
