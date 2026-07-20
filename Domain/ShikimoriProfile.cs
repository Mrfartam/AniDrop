using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AniDrop.Domain
{
    public class ShikimoriProfile
    {
        [Key]
        [ForeignKey("User")]
        [Required]
        public int UserId {  get; set; }
        [Required]
        public int ShikimoriId { get; set; }
        public string AccessToken {  get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt {  get; set; }
    }
}
