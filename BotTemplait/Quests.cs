using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTemplait
{
    public class QuestContainer
    {
        public Quest[] quests { get; set; }
    }

    public class Quest
    {
        public int id { get; set; }
        public string message { get; set; }
        public string column { get; set; }
        public Button[] buttons { get; set; }
    }
}
