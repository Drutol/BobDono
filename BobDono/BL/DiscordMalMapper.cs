using System;
using System.Collections.Generic;
using System.Text;

namespace BobDono.BL
{
    public static class DiscordMalMapper
    {
        private static Dictionary<ulong, string> _usersDictionary = new Dictionary<ulong, string>
        {
            {194547452496445440, "IATGOF"},
            {202501452596379648, "FoxInFlame"},
            {188557595382906880, "Syaoran3"},
            {74458088760934400, "Drutol"},
            {183911061496266752, "Benpai"},
            {172153939838500865, "motoko"},
            {314138361537363969, "point-nemo"},
            {115110035230687241, "trigger_death"},
            {210132576092946432, "ArtoriasMoreder"},
            {267387436349259776, "G-Lodan"},
            {269408733971349504, "IronHunter"},
            {303236614623068161, "Butterstroke"},
            {226457042171330560, "Xaetral"},
            {179286206087954432, "spacepyro"},
            {147756722440634368, "PolyMagic"},
            {173746079471239168, "AnnyInNulls"},
            {327119283718979584, "otterman965"},
        };

        public static bool TryGetMalUsername(ulong discordUserId, out string malName)
        {
            if (_usersDictionary.ContainsKey(discordUserId))
            {
                malName = _usersDictionary[discordUserId];
                return true;
            }
            malName = null;
            return false;
        }

        public static string TryGetMalUsername(string sourceName)
        {
            foreach (var name in _usersDictionary.Values)
            {
                if (name.StartsWith(sourceName, StringComparison.CurrentCultureIgnoreCase))
                    return name;
            }
            return sourceName;
        }

    }
}
