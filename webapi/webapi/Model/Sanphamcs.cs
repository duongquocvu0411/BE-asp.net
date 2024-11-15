using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using webapi.Model;
using WebApi.Model;

namespace webapi.Model
{
    [Table("sanphams")]
    public class Sanpham
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("tieude")]
        public string? Tieude { get; set; }  // Tên sản phẩm, có thể nullable nếu cơ sở dữ liệu cho phép NULL

        [Column("giatien")]
        public decimal? Giatien { get; set; } // Giá sản phẩm, nullable nếu cơ sở dữ liệu cho phép NULL

        [Column("hinhanh")]
        public string? Hinhanh { get; set; }  // Hình ảnh, nullable nếu cơ sở dữ liệu cho phép NULL

        [Column("trangthai")]
        public string? Trangthai { get; set; } // Trạng thái của sản phẩm, nullable nếu cơ sở dữ liệu cho phép NULL

        [Column("don_vi_tinh")]
        public string? DonViTinh { get; set; } // Đơn vị tính của sản phẩm, nullable nếu cơ sở dữ liệu cho phép NULL

        // Foreign Key cho Danhmucsanpham
        [ForeignKey("Danhmucsanpham")]
        [Column("danhmucsanpham_id")]
        public int DanhmucsanphamId { get; set; }

        // Liên kết với đánh giá khách hàng
        public virtual ICollection<DanhGiaKhachHang> Danhgiakhachhangs { get; set; } = new List<DanhGiaKhachHang>();

        // Liên kết với Danhmucsanpham
        [ForeignKey("DanhmucsanphamId")]
        public virtual Danhmucsanpham? Danhmucsanpham { get; set; }

        // Liên kết với chi tiết sản phẩm
        public virtual ChiTiet? ChiTiet { get; set; }

        // Liên kết với hình ảnh sản phẩm
        public virtual ICollection<HinhAnhSanPham> Images { get; set; } = new List<HinhAnhSanPham>();

    }
}
