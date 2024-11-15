using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using webapi.Model;

namespace WebApi.Model
{
    [Table("danhgiakhachhangs")]
    public class DanhGiaKhachHang
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Sanpham")]
        [Column("sanphams_id")]
        public int SanphamsId { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("ho_ten")]
        public string HoTen { get; set; }

        [MaxLength(255)]
        [Column("tieude")]
        public string TieuDe { get; set; }

        [Range(1, 5)]
        [Column("so_sao")]
        public int SoSao { get; set; }

        [Required]
        [Column("noi_dung")]
        public string NoiDung { get; set; }

        public Sanpham? Sanpham { get; set; } // Navigation property for `Sanpham`
    }
}
