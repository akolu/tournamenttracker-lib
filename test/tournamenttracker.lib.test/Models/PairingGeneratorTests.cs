using System;
using System.Collections.Generic;
using System.Linq;
using TournamentTracker.Models;
using Xunit;

namespace TournamentTrackerTests.Models
{
    public class PairingGeneratorTests
    {
        private readonly IPairingGenerator _generator = new PairingGenerator();
        private static readonly Player P1 = new Player { Name = "P1" };
        private static readonly Player P2 = new Player { Name = "P2" };
        private static readonly Player P3 = new Player { Name = "P3" };
        private static readonly Player P4 = new Player { Name = "P4" };
        private static readonly Player P5 = new Player { Name = "P5" };
        private static readonly Player P6 = new Player { Name = "P6" };

        [Fact]
        public void AfterShuffling_PairingsContainSamePlayers()
        {
            var p1 = new[] { P1, P2, P3, P4 };
            var p2 = new List<Player>();
            var pairings = _generator.Shuffle(p1);
            foreach (var i in pairings)
            {
                p2.Add(i.Player1);
                p2.Add(i.Player2);
            }

            Assert.Empty(p1.Where(item => item != null && !p2.Contains(item)));
            Assert.Empty(p2.Where(item => item != null && !p1.Contains(item)));
        }

        [Fact]
        public void Shuffle_ThrowsExceptionOnOddNumberOfPlayers()
        {
            var p1 = new[] { P1, P2, P3 };
            Assert.Throws<ArgumentException>(() => _generator.Shuffle(p1));
        }

        [Fact]
        public void Swiss_CreatesPairingsByScore()
        {
            var p1 = new Dictionary<Player, int>
            {
                { P1, 4 },
                { P2, 1 },
                { P3, 18 },
                { P4, 4 },
                { P5, 12 },
                { Player.CreateEmpty(), 0 }
            };
            var p2 = new List<Player>();
            var pairings = _generator.Swiss(p1.OrderBy(i => i.Value), new List<Pairing>());
            foreach (var i in pairings)
            {
                p2.Add(i.Player1);
                p2.Add(i.Player2);
            }
            var orderedPlayerList = p2.Where(p => p != null).ToList();

            Assert.Empty(p1.Select(p => p.Key).ToList().Where(item => !orderedPlayerList.Contains(item)));
            Assert.Empty(orderedPlayerList.Where(item => !p1.Select(p => p.Key).Contains(item)));
            Assert.Equal(p1.OrderByDescending(i => i.Value).Select(i => i.Key), orderedPlayerList);
        }

        [Fact]
        public void Swiss_ThrowsExceptionOnOddNumberOfPlayers()
        {
            var p1 = new Dictionary<Player, int>
            {
                { P1, 0 },
                { P2, 0 },
                { P3, 0 }
            };
            Assert.Throws<ArgumentException>(() => _generator.Swiss(p1.OrderBy(i => i.Value), new List<Pairing>()));
        }

        /// <summary>
        /// Players play three rounds according to the following table. 
        /// Total score before round is displayed in brackets.
        /// See PairingAlgorithm.txt for details.
        /// </summary>
        [Fact]
        public void Swiss_AvoidsCreatingSamePairingTwice()
        {
            var tournament = new Tournament(new PairingGenerator(), 3) { Players = new[] { P1, P2, P3, P4, P5, P6 } };

            var round = tournament.Rounds.First();
            round.Pairings = tournament.SwissPairings().ToList();
            round.Pairings.ElementAt(0).SetScore(11, 9); //P1 vs P2
            round.Pairings.ElementAt(1).SetScore(0, 20); //P3 vs P4
            round.Pairings.ElementAt(2).SetScore(10, 10); //P5 vs P6

            var round2 = tournament.Rounds.ElementAt(1);
            round2.Pairings = tournament.SwissPairings().ToList();
            round2.Pairings.ElementAt(0).SetScore(1, 19); //P4 vs P1
            round2.Pairings.ElementAt(1).SetScore(0, 20); //P5 vs P2
            round2.Pairings.ElementAt(2).SetScore(0, 20); //P6 vs P3

            var round3 = tournament.Rounds.ElementAt(2);
            round3.Pairings = tournament.SwissPairings().ToList();
            VerifyPairing(round3.Pairings.ElementAt(0), P1, P3);
            VerifyPairing(round3.Pairings.ElementAt(1), P2, P6);
            VerifyPairing(round3.Pairings.ElementAt(2), P4, P5);
        }

        /// <summary>
        /// Four Players play four rounds, but unique pairing is only possible for the first three rounds.
        /// Total score before round is displayed in brackets. Note that the last player is EMPTY
        ///
        /// Round 1:                  Round 2:                     Round 3:                     Round 4:
        ///   P1 (0)  vs  P2 (0)        P1 (20)  vs  P3 (20)         P3 (40)  vs  P2 (20)         P1 (43)  vs  P3 (43)
        ///   P3 (0)  vs  EMPTY         P2 (0)   vs  EMPTY           P1 (23)  vs  EMPTY           P2 (34)  vs  EMPTY
        /// </summary>
        [Fact]
        public void Swiss_CreatesSamePairingsIfUniqueNotPossible()
        {
            var tournament = new Tournament(new PairingGenerator(), 4) { Players = new[] { P1, P2, P3 } };
            var empty = Player.CreateEmpty();

            var round = tournament.Rounds.First();
            round.Pairings = tournament.SwissPairings().ToList();
            round.Pairings.ElementAt(0).SetScore(20, 0);
            round.Pairings.ElementAt(1).SetScore(20, 0);

            var round2 = tournament.Rounds.ElementAt(1);
            round2.Pairings = tournament.SwissPairings().ToList();
            round2.Pairings.ElementAt(0).SetScore(3, 17);
            round2.Pairings.ElementAt(1).SetScore(20, 0);

            var round3 = tournament.Rounds.ElementAt(2);
            round3.Pairings = tournament.SwissPairings().ToList();
            round3.Pairings.ElementAt(0).SetScore(6, 14);
            round3.Pairings.ElementAt(1).SetScore(20, 0);

            var round4 = tournament.Rounds.ElementAt(3);
            round4.Pairings = tournament.SwissPairings().ToList();

            VerifyPairing(round4.Pairings.ElementAt(0), P1, P3);
            VerifyPairing(round4.Pairings.ElementAt(1), P2, empty);
        }

        private static void VerifyPairing(Pairing pairing, Player p1, Player p2)
        {
            Assert.Equal(p1, pairing.Player1);
            Assert.Equal(p2, pairing.Player2);
        }
    }
}
