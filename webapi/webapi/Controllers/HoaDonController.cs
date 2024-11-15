using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webapi.DTO;
using webapi.Model;
using WebApi.Model;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HoaDonController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HoaDonController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách hóa đơn
        /// </summary>
        /// <returns>  Lấy danh sách hóa đơn</returns>

        // GET: api/HoaDon
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetHoaDons()
        {
            // Lấy tất cả các hóa đơn và chi tiết hóa đơn
            var hoadons = await _context.HoaDons
                //.Include(hd => hd.HoaDonChiTiets)
                .ToListAsync();

            var result = new List<object>();

            // Lặp qua các hóa đơn và chi tiết hóa đơn
            foreach (var hd in hoadons)
            {
                //var hoaDonChiTiets = new List<object>();

                //foreach (var ct in hd.HoaDonChiTiets)
                //{
                //    // Lấy thông tin sản phẩm từ SanPhamIds
                //    var sanphamDetails = await GetSanPhamDetails(ct.SanPhamIds);

                //    hoaDonChiTiets.Add(new
                //    {
                //        ct.Id,
                //        ct.BillId,
                //        ct.SanPhamIds,
                //        ct.Price,
                //        ct.Quantity,
                //        SanphamNames = sanphamDetails.SanphamNames, // Tên sản phẩm
                //        SanphamDonViTinh = sanphamDetails.SanphamDonViTinh // Đơn vị tính
                //    });
                //}

                // Thêm hóa đơn và chi tiết vào kết quả
                result.Add(new
                {
                    hd.Id,
                    hd.KhachHangId,
                    hd.TotalPrice,
                    hd.OrderCode,
                    hd.Status,
                    hd.CreatedAt,
                    //HoaDonChiTiets = hoaDonChiTiets
                });
            }

            return Ok(result);
        }

        // Lấy thông tin sản phẩm từ SanPhamIds
        private async Task<(string SanphamNames, string SanphamDonViTinh)> GetSanPhamDetails(string sanPhamIds)
        {
            // Loại bỏ dấu ngoặc vuông và tách chuỗi thành các ID sản phẩm
            var ids = sanPhamIds.Trim('[', ']').Split(',').Select(int.Parse).ToList();

            // Truy vấn thông tin sản phẩm từ database
            var sanphams = await _context.Sanpham
                .Where(sp => ids.Contains(sp.Id))
                .Select(sp => new
                {
                    sp.Tieude, // Lấy Tên sản phẩm
                    sp.DonViTinh // Lấy Đơn vị tính
                })
                .ToListAsync();

            // Gộp tất cả tên sản phẩm thành một chuỗi (nếu có nhiều sản phẩm)
            string sanphamNames = string.Join(", ", sanphams.Select(sp => sp.Tieude));

            // Lấy tất cả đơn vị tính, sẽ lấy đơn vị tính đầu tiên hoặc gộp lại nếu có nhiều đơn vị tính
            string donViTinh = sanphams.Select(sp => sp.DonViTinh).FirstOrDefault(); // Lấy đơn vị tính đầu tiên

            // Trả về tên sản phẩm và đơn vị tính dưới dạng chuỗi đơn giản
            return (sanphamNames, donViTinh);
        }


        /// <summary>
        ///  Thêm mới hóa đơn 
        /// </summary>
        /// <returns> Thêm mới hóa đơn  </returns>

        // POST: api/HoaDon
        [HttpPost]
        public async Task<ActionResult> CreateHoaDon(HoadonDTO.HoaDonDto hoaDonDto)
        {
            var orderCode = GenerateOrderCode();

            // Tính tổng giá trị của hóa đơn
            var totalPrice = 0m;
            for (int i = 0; i < hoaDonDto.SanphamIds.Count; i++)
            {
                var sanpham = await _context.Sanpham.FindAsync(hoaDonDto.SanphamIds[i]);
                if (sanpham != null)
                {
                    totalPrice += (sanpham.Giatien ?? 0) * hoaDonDto.Quantities[i];
                }
            }

            // Tạo hóa đơn mới
            var bill = new HoaDon
            {
                KhachHangId = hoaDonDto.KhachHangId,
                TotalPrice = totalPrice,
                OrderCode = orderCode,
                Status = "Chờ xử lý",
                CreatedAt = DateTime.Now
            };
            _context.HoaDons.Add(bill);
            await _context.SaveChangesAsync();

            // Tạo chi tiết hóa đơn
            for (int i = 0; i < hoaDonDto.SanphamIds.Count; i++)
            {
                var sanpham = await _context.Sanpham.FindAsync(hoaDonDto.SanphamIds[i]);
                if (sanpham != null)
                {
                    var chiTiet = new HoaDonChiTiet
                    {
                        BillId = bill.Id,
                        SanPhamIds = hoaDonDto.SanphamIds[i].ToString(),
                        Price = (sanpham.Giatien ?? 0) * hoaDonDto.Quantities[i],
                        Quantity = hoaDonDto.Quantities[i]
                    };
                    _context.HoaDonChiTiets.Add(chiTiet);
                }
            }
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đơn hàng đã được tạo", order_code = orderCode, bill });
        }


        /// <summary>
        /// Tra cứu theo mã của hóa đơn
        /// </summary>
        /// <returns>Tra cứu theo mã của hóa đơn</returns>
        // GET: api/HoaDon/TraCuu/{orderCode}
        [HttpGet("TraCuu/{orderCode}")]
        public async Task<ActionResult<object>> GetHoaDonByOrderCode(string orderCode)
        {
            // Tìm hóa đơn dựa trên OrderCode
            var hoaDon = await _context.HoaDons
                .Include(hd => hd.HoaDonChiTiets) // Bao gồm chi tiết hóa đơn
                .FirstOrDefaultAsync(hd => hd.OrderCode == orderCode);

            if (hoaDon == null)
            {
                return NotFound(new { message = "Không tìm thấy hóa đơn với mã đơn hàng này." });
            }

            // Chuẩn bị danh sách chi tiết hóa đơn với tên sản phẩm
            var chiTietHoaDon = new List<object>();

            foreach (var ct in hoaDon.HoaDonChiTiets)
            {
                // Lấy thông tin sản phẩm dựa trên SanPhamIds
                var sanphamId = int.Parse(ct.SanPhamIds);
                var sanpham = await _context.Sanpham.FindAsync(sanphamId);

                chiTietHoaDon.Add(new
                {
                    ct.Id,
                    ct.BillId,
                    SanPhamNames = sanpham?.Tieude, // Lấy tên sản phẩm từ bảng Sanpham
                    SanPhamDonViTinh = sanpham?.DonViTinh, // Lấy đơn vị tính từ bảng Sanpham
                    ct.Price,
                    ct.Quantity
                });
            }

            var result = new
            {
                hoaDon.Id,
                hoaDon.KhachHangId,
                hoaDon.TotalPrice,
                hoaDon.OrderCode,
                hoaDon.Status,
                hoaDon.CreatedAt,
                HoaDonChiTiets = chiTietHoaDon
            };

            return Ok(result);
        }


        /// <summary>
        /// tra cứu đơn và hủy đơn hàng 
        /// </summary>
        /// <returns>tra cứu đơn và hủy đơn hàng </returns>

        // PUT: api/HoaDon/TraCuu/{orderCode}/HuyDon
        [HttpPut("TraCuu/{orderCode}/HuyDon")]
        public async Task<ActionResult> CancelOrder(string orderCode)
        {
            // Tìm hóa đơn dựa trên OrderCode
            var hoaDon = await _context.HoaDons.FirstOrDefaultAsync(hd => hd.OrderCode == orderCode);

            if (hoaDon == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng với mã này." });
            }

            // Kiểm tra trạng thái hiện tại của đơn hàng
            if (hoaDon.Status == "Hủy đơn")
            {
                return BadRequest(new { message = "Đơn hàng đã bị hủy trước đó." });
            }
            if (hoaDon.Status != "Chờ xử lý")
            {
                return BadRequest(new { message = "Đơn hàng đã được xử lý và không thể hủy." });
            }

            // Cập nhật trạng thái đơn hàng thành "Hủy đơn"
            hoaDon.Status = "Hủy đơn";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đơn hàng đã được hủy thành công." });
        }


        /// <summary>
        /// Chỉnh sửa status của hóa đơn
        /// </summary>
        /// <returns> Chỉnh sửa status của hóa đơn </returns>

        // PUT: api/HoaDon/UpdateStatus/{id}
        [HttpPut("UpdateStatus/{id}")]
        public async Task<ActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var bill = await _context.HoaDons.FindAsync(id);
            if (bill == null)
                return NotFound(new { message = "Không tìm thấy đơn hàng" });

            bill.Status = dto.Status;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Trạng thái đơn hàng đã được cập nhật", bill });
        }

        /// <summary>
        /// Lấy danh thu theo tháng {year}/ {moth}
        /// </summary>
        /// <returns> Lấy danh thu theo tháng {year}/ {moth} </returns>

        // GET: api/HoaDon/DoanhThuTheoThang/2024/10
        [HttpGet("DoanhThuTheoThang/{year}/{month}")]
        public async Task<ActionResult<object>> GetDoanhThuTheoThang(int year, int month)
        {
            // Tính tổng doanh thu dựa trên TotalPrice của các hóa đơn trong tháng và năm được chỉ định
            var doanhThu = await _context.HoaDons
                .Where(hd => hd.CreatedAt.Year == year && hd.CreatedAt.Month == month)
                .SumAsync(hd => hd.TotalPrice);

            return Ok(new { Year = year, Month = month, DoanhThu = doanhThu });
        }

        /// <summary>
        /// Lấy danh thu theo ngày hiện tại
        /// </summary>
        /// <returns> Lấy danh thu theo ngày hiện tại </returns>

        [HttpGet("DoanhThuHomNay")]
        public async Task<ActionResult<object>> GetDoanhThuHomNay()
        {
            // Lấy ngày hiện tại và thiết lập mốc thời gian đầu và cuối của ngày
            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);

            // Tính tổng doanh thu của các hóa đơn có ngày tạo là hôm nay
            var doanhThuHomNay = await _context.HoaDons
                .Where(hd => hd.CreatedAt >= today && hd.CreatedAt < tomorrow)
                .SumAsync(hd => hd.TotalPrice);

            return Ok(new { Ngay = today.ToString("yyyy-MM-dd"), TongDoanhThu = doanhThuHomNay });
        }

        /// <summary>
        /// Lấy toàn bộ danh thu của các tháng
        /// </summary>
        /// <returns> Lấy toàn bộ danh thu của các tháng </returns>

        // GET: api/HoaDon/DoanhThuTheoTungThang
        [HttpGet("DoanhThuTheoTungThang")]
        public async Task<ActionResult<IEnumerable<object>>> GetDoanhThuTheoTungThang()
        {
            var doanhThuThang = await _context.HoaDons
                .GroupBy(hd => new { hd.CreatedAt.Year, hd.CreatedAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalRevenue = g.Sum(hd => hd.TotalPrice)
                })
                .OrderBy(res => res.Year)
                .ThenBy(res => res.Month)
                .ToListAsync();

            return Ok(doanhThuThang);
        }

        // Sinh mã đơn hàng duy nhất
        private string GenerateOrderCode()
        {
            var characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var madonhang = new string(Enumerable.Repeat(characters, 8)
                .Select(s => s[new Random().Next(s.Length)]).ToArray());

            if (_context.HoaDons.Any(hd => hd.OrderCode == madonhang))
                return GenerateOrderCode();

            return madonhang;
        }

        //public class HoaDonDto
        //{
        //    public int KhachHangId { get; set; }
        //    public List<int> SanphamIds { get; set; }
        //    public List<int> Quantities { get; set; }
        //}

        public class UpdateStatusDto
        {
            public string Status { get; set; }
        }
    }
}
