using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BobDono.Interfaces;
using BobDono.Interfaces.Queries;
using BobDono.MalHell.Comm;
using BobDono.Models.MalHell;
using HtmlAgilityPack;

namespace BobDono.MalHell.Queries
{
    public class CharacterDetailsQuery : ICharacterDetailsQuery
    {
        private readonly IHttpClientProvider _httpClientProvider;

        public CharacterDetailsQuery(IHttpClientProvider httpClientProvider)
        {
            _httpClientProvider = httpClientProvider;
        }

        public async Task<CharacterDetailsData> GetCharacterDetails(int id)
        {
            var output = new CharacterDetailsData();
            var raw = await _httpClientProvider.HttpClient.GetStringAsync($"https://myanimelist.net/character/{id}");
            if (string.IsNullOrEmpty(raw))
                return output;
            var doc = new HtmlDocument();
            doc.LoadHtml(raw);

            
            output.Id = id;
            try
            {
                var columns = doc.DocumentNode.Descendants("table").First().ChildNodes[1].ChildNodes.Where(node => node.Name == "td").ToList();
                var leftColumn = columns[0];
                var tables = leftColumn.Descendants("table");
                foreach (var table in tables)
                {
                    foreach (var descendant in table.Descendants("tr"))
                    {
                        var links = descendant.Descendants("a").ToList();
                        if (links[0].Attributes["href"].Value.Contains("/anime/"))
                        {
                            var curr = new AnimeLightEntry { IsAnime = true };
                            curr.Id = int.Parse(links[0].Attributes["href"].Value.Split('/')[4]);
                            var img = links[0].Descendants("img").First().Attributes["src"].Value;
                            if (!img.Contains("questionmark"))
                            {
                                img = Regex.Replace(img, @"\/r\/\d+x\d+", "");
                                curr.ImgUrl = img.Substring(0, img.IndexOf('?'));
                            }
                            curr.Title = WebUtility.HtmlDecode(links[1].InnerText.Trim());
                            output.Animeography.Add(curr);
                        }
                        else
                        {
                            var curr = new AnimeLightEntry { IsAnime = false };
                            curr.Id = int.Parse(links[0].Attributes["href"].Value.Split('/')[4]);
                            var img = links[0].Descendants("img").First().Attributes["src"].Value;
                            if (!img.Contains("questionmark"))
                            {
                                img = Regex.Replace(img, @"\/r\/\d+x\d+", "");
                                curr.ImgUrl = img.Substring(0, img.IndexOf('?'));
                            }
                            curr.Title = WebUtility.HtmlDecode(links[1].InnerText.Trim());
                            output.Mangaography.Add(curr);
                        }
                    }
                }               
                var image = leftColumn.Descendants("img").First();
                if (image.Attributes.Contains("alt"))
                {
                    output.ImgUrl = image.Attributes["src"].Value;
                }

                output.Name = WebUtility.HtmlDecode(doc.DocumentNode.Descendants("h1").First().InnerText).Trim().Replace("  "," "); //because mal tends to leave two spaces there and there's pretty hardcore guy on github who can spot such things... props ;d
                output.Content = output.SpoilerContent = "";
                output.Content += WebUtility.HtmlDecode(leftColumn.LastChild.InnerText.Trim()) + "\n\n";
                foreach (var node in columns[1].ChildNodes)
                {
                    if (node.Name == "#text")
                        output.Content += WebUtility.HtmlDecode(node.InnerText.Trim());
                    else if (node.Name == "br" && !output.Content.EndsWith("\n\n"))
                        output.Content += "\n";
                    else if (node.Name == "div" && node.Attributes.Contains("class") && node.Attributes["class"].Value == "spoiler")
                        output.SpoilerContent += WebUtility.HtmlDecode(node.InnerText.Trim()) + "\n\n";
                    else if (node.Name == "table")
                    {
                        foreach (var descendant in node.Descendants("tr"))
                        {
                            var current = new AnimeStaffPerson();
                            var img = descendant.Descendants("img").First();
                            var imgUrl = img.Attributes["src"].Value;
                            if (!imgUrl.Contains("questionmark"))
                            {
                                var pos = imgUrl.LastIndexOf("v");
                                if (pos != -1)
                                    imgUrl = imgUrl.Remove(pos, 1);

                            }
                            current.ImgUrl = imgUrl;
                            var info = descendant.Descendants("td").Last();
                            current.Id = info.ChildNodes[0].Attributes["href"].Value.Split('/')[4];
                            current.Name = WebUtility.HtmlDecode(info.ChildNodes[0].InnerText.Trim());
                            current.Notes = info.ChildNodes[2].InnerText;
                            output.VoiceActors.Add(current);
                        }
                    }
                }
                output.Content = output.Content.Trim();
                output.SpoilerContent = output.SpoilerContent.Trim();
            }
            catch (Exception)
            {
                //html
            }           

            return output;
        }
    }
}
