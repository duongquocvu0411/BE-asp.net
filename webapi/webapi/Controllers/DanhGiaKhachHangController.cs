using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Model;


namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DanhGiaKhachHangController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DanhGiaKhachHangController(AppDbContext context)
        {
            _context = context;
        }
        /// <summary>
        /// lấy danh sách của Đánh giá khách hàng
        /// </summary>
        /// <returns> lấy danh sách của Đánh giá khách hàng</returns>
        // GET: api/DanhGiaKhachHang
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DanhGiaKhachHang>>> GetDanhGiaKhachHang([FromQuery] int? sanphams_id)
        {
            if (sanphams_id.HasValue)
            {
                var danhgias = await _context.DanhGiaKhachHang
                    .Where(dg => dg.SanphamsId == sanphams_id.Value)
                    .Include(dg => dg.Sanpham)
                    .ToListAsync();

                if (!danhgias.Any())
                    return Ok(new { message = "Không có đánh giá cho sản phẩm này" });

                return danhgias;
            }
            else
            {
                var danhgias = await _context.DanhGiaKhachHang.Include(dg => dg.Sanpham).ToListAsync();

                if (!danhgias.Any())
                    return NoContent();

                return danhgias;
            }
        }

        /// <summary>
        /// Lấy đánh giá khách hàng theo {id_sanpham}
        /// </summary>
        /// <returns> Lấy đánh giá khách hàng theo {id_sanpham}</returns>

        // GET: api/DanhGiaKhachHang/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<DanhGiaKhachHang>> GetDanhGiaKhachHangById(int id)
        {
            var danhgia = await _context.DanhGiaKhachHang.Include(dg => dg.Sanpham).FirstOrDefaultAsync(dg => dg.Id == id);

            if (danhgia == null)
                return NotFound(new { message = "Đánh giá không tồn tại" });

            return danhgia;
        }


        /// <summary>
        ///  Thêm mới 1 đánh giá  của {id_sanpham}
        /// </summary>
        /// <returns> Thêm mới 1 đánh giá của {id_sanpham}  </returns>

        // POST: api/DanhGiaKhachHang
        [HttpPost]
        public async Task<ActionResult<DanhGiaKhachHang>> CreateDanhGiaKhachHang(DanhGiaKhachHang danhgia)
        {
            if (!_context.Sanpham.Any(sp => sp.Id == danhgia.SanphamsId))
                return BadRequest(new { message = "Sản phẩm không tồn tại" });

            _context.DanhGiaKhachHang.Add(danhgia);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDanhGiaKhachHangById), new { id = danhgia.Id }, danhgia);
        }


        /// <summary>
        ///  Xóa 1 đánh giá theo {id} 
        /// </summary>
        /// <returns> Xóa 1 đánh giá theo {id}  </returns>

        // DELETE: api/DanhGiaKhachHang/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDanhGiaKhachHang(int id)
        {
            var danhgia = await _context.DanhGiaKhachHang.FindAsync(id);

            if (danhgia == null)
                return NotFound(new { message = "Đánh giá không tồn tại" });

            _context.DanhGiaKhachHang.Remove(danhgia);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đánh giá đã được xóa thành công" });
        }
    }
}
