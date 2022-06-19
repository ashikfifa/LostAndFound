using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace LostAndFound.Data.Models
{
    public class Image
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string UserPostId { get; set; }
        public UserPost UserPost { get; set; }
        public string Name { get; set; }
        public bool IsProcessed { get; set; }
        public DateTime? LastSearched { get; set; }
        public string Status { get; set; }
        public bool IsFaceDetected { get; set; }

    }
}
