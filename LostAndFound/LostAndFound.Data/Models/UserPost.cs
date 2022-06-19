using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace LostAndFound.Data.Models
{
    public class UserPost
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string PhoneNo { get; set; }
        public string Comment { get; set; }
        public string OTP { get; set; }
        public DateTime OTPCreatedAt { get; set; }
        public bool IsVerified { get; set; }
        public string PostType { get; set; }
        public DateTime Date { get; set; }
        public bool IsMatched { get; set; }


        public bool IsUnlocked { get; set; }
        public string UnlockOTP { get; set; }
        public DateTime? UnlockOTPCreatedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserPostNumber { get; set; }

    }
}
