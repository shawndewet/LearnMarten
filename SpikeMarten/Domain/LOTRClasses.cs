using Baseline;

namespace SpikeMarten.Domain
{
    public class ArrivedAtLocation
    {
        public int Day { get; set; }

        public string Location { get; set; }

        public override string ToString()
        {
            return $"Arrived at {Location} on Day {Day}";
        }
    }

    public class MembersJoined
    {
        public MembersJoined()
        {
        }

        public MembersJoined(int day, string location, params string[] members)
        {
            Day = day;
            Location = location;
            Members = members;
        }

        public Guid QuestId { get; set; }

        public int Day { get; set; }

        public string Location { get; set; }

        public string[] Members { get; set; }

        public override string ToString()
        {
            return $"Members {Members.Join(", ")} joined at {Location} on Day {Day}";
        }

        protected bool Equals(MembersJoined other)
        {
            return QuestId.Equals(other.QuestId) && Day == other.Day && Location == other.Location && Members.SequenceEqual(other.Members);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MembersJoined)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(QuestId, Day, Location, Members);
        }
    }

    public class QuestStarted
    {
        public string Name { get; set; }
        public Guid Id { get; set; }

        public override string ToString()
        {
            return $"Quest {Name} started";
        }

        protected bool Equals(QuestStarted other)
        {
            return Name == other.Name && Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((QuestStarted)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Id);
        }
    }

    public class QuestEnded
    {
        public string Name { get; set; }
        public Guid Id { get; set; }

        public override string ToString()
        {
            return $"Quest {Name} ended";
        }
    }

    public class MembersDeparted
    {
        public Guid Id { get; set; }

        public Guid QuestId { get; set; }

        public int Day { get; set; }

        public string Location { get; set; }

        public string[] Members { get; set; }

        public override string ToString()
        {
            return $"Members {Members.Join(", ")} departed at {Location} on Day {Day}";
        }
    }

    public class MembersEscaped
    {
        public Guid Id { get; set; }

        public Guid QuestId { get; set; }

        public string Location { get; set; }

        public string[] Members { get; set; }

        public override string ToString()
        {
            return $"Members {Members.Join(", ")} escaped from {Location}";
        }
    }

    public class QuestParty
    {
        public List<string> Members { get; set; } = new();
        public IList<string> Slayed { get; } = new List<string>();
        public string Key { get; set; }
        public string Name { get; set; }

        // In this particular case, this is also the stream id for the quest events
        public Guid Id { get; set; }

        // These methods take in events and update the QuestParty
        public void Apply(MembersJoined joined) => Members.Fill(joined.Members);
        public void Apply(MembersDeparted departed) => Members.RemoveAll(x => departed.Members.Contains(x));
        public void Apply(QuestStarted started) => Name = started.Name;

        public override string ToString()
        {
            return $"Quest party '{Name}' is {Members.Join(", ")}";
        }
    }

}
