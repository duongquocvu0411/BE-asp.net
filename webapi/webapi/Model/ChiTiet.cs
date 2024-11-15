using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webapi.Model
{
    [Table("chitiets")]
    public class ChiTiet
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("sanphams")] // Xác định rằng SanphamsId là khóa ngoại
        [Column("sanphams_id")]
        public int SanphamsId { get; set; }

        [Required]
        [Column("mo_ta_chung")]
        public string? MoTaChung { get; set; }
        [Column("hinh_dang")]
        public string? HinhDang { get; set; }
        [Column("cong_dung")]
        public string? CongDung { get; set; }
        [Column("xuat_xu")]
        public string? XuatXu { get; set; }
        [Column("khoi_luong")]
        public string? KhoiLuong { get; set; }
        [Column("bao_quan")]
        public string? BaoQuan { get; set; }
        [Column("thanh_phan_dinh_duong")]
        public string? ThanhPhanDinhDuong { get; set; }
        [Column("ngay_thu_hoach")]
        public DateTime? NgayThuHoach { get; set; }

        [Column("huong_vi")]
        public string? HuongVi { get; set; }
        [Column("nong_do_duong")]
        public string? NongDoDuong { get; set; }
        [Column("bai_viet")]
        public string? BaiViet { get; set; }

        // Quan hệ ngược với Sanpham
        public virtual Sanpham Sanpham { get; set; }
    }
}

