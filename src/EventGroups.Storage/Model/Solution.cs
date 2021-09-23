using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventGroups.Storage.Model
{
    public class Solution
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }        

        public IdentityUser Owner { get; set; }

        public List<EventGroup> EventGroups { get; set; }
    }
}
