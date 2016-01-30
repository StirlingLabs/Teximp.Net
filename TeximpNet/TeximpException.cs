using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeximpNet
{
    public class TeximpException : Exception
    {
        public TeximpException(String message) : base(message) { }

        public TeximpException(String message, Exception innerException) : base(message, innerException) { }
    }
}
