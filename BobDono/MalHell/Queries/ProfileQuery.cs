using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BobDono.Interfaces;
using BobDono.MalHell.Comm;
using BobDono.MalHell.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace BobDono.MalHell.Queries
{
    public class ProfileQuery
    {
        private IHttpClientProvider _httpClientProvider;

        public ProfileQuery(IHttpClientProvider httpClientProvider)
        {
            _httpClientProvider = httpClientProvider;
        }

        public async Task<MalProfile.ProfileData> GetProfileData(string userName)
        {
            try
            {
                var raw = await _httpClientProvider.HttpClient.GetStringAsync(
                    $"https://myanimelist.net/profile/{userName}");
                var doc = new HtmlDocument();
                doc.LoadHtml(raw);
                var current = new MalProfile.ProfileData {User = {Name = userName } };


                #region FavChar

                try
                {
                    foreach (
                        var favCharNode in
                        doc.DocumentNode.Descendants("ul")
                            .First(
                                node =>
                                    node.Attributes.Contains("class") &&
                                    node.Attributes["class"].Value ==
                                    "favorites-list characters")
                            .Descendants("li"))
                    {
                        var curr = new AnimeCharacter();
                        var imgNode = favCharNode.Descendants("a").First();
                        var styleString = imgNode.Attributes["style"].Value.Substring(22);
                        curr.ImgUrl = styleString.Replace("/r/80x120", "");
                        curr.ImgUrl = curr.ImgUrl.Substring(0, curr.ImgUrl.IndexOf('?'));
                        var infoNode = favCharNode.Descendants("div").Skip(1).First();
                        var nameNode = infoNode.Descendants("a").First();
                        curr.Name = nameNode.InnerText.Trim();
                        curr.Id = nameNode.Attributes["href"].Value.Substring(9).Split('/')[2];
                        var originNode = infoNode.Descendants("a").Skip(1).First();
                        curr.Notes = originNode.InnerText.Trim();
                        curr.ShowId = originNode.Attributes["href"].Value.Split('/')[2];
                        curr.FromAnime = originNode.Attributes["href"].Value.Split('/')[1] == "anime";
                        current.FavouriteCharacters.Add(curr);
                    }
                }
                catch (Exception)
                {
                    //no favs
                }

                #endregion

                #region FavManga

                try
                {
                    foreach (
                        var favMangaNode in
                        doc.DocumentNode.Descendants("ul")
                            .First(
                                node =>
                                    node.Attributes.Contains("class") &&
                                    node.Attributes["class"].Value ==
                                    "favorites-list manga")
                            .Descendants("li"))
                    {
                        current.FavouriteManga.Add(
                            int.Parse(
                                favMangaNode.Descendants("a").First().Attributes["href"].Value.Substring(9).Split('/')[2
                                ]));
                    }
                }
                catch (Exception)
                {
                    //no favs
                }

                #endregion

                #region FavAnime

                try
                {
                    foreach (
                        var favAnimeNode in
                        doc.DocumentNode.Descendants("ul")
                            .First(
                                node =>
                                    node.Attributes.Contains("class") &&
                                    node.Attributes["class"].Value ==
                                    "favorites-list anime")
                            .Descendants("li"))
                    {
                        current.FavouriteAnime.Add(
                            int.Parse(
                                favAnimeNode.Descendants("a").First().Attributes["href"].Value.Substring(9).Split('/')[2
                                ]));
                    }
                }
                catch (Exception)
                {
                    //no favs
                }

                #endregion

                #region FavPpl

                try
                {
                    foreach (
                        var favPersonNode in
                        doc.DocumentNode.Descendants("ul")
                            .First(
                                node =>
                                    node.Attributes.Contains("class") &&
                                    node.Attributes["class"].Value ==
                                    "favorites-list people")
                            .Descendants("li"))
                    {
                        var curr = new AnimeStaffPerson();
                        var aElems = favPersonNode.Descendants("a");
                        var styleString = aElems.First().Attributes["style"].Value.Substring(22);
                        curr.ImgUrl = styleString.Replace("/r/80x120", "");
                        curr.ImgUrl = curr.ImgUrl.Substring(0, curr.ImgUrl.IndexOf('?'));

                        curr.Name = aElems.Skip(1).First().InnerText.Trim();
                        curr.Id = aElems.Skip(1).First().Attributes["href"].Value.Substring(9).Split('/')[2];

                        current.FavouritePeople.Add(curr);
                    }
                }
                catch (Exception)
                {
                    //no favs
                }

                #endregion



                return current;
            }
            catch (Exception e)
            {
                //it's html
            }
            return new MalProfile.ProfileData();
        }
    }
}