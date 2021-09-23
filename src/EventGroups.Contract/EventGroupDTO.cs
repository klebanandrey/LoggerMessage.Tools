using System;

namespace EventGroups.Contract
{
    public class EventGroupDTO
    {        
        public string EventGroupAbbr { get; set; }
        
        public string Description { get; set; }

        public Guid SolutionId { get; set; }
        public string SolutionName { get; set; }

        public Guid Owner { get; set; }
    }
}
