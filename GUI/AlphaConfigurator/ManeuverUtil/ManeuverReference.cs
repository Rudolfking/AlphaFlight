using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaConfigurator.ManeuverUtil
{
    public class ManeuverReference
    {
        [JsonProperty("UidReference")]
        public int UidReference { get; set; }

        [JsonIgnore]
        public bool IsHighlighted { get; set; } = false;

        private static int myUid = 500000;
        [JsonProperty("MyUid")]
        public int MyUid { get; private set; }

        public string GuiText
        {
            get
            {
                var dc = TheHost.Maneuvers.FirstOrDefault(x => x.Uid == UidReference);
                if (dc == null)
                    return "##### UNAUTHORIZED #####";
                return dc.Name;
            }
        }

        [JsonIgnore]
        public MainWindow TheHost { get; set; }

        public ManeuverReference()
        {

        }

        public static void UpdateUid(int uid)
        {
            myUid = uid;
        }

        public ManeuverReference(int uid, MainWindow host)
        {
            MyUid = myUid++;
            TheHost = host;
            UidReference = uid;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ManeuverReference))
                return false;
            var other = obj as ManeuverReference;

            if (other.MyUid != MyUid)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return MyUid.GetHashCode();
        }
    }
}
