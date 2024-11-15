using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using webapi.Model;

namespace WebApi.Model
{
    [Table("hoadonchitiets")]
    public class HoaDonChiTiet
    {
        public int Id { get; set; }

        [Required]
        [Column("bill_id")]
        public int BillId { get; set; }
        [Column("sanpham_ids")]
        public string SanPhamIds { get; set; }
        [Column("price")]
        public decimal Price { get; set; }
        [Column("quantity")]
        public int Quantity { get; set; }
        [NotMapped]
        public string SanphamNames { get; set; }

        [NotMapped]
        public List<SanPhamDetail> SanphamDonViTinh { get; set; }

        // Định nghĩa quan hệ với HoaDon
        public HoaDon HoaDon { get; set; }
       
    }
}
