using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.Config
{
    public class RecentPathSection
    {
        public string? SendRomFolder { get; set; }

        public string? RamromPcReplayFolder { get; set; }

        public List<string> RecentSendRom { get; set; } = new List<string>();
    }
}
