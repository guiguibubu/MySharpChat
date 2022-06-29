using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Core.Utils
{
    public interface IParser<T>
    {
        public T? Parse(string? text);
        public bool TryParse(string? text, out T? parsedObject);
    }
}
