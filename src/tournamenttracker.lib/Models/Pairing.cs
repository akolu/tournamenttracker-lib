using System;
using System.Globalization;
using System.Linq;

namespace TournamentTracker.Models
{
    public class Pairing
    {
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public int P1Score { get; private set; }
        public int P2Score { get; private set; }

        public Pairing(Player p1, Player p2)
        {
            Player1 = p1;
            Player2 = p2;
        }

        public void SetScore(int p1, int p2)
        {
            P1Score = p1;
            P2Score = p2;
        }

        public bool ContainsPlayers(params Player[] other)
        {
            return other.All(player => player.Equals(Player1) || player.Equals(Player2));
        }

        public Player GetOpponent(Player player)
        {
            if (!ContainsPlayers(player))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Player '{0}' does not exist in this pairing", player.Name));
            }
            return player.Equals(Player1) ? Player2 : Player1;
        }

        public void ReplacePlayer(Player initial, Player replacement)
        {
            if (Player1.Equals(initial))
            {
                Player1 = replacement;
            }
            else if (Player2.Equals(initial))
            {
                Player2 = replacement;
            }
            else
            {
                throw new ArgumentException("Requested player could not be found!");
            }
        }
    }
}