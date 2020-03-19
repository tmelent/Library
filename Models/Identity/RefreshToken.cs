using Library.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Models.Identity
{
    public class RefreshToken : IEntity
    {       
        [Key]
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTimeOffset Expiration { get; set; }
        [ForeignKey("UserId")]
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}