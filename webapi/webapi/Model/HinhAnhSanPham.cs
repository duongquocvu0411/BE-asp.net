using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webapi.Model
{
    [Table("hinhanh_sanpham")]
    public class HinhAnhSanPham
    {
        [Key]
        public int Id { get; set; }
       
        [Column("sanphams_id")]
        public int SanphamsId { get; set; }

        [Required]
        [Column("hinhanh")]
        public string? Hinhanh { get; set; }

        // Quan hệ ngược với Sanpham
        [ForeignKey("SanphamsId")]
        public virtual Sanpham Sanpham { get; set; }
    }
}

