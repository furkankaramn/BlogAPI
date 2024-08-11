using System.ComponentModel.DataAnnotations.Schema;

namespace Blog.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public int PostsId { get; set; }
        [ForeignKey(nameof(PostsId))]
        public Post? Post { get; set; }
        public string? MembersId { get; set; }
        [ForeignKey(nameof(MembersId))]
        public Member? Member { get; set; }
        public int? ParentCommentId { get; set; }
        [ForeignKey(nameof(ParentCommentId))]
        public Comment? ParentComment { get; set; }
        public ICollection<Comment>? Replies { get; set; }
        public ICollection<Like>? Likes { get; set; }

        // Beğeni sayısını tutan özellik
        public int LikeCount { get; set; }
    }
}
