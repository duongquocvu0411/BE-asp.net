using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webapi.Model
{
    [Table("admins")]
    public class Admin
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(255)]
        public string Username { get; set; }
        [Required]
        [MaxLength(255)]
        public string Password { get; set; } // Lưu mật khẩu đã mã hóa
    }
}
