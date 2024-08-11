using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Blog.Models
{
    public class Post
    {
        public int Id { get; set; }
        [StringLength(2000)]
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string MembersId { get; set; } = "";
        [JsonIgnore]
        [ForeignKey(nameof(MembersId))]
        public Member? Member { get; set; }
        public ICollection<Comment>? Comments { get; set; }
        public ICollection<Like>? Likes { get; set; }

        // Beğeni sayısını tutan özellik
        public int LikeCount { get; set; }
    }
}
