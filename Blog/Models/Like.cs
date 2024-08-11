using System.ComponentModel.DataAnnotations.Schema;

namespace Blog.Models
{
    public class Like
    {
        public int Id { get; set; }
        public string MembersId { get; set; } = "";

        [ForeignKey(nameof(MembersId))]
        public Member? Member { get; set; }
        public int? PostsId { get; set; }
        [ForeignKey(nameof(PostsId))]
        public Post? Post { get; set; }
        public int? CommentsId { get; set; }
        [ForeignKey(nameof(CommentsId))]
        public Comment? Comment { get; set; }
    }
}
