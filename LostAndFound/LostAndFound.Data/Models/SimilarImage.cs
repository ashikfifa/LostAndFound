using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace LostAndFound.Data.Models
{
    public class SimilarImage
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        public string LostImageId { get; set; }
        public Image LostImage { get; set; }


        public string FoundImageId { get; set; }
        public Image FoundImage { get; set; }


        public double Accuracy { get; set; }


    }
}
