using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LostAndFound.Models
{
    public class Response<Type> where Type : class
    {
        public Type Result { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }

    }
}
