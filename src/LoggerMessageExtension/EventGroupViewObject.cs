using Microsoft.CodeAnalysis;
using System;
using System.IO;

namespace LoggerMessageExtension.Scopes
{
    public class EventGroupViewObject
    {
        public string Abbreviation { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return $"{Abbreviation}:{Description}";
        }
    }
}
