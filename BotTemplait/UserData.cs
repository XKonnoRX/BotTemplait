using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BotTemplait
{
    [Table("users")]
    public class UserData
    {
        [Key]
        public long tg_id { get; set; }
        public int lastbotmsg {  get; set; }
        public string username { get; set; }
        public string? firstname { get; set; }
        public string? lastname { get; set; }
        public string? bot_state { get; set; }
    }
}
