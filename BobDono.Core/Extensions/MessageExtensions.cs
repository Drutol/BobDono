using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace BobDono.Core.Extensions
{
    public static class MessageExtensions
    {
        public static string GetSubject(this DiscordMessage message)
        {
            var args = message.Content.Split(' ');
            return message.MentionedUsers.Any()
                ? message.MentionedUsers.First().Username
                : args.Last();
        }
    }
}
