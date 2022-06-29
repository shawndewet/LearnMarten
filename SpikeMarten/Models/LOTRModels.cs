namespace SpikeMarten.Models
{
    public class StartQuestModel
    {
        public string QuestName { get; set; }
        public JoinQuestModel InitialMembers { get; set; }
    }

    public class JoinQuestModel
    {
        public int Day { get; set; }

        public string Location { get; set; }

        public string[] Members { get; set; }

    }
}
