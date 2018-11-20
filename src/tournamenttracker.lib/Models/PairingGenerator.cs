using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TournamentTracker.Models
{
    public class PairingGenerator : IPairingGenerator
    {
        private const int SwissTimeout = 5000;

        public IEnumerable<Pairing> Shuffle(IList<Player> players)
        {
            ValidatePlayerCount(players);
            var pairings = new List<Pairing>();
            var shuffled = players.OrderBy(i => Guid.NewGuid()).ToList();
            for (var i = 0; i < shuffled.Count; i += 2)
            {
                pairings.Add(new Pairing(shuffled[i], shuffled[i + 1]));
            }
            return pairings;
        }

        /// <summary>
        /// Pairs the players against each other in swiss format, e.g. top player plays
        /// against the second, third against the fourth and so on. Avoids creating the 
        /// same pairing twice. See the PairingAlgorithm.txt document for details.
        /// </summary>
        public IEnumerable<Pairing> Swiss(IOrderedEnumerable<KeyValuePair<Player, int>> standings, IList<Pairing> history)
        {
            ValidatePlayerCount(standings.Select(i => i.Key));
            var players = standings.OrderByDescending(i => i.Value).Select(i => i.Key).ToList();
            var pairings = new Stack<Pairing>();
            var timer = Stopwatch.StartNew();
            while (!AllPlayersPaired(pairings, players))
            {
                TryAddPairing(pairings, players, history);
                if (timer.ElapsedMilliseconds > SwissTimeout)
                {
                    throw new TimeoutException("The requested operation took too long to complete.");
                }
            }
            return pairings.ToArray().Reverse();
        }

        #region Private methods

        private static void TryAddPairing(Stack<Pairing> pairings, ICollection<Player> players, IList<Pairing> history)
        {
            var unpaired = players.Where(i => !pairings.Any(j => j.ContainsPlayers(i))).ToList();
            var pairing = GetUniquePairing(players, unpaired, history);
            if (pairing != null)
            {
                pairings.Push(pairing);
                return;
            }
            history.Add(pairings.Peek());
            pairings.Pop();
        }

        private static Pairing GetUniquePairing(ICollection<Player> players, IList<Player> unpaired, IList<Pairing> history)
        {
            for (var i = 0; i < unpaired.Count; i++)
            {
                for (var j = i + 1; j < unpaired.Count; j++)
                {
                    var pairing = new Pairing(unpaired[i], unpaired[j]);
                    if (IsUniquePairingPossible(players, history) && PlayersHavePlayedBefore(history, pairing))
                    {
                        continue;
                    }
                    return pairing;
                }
            }
            return null;
        }

        private static bool AllPlayersPaired(IEnumerable<Pairing> pairings, IEnumerable<Player> players)
        {
            return players.All(i => pairings.Any(p => p.ContainsPlayers(i)));
        }

        private static bool PlayersHavePlayedBefore(IEnumerable<Pairing> history, Pairing pairing)
        {
            return history.Any(p => p.ContainsPlayers(pairing.Player1, pairing.Player2));
        }

        /// <summary>
        /// Check if finding a unique pairing is possible.
        /// </summary>
        /// <param name="players">Player list</param>
        /// <param name="history">Pairing history</param>
        /// <returns>False if any player has played against every other player, true otherwise</returns>
        private static bool IsUniquePairingPossible(ICollection<Player> players, IEnumerable<Pairing> history)
        {
            return players.All(i => history.Count(h => h.ContainsPlayers(i)) < players.Count - 1);
        }

        private static void ValidatePlayerCount(IEnumerable<Player> players)
        {
            if (players.Count() % 2 != 0)
            {
                throw new ArgumentException("Can't create pairings from an odd number of players");
            }
        }

        #endregion
    }
}