using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace BobDono.MalHell.Models
{
    [DataContract]
    public class MalUser
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string ImgUrl { get; set; }

        private sealed class NameEqualityComparer : IEqualityComparer<MalUser>
        {
            public bool Equals(MalUser x, MalUser y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return string.Equals(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase);
            }

            public int GetHashCode(MalUser obj)
            {
                return obj.Name?.GetHashCode() ?? 0;
            }
        }

        public static IEqualityComparer<MalUser> NameComparer { get; } = new NameEqualityComparer();
    }
}
