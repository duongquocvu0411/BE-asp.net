using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Model
{
    [Table("hoadons")]
    public class HoaDon
    {
        public int Id { get; set; }

        [Required]
        [Column("khachhang_id")]
        public int KhachHangId { get; set; }
        [Column("total_price")]
        public decimal TotalPrice { get; set; }
        [Column("order_code")]
        public string OrderCode { get; set; }
        [Column("status")]
        public string Status { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        // Định nghĩa quan hệ với Khách Hàng
        public KhachHang KhachHang { get; set; }

        // Định nghĩa quan hệ một-nhiều với HoaDonChiTiet
        public ICollection<HoaDonChiTiet> HoaDonChiTiets { get; set; }
    }
}
