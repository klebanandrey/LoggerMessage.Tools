namespace LoggerMessage.Shared
{
    public class ViewParams
    {
        public ViewParams(string abbr, Level level, string template)
        {
            OldAbbr = abbr;
            OldLevel = level;
            OldTemplate = template;
        }

        public string OldAbbr { get;}
        public Level OldLevel { get;}

        public string OldTemplate { get; }

        public string NewAbbr { get; set; }
        public Level NewLevel { get; set; }

        public string NewTemplate { get; set; }
    }
}
