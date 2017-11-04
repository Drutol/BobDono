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
    public class StaffDetailsQuery : IStaffDetailsQuery
    {
        private readonly IHttpClientProvider _httpClientProvider;

        public StaffDetailsQuery(IHttpClientProvider httpClientProvider)
        {
            _httpClientProvider = httpClientProvider;
        }

        public async Task<StaffDetailsData> GetStaffDetails(int id)
        {
            var output = new StaffDetailsData();
            var raw = await _httpClientProvider.HttpClient.GetStringAsync($"https://myanimelist.net/people/{id}");
            if (string.IsNullOrEmpty(raw))
                return output;
            var doc = new HtmlDocument();
            doc.LoadHtml(raw);

            output.Id = id;
            try
            {
                var columns =
                    doc.DocumentNode.Descendants("table").First().ChildNodes[0].ChildNodes.Where(
                        node => node.Name == "td").ToList();
                var leftColumn = columns[0];
                var image = leftColumn.Descendants("img").FirstOrDefault();
                if (image != null && image.Attributes.Contains("alt"))
                    output.ImgUrl = image.Attributes["src"].Value;

                output.Name = WebUtility.HtmlDecode(doc.DocumentNode.Descendants("h1").First().InnerText.Trim());

                bool recording = false;
                var currentString = "";
                int i = 0;
                foreach (var child in leftColumn.ChildNodes)
                {
                    if (!recording)
                    {
                        if (child.Attributes.Contains("class") &&
                            child.Attributes["class"].Value.Trim() == "js-sns-icon-container icon-block")
                            recording = true;
                        else
                            continue;
                    }

                    if (child.Attributes.Contains("class") &&
                        child.Attributes["class"].Value == "spaceit_pad")
                    {
                        output.Details.Add(WebUtility.HtmlDecode(child.InnerText.Trim()));
                        currentString = "";
                        i = 0;
                    }
                    else if (!string.IsNullOrWhiteSpace(child.InnerText))
                    {
                        currentString += WebUtility.HtmlDecode(child.InnerText.Trim()) + " ";
                        i++;
                        if (i == 2)
                        {
                            output.Details.Add(currentString);
                            currentString = "";
                            i = 0;
                        }
                    }

                    if (child.Name == "div" && !child.Attributes.Contains("class"))
                        break;
                }

                foreach (var table in columns[1].Descendants("table").Take(2))
                    try
                    {
                        foreach (var row in table.Descendants("tr"))
                        {

                            var tds = row.Descendants("td").ToList();
                            if (tds.Count == 4)
                            {
                                var current = new ShowCharacterPair();
                                var show = new AnimeLightEntry();
                                var img = tds[0].Descendants("img").First().Attributes["data-src"].Value;
                                if (!img.Contains("questionmark"))
                                {
                                    img = Regex.Replace(img, @"\/r\/\d+x\d+", "");
                                    show.ImgUrl = img.Substring(0, img.IndexOf('?'));
                                }
                                var link = tds[1].Descendants("a").First();
                                show.IsAnime = true;
                                show.Id = int.Parse(link.Attributes["href"].Value.Split('/')[2]);
                                show.Title = WebUtility.HtmlDecode(link.InnerText.Trim());
                                current.AnimeLightEntry = show;

                                var character = new AnimeCharacter();
                                character.FromAnime = true;
                                character.ShowId = show.Id.ToString();
                                link = tds[2].Descendants("a").First();
                                character.Id = link.Attributes["href"].Value.Split('/')[2];
                                character.Name = WebUtility.HtmlDecode(link.InnerText.Trim());
                                character.Notes = WebUtility.HtmlDecode(tds[2].Descendants("div").Last().InnerText);

                                img = tds[3].Descendants("img").First().Attributes["data-src"].Value;
                                if (!img.Contains("questionmark"))
                                {
                                    img = Regex.Replace(img, @"\/r\/\d+x\d+", "");
                                    character.ImgUrl = img.Substring(0, img.IndexOf('?'));
                                }

                                current.AnimeCharacter = character;
                                output.ShowCharacterPairs.Add(current);
                            }
                            else
                            {
                                var show = new AnimeLightEntry();
                                var img = tds[0].Descendants("img").First().Attributes["data-src"].Value;
                                if (!img.Contains("questionmark"))
                                {
                                    img = Regex.Replace(img, @"\/r\/\d+x\d+", "");
                                    show.ImgUrl = img.Substring(0, img.IndexOf('?'));
                                }
                                var link = tds[1].Descendants("a").First();
                                show.IsAnime = !link.Attributes["href"].Value.Contains("/manga/");
                                show.Id = int.Parse(link.Attributes["href"].Value.Split('/')[2]);
                                show.Title = WebUtility.HtmlDecode(link.InnerText.Trim());
                                show.Notes =
                                    WebUtility.HtmlDecode(
                                        tds[1].Descendants("div").Last().InnerText.Replace("add", "").Trim());

                                output.StaffPositions.Add(show);

                            }
                        }
                    }
                    catch
                        (Exception e)
                    {
                        //htaml
                    }
            }
            catch (Exception)
            {
                //sorcery 
            }          

            return output;
        }
    }
}
