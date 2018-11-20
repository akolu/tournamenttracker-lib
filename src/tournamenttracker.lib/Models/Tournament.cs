using System;
using System.Collections.Generic;
using System.Linq;

namespace TournamentTracker.Models
{
    public class Tournament
    {
        private readonly IPairingGenerator _pairingGenerator;
        private IEnumerable<Player> _players;

        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<Round> Rounds { get; private set; }

        public IEnumerable<Player> Players
        {
            get { return _players; }
            set
            {
                var players = value.ToList();
                if (value.Count() % 2 != 0)
                {
                    players.Add(Player.CreateEmpty());
                }
                _players = players;
                Rounds = CreateEmptyRounds(Rounds.Count());
            }
        }

        public Tournament(IPairingGenerator generator, int rounds)
        {
            if (rounds <= 0)
            {
                throw new ArgumentException("Tournament should have at least one round");
            }
            _pairingGenerator = generator;
            Rounds = CreateEmptyRounds(rounds);
        }

        public IEnumerable<Pairing> RandomizePairings()
        {
            return _pairingGenerator.Shuffle(Players.ToList());
        }

        public IEnumerable<Pairing> SwissPairings()
        {
            return _pairingGenerator.Swiss(GetPlayerStandings(), GetPairingHistory().ToList()).ToList();
        }

        public IOrderedEnumerable<KeyValuePair<Player, int>> GetPlayerStandings()
        {
            var standings = Players.ToDictionary(i => i, i => 0);
            foreach (var k in Rounds.SelectMany(j => j.GetStandings()))
            {
                standings[k.Key] += k.Value;
            }
            return standings.OrderByDescending(i => i.Value);
        }

        private IEnumerable<Pairing> GetPairingHistory()
        {
            return Rounds.SelectMany(i => i.Pairings).ToList();
        }

        #region Private methods

        private static IEnumerable<Round> CreateEmptyRounds(int count)
        {
            var rounds = new Round[count];
            for (var i = 0; i < count; i++)
            {
                rounds[i] = new Round();
            }
            return rounds;
        }

        #endregion
    }
}