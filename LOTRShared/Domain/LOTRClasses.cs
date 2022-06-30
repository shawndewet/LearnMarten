using Baseline;
using Marten.Events.Aggregation;

namespace LOTRShared.Domain
{
    public class ArrivedAtLocation
    {
        public Guid QuestId { get; set; }

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

        public MembersJoined(Guid questId, int day, string location, params string[] members)
        {
            Day = day;
            Location = location;
            Members = members;
            QuestId = questId;
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

    public class CharactersSlayed
    {
        public CharactersSlayed()
        {
        }

        public CharactersSlayed(Guid questId, int day, string location, params string[] characters)
        {
            Day = day;
            Location = location;
            Characters = characters;
            QuestId = questId;
        }

        public Guid QuestId { get; set; }

        public int Day { get; set; }

        public string Location { get; set; }

        public string[] Characters { get; set; }

        public override string ToString()
        {
            return $"Characters {Characters.Join(", ")} slayed at {Location} on Day {Day}";
        }

        protected bool Equals(MembersJoined other)
        {
            return QuestId.Equals(other.QuestId) && Day == other.Day && Location == other.Location && Characters.SequenceEqual(other.Members);
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
            return HashCode.Combine(QuestId, Day, Location, Characters);
        }
    }

    public class QuestStarted
    {
        public string Name { get; set; }

        public string Location { get; set; }

        public Guid Id { get; set; }

        public override string ToString()
        {
            return $"Quest {Name} started at {Location}";
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
        public Guid QuestId { get; set; }

        public int Day { get; set; }

        public string Location { get; set; }

        public string[] Members { get; set; }

        public override string ToString()
        {
            return $"Members {Members.Join(", ")} departed at {Location} on Day {Day}";
        }
    }

    public class Quest
    {
        public List<string> Members { get; set; } = new();
        public IList<string> Slayed { get; } = new List<string>();
        public string Name { get; set; }

        // In this particular case, this is also the stream id for the quest events
        public Guid Id { get; set; }

        public int DaysIn { get; set; }

        public string Location { get; set; }

        //// These methods take in events and update the QuestParty
        //// only started and arrived change the Location...others also change the DaysIn
        //public void Apply(QuestStarted started)
        //{
        //    Name = started.Name;
        //    Location = started.Location;
        //}

        //public void Apply(MembersJoined joined)
        //{
        //    Members.Fill(joined.Members);
        //    DaysIn = joined.Day;
        //}

        //public void Apply(ArrivedAtLocation arrived)
        //{
        //    Location = arrived.Location;
        //    DaysIn = arrived.Day;
        //}

        //public void Apply(MembersDeparted departed)
        //{
        //    Members.RemoveAll(x => departed.Members.Contains(x));
        //    DaysIn = departed.Day;
        //}

        //public void Apply(CharactersSlayed slayed)
        //{
        //    Slayed.Fill(slayed.Characters);
        //    DaysIn = slayed.Day;
        //}

        public override string ToString()
        {
            return $"Quest party '{Name}' has members {Members.Join(", ")}";
        }
    }

    public class QuestProjection : SingleStreamAggregation<Quest>
    {
     
        // These methods take in events and update the QuestParty
        // only started and arrived change the Location...others also change the DaysIn
        public void Apply(QuestStarted started, Quest quest)
        {
            quest.Name = started.Name;
            quest.Location = started.Location;
        }

        public void Apply(MembersJoined joined, Quest quest)
        {
            quest.Members.Fill(joined.Members);
            quest.DaysIn = joined.Day;
        }

        public void Apply(ArrivedAtLocation arrived, Quest quest)
        {
            quest.Location = arrived.Location;
            quest.DaysIn = arrived.Day;
        }

        public void Apply(MembersDeparted departed, Quest quest)
        {
            quest.Members.RemoveAll(x => departed.Members.Contains(x));
            quest.DaysIn = departed.Day;
        }

        public void Apply(CharactersSlayed slayed, Quest quest)
        {
            quest.Slayed.Fill(slayed.Characters);
            quest.DaysIn = slayed.Day;
        }

    }

}
