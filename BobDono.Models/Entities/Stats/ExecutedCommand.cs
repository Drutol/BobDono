using System;

namespace BobDono.Models.Entities.Stats
{
    public class ExecutedCommand
    {
        public long Id { get; set; }

        public int CommandHash { get; set; }
        public int CallerHash { get; set; }

        public string CommandName { get; set; }
        public string CallerName { get; set; }

        public bool Existed { get; set; }
        public bool Contextual { get; set; }

        public DateTime Time { get; set; }
    }
}
