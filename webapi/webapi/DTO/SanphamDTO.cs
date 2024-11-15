using webapi.Controllers;

namespace webapi.DTO
{
    public class SanphamDTO
    {
        public class SanphamUpdateRequest
        {
            public string Tieude { get; set; }
            public decimal Giatien { get; set; }
            public string Trangthai { get; set; }
            public string DonViTinh { get; set; }
            public int DanhmucsanphamId { get; set; }
            public IFormFile? Hinhanh { get; set; } // Main image, optional for PUT
            public IFormFileCollection? Images { get; set; } // Secondary images
            public ChiTietDto? ChiTiet { get; set; } // Product details
        }


        public class SanphamCreateRequest
        {
            public string Tieude { get; set; }
            public decimal Giatien { get; set; } // Make this nullable
                                                 //public decimal? Giatien { get; set; } // Make this nullable
            public string Trangthai { get; set; }
            public string DonViTinh { get; set; }
            public int DanhmucsanphamId { get; set; }
            //public int? DanhmucsanphamId { get; set; } // Make this nullable
            public IFormFile Hinhanh { get; set; } // Main image
            public IFormFileCollection? Images { get; set; } // Secondary images
            public ChiTietDto? ChiTiet { get; set; } // Product details
        }
        public class ChiTietDto
        {
            public string? MoTaChung { get; set; }
            public string? HinhDang { get; set; }
            public string? CongDung { get; set; }
            public string? XuatXu { get; set; }
            public string? KhoiLuong { get; set; }
            public string? BaoQuan { get; set; }
            public string? ThanhPhanDinhDuong { get; set; }
            public DateTime? NgayThuHoach { get; set; }
            public string? HuongVi { get; set; }
            public string? NongDoDuong { get; set; }
            public string? BaiViet { get; set; }
        }
    }
}
