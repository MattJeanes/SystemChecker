using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SystemChecker.Model.Helpers
{
    public class SubCheckException : Exception
    {
        public SubCheckException() { }
        public SubCheckException(string message) : base(message) { }
        public SubCheckException(string message, Exception innerException) : base(message, innerException) { }
    }
}
