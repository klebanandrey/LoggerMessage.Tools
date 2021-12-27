using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoggerMessageTools.Exceptions
{
    public class EditorException : Exception
    {
        public EditorException(string message) :base(message)
        {}
    }
}
