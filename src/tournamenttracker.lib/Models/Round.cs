using System;
using System.Collections.Generic;
using System.Linq;

namespace TournamentTracker.Models
{
    public class Round
    {
        public IEnumerable<Pairing> Pairings { get; set; }

        public Round()
        {
            Pairings = new Pairing[0];
        }

        public IOrderedEnumerable<KeyValuePair<Player, int>> GetStandings()
        {
            var standings = new Dictionary<Player, int>();
            foreach (var i in Pairings)
            {
                standings.Add(i.Player1, i.P1Score);
                standings.Add(i.Player2, i.P2Score);
            }
            return standings.OrderByDescending(i => i.Value);
        }

        public int GetPlayerScore(Player player)
        {
            foreach (var i in Pairings)
            {
                if (player.Equals(i.Player1))
                {
                    return i.P1Score;
                }
                if (player.Equals(i.Player2))
                {
                    return i.P2Score;
                }
            }
            throw new ArgumentException("Player not found!");
        }

        public void Swap(Player first, Player second)
        {
            var pairing1 = Pairings.FirstOrDefault(i => i.ContainsPlayers(first));
            var pairing2 = Pairings.FirstOrDefault(i => i.ContainsPlayers(second));

            if (pairing1 == null || pairing2 == null)
            {
                throw new ArgumentException("Could not swap players! Requested player was not found");
            }

            pairing1.ReplacePlayer(first, second);
            pairing2.ReplacePlayer(second, first);
        }
    }
}