using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTrackerWin.Models
{
    public class TrackedWindow
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int TimeSpent { get; set; }
    }

    public class TrackedWindowStorage: TrackedWindow
    {
        public string UserName { get; set; }
        public DateTime Date { get; set; }
    }
}
