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
        public static bool GetSubject(this DiscordMessage message,out string name)
        {
            if (message.MentionedUsers.Any())
            {
                name = null;
                return false;
            }
            name = message.Content.Split(' ').Last();
            return true;
        }
    }
}
