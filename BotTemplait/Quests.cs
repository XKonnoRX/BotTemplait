using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTemplait
{
    public class QuestContainer
    {
        public string name {  get; set; }
        public string table { get; set; }
        public Dictionary<string, string> messages { get; set; }
        public Quest[] quests { get; set; }
    }

    public class Quest
    {
        public int id { get; set; }
        public string name { get; set; }
        public string message { get; set; }
        public string column { get; set; }
        public Button[] buttons { get; set; }
    }
}
