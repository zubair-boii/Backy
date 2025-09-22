using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backy.Classes
{
    public class GameData
    {
        public GameData()
        {
            
        }

        public int id { get; set; }
        public string name { get; set; }
        public string saveLocation { get; set; }
        public string storeLocation { get; set; }
        public bool IsBackedUp { get; set; }

    }
}
