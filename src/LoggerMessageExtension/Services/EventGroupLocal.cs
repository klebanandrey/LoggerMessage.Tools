﻿using LoggerMessage.Shared;

namespace LoggerMessageExtension.Services
{
    public class EventGroupLocal : IEventGroup
    {
        public string Abbreviation { get; set; }
        public string Description { get; set; }
    }
}
