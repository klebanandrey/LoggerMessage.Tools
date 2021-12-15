namespace LoggerMessage.Shared
{
    public class EventGroupViewObject : IEventGroup
    {
        public string Abbreviation { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return $"{Abbreviation}:{Description}";
        }
    }
}
