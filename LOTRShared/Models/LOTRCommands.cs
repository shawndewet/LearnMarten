namespace LOTRShared.Commands
{
    public class StartQuestCommand
    {
        public string QuestName { get; set; }
        public string Location { get; set; }
        public JoinQuestCommand InitialMembers { get; set; }
    }

    public class ArriveAtLocationCommand
    {
        public string Location { get; set; }

        public int Day { get; set; }

    }

    public class JoinQuestCommand
    {
        public int Day { get; set; }

        //public string Location { get; set; }

        public string[] Members { get; set; }

    }

    public class LeaveQuestCommand
    {
        public int Day { get; set; }

        public string[] Members { get; set; }

    }
    public class SlayCharactersCommand
    {
        public int Day { get; set; }

        public string[] Characters { get; set; }

    }

}
