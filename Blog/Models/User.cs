using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Blog.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? ProfilePhotoUrl { get; set; }
        public string? Name { get; set; }
        public string? SurName { get; set; }
        [NotMapped]
        public string? Password { get; set; }
        [NotMapped]
        [Compare(nameof(Password))]
        public string? ConfirmPassword { get; set; }

    }
    public class Member
    {
        [Key]
        public string Id { get; set; } = "";
        [ForeignKey(nameof(Id))]
        public ApplicationUser? ApplicationUser { get; set; }
        public bool Banned { get; set; }
        public ICollection<Post>? Posts { get; set; }
        public ICollection<Comment>? Comments { get; set; }
        public ICollection<Like>? Likes { get; set; }

    }
    public class Admin
    {
        [Key]
        public string Id { get; set; } = "";
        [ForeignKey(nameof(Id))]
        public ApplicationUser? ApplicationUser { get; set; }
    }
}
