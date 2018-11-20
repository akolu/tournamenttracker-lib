namespace TournamentTracker.Models
{
    public class Player
    {
        public string Name { get; set; }

        public override bool Equals(System.Object other)
        {
            var p = other as Player;
            return p != null && Equals(p);
        }

        protected bool Equals(Player other)
        {
            return string.Equals(Name, other.Name);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public static Player CreateEmpty()
        {
            return new Player
            {
                Name = string.Empty
            };
        }
    }
}