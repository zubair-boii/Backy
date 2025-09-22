using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backy.Classes
{
    public class AppConfig
    {
        public string StorePath { get; set; }
        public List<GameData> Games { get; set; } = new();
    }
}
