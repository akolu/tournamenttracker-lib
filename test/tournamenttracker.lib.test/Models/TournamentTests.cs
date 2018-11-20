using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using TournamentTracker.Models;
using Xunit;

namespace TournamentTrackerTests.Models
{
    public class TournamentTests
    {
        private static readonly Mock<IPairingGenerator> PairingGenerator = new Mock<IPairingGenerator>();

        private static readonly Player P1 = new Player { Name = "P1" };
        private static readonly Player P2 = new Player { Name = "P2" };
        private static readonly Player P3 = new Player { Name = "P3" };
        private static readonly Player P4 = new Player { Name = "P4" };

        #region Tournament tests

        [Fact]
        public void CreatingTournamentWithZeroOrNegativeRounds_ThrowsArgumentExecption()
        {
            Assert.Throws<ArgumentException>(() => CreateTournament(-1));
        }

        [Fact]
        public void CreatingTournament_InitializesEmptyUniqueRounds()
        {
            const int rounds = 5;
            var tournament = CreateTournament(rounds);

            Assert.Equal(rounds, tournament.Rounds.Count());
            tournament.Rounds.ToList().ForEach(i => Assert.True(i.GetType() == typeof(Round)));
            tournament.Rounds.ToList().ForEach(i => Assert.Equal(1, tournament.Rounds.Count(n => n == i)));
        }

        [Fact]
        public void CreatingTournamentWithOddNumberOfPlayers_CreatesEmptyPlayerToPlayerList()
        {
            var tournament = CreateTournament(1);
            tournament.Players = new[] { P1, P2, P3 };

            Assert.Equal(4, tournament.Players.Count());
            Assert.Contains(Player.CreateEmpty(), tournament.Players.ToList());
        }

        [Fact]
        public void RandomizePairings_CallsPairingGeneratorShuffle()
        {
            var tournament = CreateTournament(1);
            tournament.Players = new[] { P1, P2, P3, P4 };
            PairingGenerator.Setup(pg => pg.Shuffle(It.IsAny<List<Player>>())).Returns(new List<Pairing>());

            tournament.RandomizePairings();
            PairingGenerator.Verify(pg => pg.Shuffle(It.IsAny<List<Player>>()), Times.Once);
        }

        [Fact]
        public void SwissPairings_CallsPairingGeneratorSwiss()
        {
            var tournament = CreateTournament(1);
            tournament.Players = new[] { P1, P2, P3, P4 };
            PairingGenerator.Setup(
                pg => pg.Swiss(It.IsAny<IOrderedEnumerable<KeyValuePair<Player, int>>>(), It.IsAny<IList<Pairing>>()))
                .Returns(new List<Pairing>())
                .Verifiable();

            tournament.SwissPairings();
            PairingGenerator.Verify(
                pg => pg.Swiss(It.IsAny<IOrderedEnumerable<KeyValuePair<Player, int>>>(), It.IsAny<IList<Pairing>>()));
        }

        [Fact]
        public void ModifyingPlayerList_ResetsRounds()
        {
            var tournament = new Tournament(new PairingGenerator(), 1) { Players = new[] { P1, P2 } };
            var round = tournament.Rounds.First();
            round.Pairings = tournament.RandomizePairings();
            Assert.NotNull(tournament.Rounds.First().Pairings);

            tournament.Players = new[] { P1, P2, P3 };
            Assert.NotEqual(round, tournament.Rounds.First());
            Assert.Empty(tournament.Rounds.First().Pairings);
        }

        /// <summary>
        /// Four players play two rounds, scoring as follows (round scores in brackets):
        /// 
        /// Round 1:                     Round 2                      Total
        ///   P1 (12)  vs  P2 (8)          P4 (13)  vs  P1 (7)          P1 = 19      P3 = 11
        ///   P3  (1)  vs  P4 (19)         P2 (10)  vs  P3 (10)         P2 = 18      P4 = 32
        /// </summary>                                                                       
        [Fact]
        public void GetPlayerStandings_CalculatesScoresFromAllRounds()
        {
            var tournament = new Tournament(new PairingGenerator(), 3)
            {
                Players = new List<Player> { P1, P2, P3, P4 }
            };

            var pairings = tournament.SwissPairings().ToList();
            pairings[0].SetScore(12, 8);
            pairings[1].SetScore(1, 19);
            tournament.Rounds.ElementAt(0).Pairings = pairings;

            pairings = tournament.SwissPairings().ToList();
            pairings[0].SetScore(13, 7);
            pairings[1].SetScore(10, 10);
            tournament.Rounds.ElementAt(1).Pairings = pairings;

            var standings = tournament.GetPlayerStandings();
            VerifyPlayersStandingWithScore(standings.ElementAt(0), P4, 32);
            VerifyPlayersStandingWithScore(standings.ElementAt(1), P1, 19);
            VerifyPlayersStandingWithScore(standings.ElementAt(2), P2, 18);
            VerifyPlayersStandingWithScore(standings.ElementAt(3), P3, 11);
        }

        #endregion

        #region Round tests

        [Fact]
        public void CreateRound_InitializesRoundWithEmptyPairings()
        {
            var round = new Round();
            Assert.NotNull(round.Pairings);
            Assert.Empty(round.Pairings);
        }

        [Fact]
        public void GetStandings_ReturnsPlayersWithScoresInDescOrder()
        {
            var tournament = new Tournament(new PairingGenerator(), 2) { Players = new[] { P1, P2, P3, P4 } };
            var pairings = tournament.RandomizePairings().ToList();
            pairings[0].SetScore(15, 5);
            pairings[1].SetScore(10, 10);

            PairingGenerator.Setup(
                pg => pg.Swiss(It.IsAny<IOrderedEnumerable<KeyValuePair<Player, int>>>(), It.IsAny<IList<Pairing>>()))
                .Returns(pairings);
            var standings = new Round { Pairings = pairings }.GetStandings().Select(i => i.Value).ToList();

            Assert.Equal(tournament.Players.Count(), standings.Count);
            Assert.Equal(standings, standings.OrderByDescending(i => i));
        }

        /// <summary>
        /// Initial pairings are:
        ///     P1  vs  P2
        ///     P3  vs  P4
        /// After swapping P1 and P4, pairings are:
        ///     P4  vs  P2
        ///     P3  vs  P1
        /// After second swap (P2 and P1), pairings are:
        ///     P4  vs  P1
        ///     P3  vs  P2
        /// </summary>
        [Fact]
        public void Swap_ChangesPlacesOfTwoPlayersInRound()
        {
            var p1 = new Player { Name = "P1" };
            var p2 = new Player { Name = "P2" };
            var p3 = new Player { Name = "P3" };
            var p4 = new Player { Name = "P4" };
            var tournament = new Tournament(new PairingGenerator(), 1) { Players = new[] { p1, p2, p3, p4 } };
            var pairings = tournament.SwissPairings().ToList();
            var round = tournament.Rounds.ElementAt(0);
            round.Pairings = pairings;

            round.Swap(pairings.ElementAt(0).Player1, pairings.ElementAt(1).Player2);
            Assert.Equal(p4, pairings[0].Player1);
            Assert.Equal(p2, pairings[0].Player2);
            Assert.Equal(p3, pairings[1].Player1);
            Assert.Equal(p1, pairings[1].Player2);

            round.Swap(p2, p1);
            Assert.Equal(p4, pairings[0].Player1);
            Assert.Equal(p1, pairings[0].Player2);
            Assert.Equal(p3, pairings[1].Player1);
            Assert.Equal(p2, pairings[1].Player2);
        }

        #endregion

        #region Pairing tests

        [Fact]
        public void SetScore_AddsScoresForBothPlayers()
        {
            var pairing = new Pairing(new Player { Name = "P1" }, new Player { Name = "P2" });
            pairing.SetScore(9, 12);
            Assert.Equal(9, pairing.P1Score);
            Assert.Equal(12, pairing.P2Score);
        }

        [Fact]
        public void ContainsPlayer_ReturnsTrueIfPlayerIsInPairing()
        {
            var players = new[] { P1, P2, P3 };
            var pairing = new Pairing(players[0], players[1]);

            Assert.True(pairing.ContainsPlayers(players[0]));
            Assert.True(pairing.ContainsPlayers(players[1]));
            Assert.False(pairing.ContainsPlayers(players[2]));
            Assert.False(pairing.ContainsPlayers(new Player { Name = "foo" }));
        }

        [Fact]
        public void ContainsPlayers_ReturnsTrueIfBothPlayersExistInPairing()
        {
            var pairing = new Pairing(P1, P2);

            Assert.False(pairing.ContainsPlayers(P1, P3));
            Assert.False(pairing.ContainsPlayers(P3, P2));
            Assert.True(pairing.ContainsPlayers(P1, P2));
            Assert.True(pairing.ContainsPlayers(P2, P1));
        }

        [Fact]
        public void GetOpponent_ReturnsPlayersOpponentOrThrowsException()
        {
            var players = new[] { P1, P2, P3 };
            var tournament = new Tournament(new PairingGenerator(), 1) { Players = players };
            tournament.Rounds.ElementAt(0).Pairings = tournament.SwissPairings();

            Assert.Equal(players[0], tournament.Rounds.ElementAt(0).Pairings.ElementAt(0).GetOpponent(players[1]));
            Assert.Equal(Player.CreateEmpty(), tournament.Rounds.ElementAt(0).Pairings.ElementAt(1).GetOpponent(players[2]));
            Assert.Throws<ArgumentException>(() => tournament.Rounds.ElementAt(0).Pairings.ElementAt(0).GetOpponent(new Player { Name = "P4" }));
        }

        #endregion

        #region Helper methods

        private static Tournament CreateTournament(int numberOfRounds)
        {
            return new Tournament(PairingGenerator.Object, numberOfRounds);
        }

        private static void VerifyPlayersStandingWithScore(KeyValuePair<Player, int> standing, Player player, int score)
        {
            Assert.Equal(player, standing.Key);
            Assert.Equal(score, standing.Value);
        }

        #endregion
    }
}
