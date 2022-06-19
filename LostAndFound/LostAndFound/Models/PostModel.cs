using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LostAndFound.Models
{
    public class PostModel
    {
        public string Id { get; set; }
        public string Comment { get; set; }
        public string PostType { get; set; }
        public string Date { get; set; }
        public bool IsMatched { get; set; }
        public string UserPostNumber { get; set; }

        public List<string> Images { get; set; }
    }
}
