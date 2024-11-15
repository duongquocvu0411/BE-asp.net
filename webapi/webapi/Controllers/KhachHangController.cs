using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Model;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhachHangController : ControllerBase
    {
        private readonly AppDbContext _context;

        public KhachHangController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách của Khách hàng và hóa đơn
        /// </summary>
        /// <returns> Lấy danh sách của Khách hàng và hóa đơn</returns>

        // GET: api/KhachHang
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetKhachHangs()
        {
            var khachHangs = await _context.KhachHangs
                .Include(kh => kh.HoaDons)
                  
                .ToListAsync();

            var result = new List<object>();

            // Lặp qua các khách hàng và lấy thông tin của họ cùng hóa đơn chi tiết
            foreach (var kh in khachHangs)
            {
                var hoaDons = new List<object>();

                foreach (var bill in kh.HoaDons)
                {
                   
                    

                    hoaDons.Add(new
                    {
                        bill.Id,
                        bill.KhachHangId,
                        bill.TotalPrice,
                        bill.OrderCode,
                        bill.Status,
                        bill.CreatedAt,
                     
                    });
                }

                result.Add(new
                {
                    kh.Id,
                    kh.Ten,
                    kh.Ho,
                    kh.DiaChiCuThe,
                    kh.ThanhPho,
                    kh.Sdt,
                    kh.EmailDiaChi,
                    kh.GhiChu,
                    HoaDons = hoaDons
                });
            }

            return Ok(result);
        }

        /// <summary>
        /// Xem khách hàng theo id có hóa đơn, hóa đơn chi tiết
        /// </summary>
        /// <returns> xem khách hàng theo id có hóa đơn , hóa đơn chi tiết </returns>

        // GET: api/KhachHang/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetKhachHang(int id)
        {
            var khachHang = await _context.KhachHangs
                .Include(kh => kh.HoaDons)
                    .ThenInclude(hd => hd.HoaDonChiTiets)
                .FirstOrDefaultAsync(kh => kh.Id == id);

            if (khachHang == null)
            {
                return NotFound(new { message = "Không tìm thấy khách hàng với ID này." });
            }

            // Lặp qua các hóa đơn và chi tiết hóa đơn để lấy thông tin sản phẩm
            var hoaDons = khachHang.HoaDons.Select(bill => new
            {
                bill.Id,
                bill.KhachHangId,
                bill.TotalPrice,
                bill.OrderCode,
                bill.Status,
                bill.CreatedAt,
                HoaDonChiTiets = bill.HoaDonChiTiets.Select(async ct =>
                {
                    var sanphamDetails = await GetSanPhamDetails(ct.SanPhamIds);
                    return new
                    {
                        ct.Id,
                        ct.BillId,
                        ct.Price,
                        ct.Quantity,
                        SanphamNames = sanphamDetails.SanphamNames,
                        SanphamDonViTinh = sanphamDetails.SanphamDonViTinh
                    };
                }).Select(task => task.Result).ToList()
            }).ToList();

            var result = new
            {
                khachHang.Id,
                khachHang.Ten,
                khachHang.Ho,
                khachHang.DiaChiCuThe,
                khachHang.ThanhPho,
                khachHang.Sdt,
                khachHang.EmailDiaChi,
                khachHang.GhiChu,
                HoaDons = hoaDons
            };

            return Ok(result);
        }

        /// <summary>
        /// Thêm mới khách hàng
        /// </summary>
        /// <returns> Thêm mới khách hàng</returns>

        // POST: api/KhachHang
        [HttpPost]
        public async Task<ActionResult<KhachHang>> PostKhachHang(KhachHang khachHang)
        {
            _context.KhachHangs.Add(khachHang);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetKhachHang), new { id = khachHang.Id }, khachHang);
        }

        /// <summary>
        /// Xóa khách hàng theo {id} 
        /// </summary>
        /// <returns> Xóa khách hàng theo {id} </returns>

        // DELETE: api/KhachHang/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKhachHang(int id)
        {

            var KhachHang = await _context.KhachHangs.FindAsync(id);

            if(KhachHang == null)
            {
                return NotFound(new { mewssage = " không tìm thấy khách hàng với id này" });
            }

            _context.KhachHangs.Remove(KhachHang);
            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            return Ok(new { mesaage = " Xóa khách hàng thành công" });

        }



        /// <summary>
        /// Lấy tổng số khách hàng mới trong tháng hiện tại
        /// </summary>
        /// <returns> Tổng số khách hàng mới trong tháng hiện tại </returns>

        // GET: api/KhachHang/NewCustomersInMonth
        [HttpGet("khachhangthangmoi")]
        public async Task<ActionResult<int>> LayTongKhachHangMoiTrongThang()
        {
            var thangHientai = DateTime.Now.Month;
            var namHientai = DateTime.Now.Year;

            // LỌC CÁC KHÁCH HÀNG CÓ CREATEAT TRONG THÁNG HIỆN TẠI
            var tongSoKachhangmoi = await _context.KhachHangs
                .Where(kh => kh.CreatedAt.Month == thangHientai && kh.CreatedAt.Year == namHientai)
                .CountAsync();

            return Ok(new { tongSoKachhangmoi = tongSoKachhangmoi });
        }

        // Phương thức kiểm tra sự tồn tại của KhachHang
        private bool KhachHangExists(int id)
        {
            return _context.KhachHangs.Any(e => e.Id == id);
        }

        // Lấy thông tin sản phẩm từ SanPhamIds
        private async Task<(string SanphamNames, string SanphamDonViTinh)> GetSanPhamDetails(string sanPhamIds)
        {
            var ids = sanPhamIds.Trim('[', ']').Split(',').Select(int.Parse).ToList();
            var sanphams = await _context.Sanpham
                .Where(sp => ids.Contains(sp.Id))
                .Select(sp => new
                {
                    sp.Tieude, // Tên sản phẩm
                    sp.DonViTinh // Đơn vị tính
                })
                .ToListAsync();

            if (!sanphams.Any())
            {
                return (null, null);
            }

            string sanphamNames = string.Join(", ", sanphams.Select(sp => sp.Tieude));
            string donViTinh = sanphams.Select(sp => sp.DonViTinh).FirstOrDefault();

            return (sanphamNames, donViTinh);
        }
    }
}
