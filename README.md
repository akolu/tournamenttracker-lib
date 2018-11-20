# tournamenttracker-lib #

This is a class library that contains logic for managing records of tournaments of various kinds. The logic has been generated with Swiss-style tournaments in mind, however, randomizing is also an option.

The user can create tournaments with a desired number of rounds, set a player list, generate pairings for rounds (randomized or Swiss) and set scores for individual pairings. The swiss logic keeps track of pairings that have already been played to avoid the same pairing happening twice (unless it is not possible, due to a certain number of players / rounds). A thorough explanation of the swiss pairing logic can be seen <a href="https://github.com/akukolu/tournamenttracker-lib/blob/master/src/tournamenttracker.lib/Documents/PairingAlgorithm.txt">here</a>.

