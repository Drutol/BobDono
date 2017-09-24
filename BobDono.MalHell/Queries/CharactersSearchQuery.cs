using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BobDono.Interfaces;
using BobDono.Models.MalHell;
using HtmlAgilityPack;

namespace BobDono.MalHell.Queries
{
    public class CharactersSearchQuery : ICharactersSearchQuery
    {
        private readonly IHttpClientProvider _httpClientProvider;

        public CharactersSearchQuery(IHttpClientProvider httpClientProvider)
        {
            _httpClientProvider = httpClientProvider;
        }


        public async Task<List<AnimeCharacter>> GetSearchResults(string query)
        {
            var output = new List<AnimeCharacter>();

            var raw = await _httpClientProvider.HttpClient.GetStringAsync(
                $"https://myanimelist.net/character.php?q={query}");
            if (string.IsNullOrEmpty(raw))
                return null;
            var doc = new HtmlDocument();
            doc.LoadHtml(raw);

            try
            {
                foreach (var row in doc.DocumentNode.Descendants("table").First().Descendants("tr").Skip(1))
                {
                    try
                    {
                        var character = new AnimeCharacter();
                        var tds = row.Descendants("td").ToList();
                        var link = tds[1].Descendants("a").First();
                        character.Id = link.Attributes["href"].Value.Split('/')[2];
                        character.Name = WebUtility.HtmlDecode(link.InnerText.Trim());
                        var smalls = tds[1].Descendants("small");
                        if (smalls.Any())
                            character.Notes = WebUtility.HtmlDecode(smalls.Last().InnerText);

                        var img = tds[0].Descendants("img").First().Attributes["src"].Value;
                        if (!img.Contains("questionmark"))
                        {
                            img = Regex.Replace(img, @"\/r\/\d+x\d+", "");
                            character.ImgUrl = img.Substring(0, img.IndexOf('?'));
                        }

                        var links = tds[2].Descendants("a").ToList();
                        if (links.Any())
                            character.ShowName = WebUtility.HtmlDecode(links.First().InnerText.Trim());

                        output.Add(character);
                    }
                    catch (Exception)
                    {
                        //
                    }

                    
                }
            }
            catch (Exception)
            {
                //
            }

            return output;
        }
    }
}
