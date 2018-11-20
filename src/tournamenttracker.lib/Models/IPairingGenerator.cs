using System.Collections.Generic;
using System.Linq;

namespace TournamentTracker.Models
{
    public interface IPairingGenerator
    {
        IEnumerable<Pairing> Shuffle(IList<Player> players);

        IEnumerable<Pairing> Swiss(IOrderedEnumerable<KeyValuePair<Player, int>> standings, IList<Pairing> history);
    }
}
