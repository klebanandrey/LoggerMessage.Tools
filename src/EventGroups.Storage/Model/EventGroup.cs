using System;
using System.ComponentModel.DataAnnotations;
using EventGroups.Common;
using LoggerMessages.Common;

namespace EventGroups.Storage.Model
{
    public class EventGroup : ILoggerGroup
    {
        public EventGroup()
        {
            Oid = Guid.NewGuid();
        }

        [Key]
        public Guid Oid { get; set; }

        [MaxLength(Constants.AbbrLength)]
        public string EventGroupAbbr { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public Guid SolutionId { get; set; }
        public Solution Solution { get; set; }
    }
}
