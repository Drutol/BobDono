using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BobDono.Core.Attributes;
using BobDono.Core.Interfaces;
using BobDono.Interfaces;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;

namespace BobDono.Modules
{
    [Module(Name = "Fluff",Description = "Clearly commands of greatest importance.")]
    public class FluffModule
    {
        private SemaphoreSlim _ocrSemaphore = new SemaphoreSlim(1);

        private List<string> _supportedExtensions = new List<string>
        {
            ".png",
            ".jpg",
        };

        private Dictionary<ulong,DateTime> _ocrCooldowns = new Dictionary<ulong, DateTime>();
        private Dictionary<string,string> _ocrCache = new Dictionary<string, string>();

        private readonly IBotBackbone _botBackbone;
        private readonly IExceptionHandler _exceptionHandler;

        public FluffModule(IBotBackbone botBackbone, IExceptionHandler exceptionHandler)
        {
            _botBackbone = botBackbone;
            _exceptionHandler = exceptionHandler;
        }

       
        [CommandHandler(IgnoreRegexWrap = true, Regex = ".*java.*", HumanReadableCommand = "..java..",
            HelpText = "Oh sorry, I have allergy for **this** word.")]
        public async Task CoughCough(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            if (!(_botBackbone.ModuleInstances[typeof(ElectionsModule)] as ElectionsModule).ElectionsContexts.Any(
                context => context.ChannelIdContext == args.Channel.Id))
            {
                if(!args.Message.Content.ToLower().Contains("javascript"))
                    await args.Channel.SendMessageAsync("*cough cough*");
            }
        }

        [CommandHandler(Regex = @"ocr($|\sen|\sjp)", HumanReadableCommand = "ocr [en/jp]", Awaitable = false,
            HelpText =
                "Extracts text from first image in chat history and forwards it to Andrę for translation. You can call it once a minute. By default it will try to ocr moonrunes.")]
        public async Task Ocr(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            string ocrText = null;
            await _ocrSemaphore.WaitAsync();
            try
            {
                var msgs = await args.Channel.GetMessagesAsync();
                var lastWithImage = msgs.First(message =>
                    message.Attachments.Any(
                        attachment => _supportedExtensions.Any(s => attachment.FileName.EndsWith(s))) ||
                    message.Embeds.Any(embed => !string.IsNullOrEmpty(embed.Url?.ToString())));

                var link = string.Empty;

                if (lastWithImage.Attachments.Any())
                {
                    link = lastWithImage.Attachments
                        .FirstOrDefault(attachment => _supportedExtensions.Any(s => attachment.FileName.EndsWith(s)))
                        ?.Url;
                }

                if (lastWithImage.Embeds.Any() && link == string.Empty)
                {
                    link = lastWithImage.Embeds
                        .FirstOrDefault(embed => _supportedExtensions.Any(s => embed.Url.ToString().EndsWith(s)))
                        ?.Url
                        .ToString();
                }

                if (_ocrCache.ContainsKey(link))
                {
                    ocrText = _ocrCache[link];
                }

                if (ocrText == null)
                {
                    if (_ocrCooldowns.ContainsKey(args.Author.Id))
                    {
                        if (DateTime.UtcNow - _ocrCooldowns[args.Author.Id] < TimeSpan.FromMinutes(1))
                        {
                            await args.Channel.SendMessageAsync("You are still on cooldown! Calm your art!");
                            return;
                        }
                    }

                    try
                    {
                        await args.Channel.TriggerTypingAsync();

                        using (var client = new HttpClient())
                        {
                            var lang = args.Message.Content.Contains("en") ? "eng" : "jpn";

                            client.DefaultRequestHeaders.Add("apikey", "webocr5");
                            client.DefaultRequestHeaders.Referrer = new Uri("https://ocr.space/");

                            var cnt = new MultipartFormDataContent
                            {
                                {new StringContent(link), "url"},
                                {new StringContent(lang), "language"}
                            };

                            await client.GetAsync("https://ocr.space/");
                            var res = await client.PostAsync("https://api.ocr.space/parse/image", cnt);


                            var json = await res.Content.ReadAsStringAsync();
                            var result = JsonConvert.DeserializeObject<OcrResult>(json);
                            ocrText = result.ParsedResults.First().ParsedText;
                            _ocrCache.Add(link, ocrText);
                        }
                    }
                    catch (Exception)
                    {
                        ocrText = null;
                    }
                    finally
                    {
                        _ocrCooldowns[args.Author.Id] = DateTime.UtcNow;
                    }
                }

                if (string.IsNullOrEmpty(ocrText))
                {
                    await args.Channel.SendMessageAsync("I'm unable to read your doodly thingies.");
                }
                else
                {
                    await args.Channel.SendMessageAsync($"{(args.Message.Content.Contains("en") ? "" : "!tr ")}{ocrText}");
                }
            }
            catch (Exception e)
            {
                await args.Channel.SendMessageAsync(_exceptionHandler.Handle(e));
            }
            finally
            {
                _ocrSemaphore.Release();
            }
        }


        public class TextOverlay
        {
            public List<object> Lines { get; set; }
            public bool HasOverlay { get; set; }
            public string Message { get; set; }
        }

        public class ParsedResult
        {
            public TextOverlay TextOverlay { get; set; }
            public int FileParseExitCode { get; set; }
            public string ParsedText { get; set; }
            public string ErrorMessage { get; set; }
            public string ErrorDetails { get; set; }
        }

        public class OcrResult
        {
            public List<ParsedResult> ParsedResults { get; set; }
            public int OCRExitCode { get; set; }
            public bool IsErroredOnProcessing { get; set; }
            public object ErrorMessage { get; set; }
            public object ErrorDetails { get; set; }
            public string ProcessingTimeInMilliseconds { get; set; }
            public string SearchablePDFURL { get; set; }
        }
    }
}
