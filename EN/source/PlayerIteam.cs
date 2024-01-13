using System.Threading;

namespace RanksPointsNamespace;
public partial class RanksPoints
{
    public class PlayerIteam
    {

        public string? ClanTag { get; set; }
        public int? value { get; set; }
        public int? valuechange { get; set; }
        public int? rank { get; set; }

        public PlayerIteam()
        {
            Init();
        }

        public void Init()
        {
            ClanTag = null;
            value = 0;
            valuechange = 0;    
            rank = 0;
        }
    }
}

