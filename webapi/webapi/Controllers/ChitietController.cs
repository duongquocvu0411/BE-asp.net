using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using webapi.Model;

namespace webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChitietsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChitietsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// lấy toàn bộ danh sách chitiets
        /// </summary>
        /// <returns> lấy toàn bộ danh sách chitiets</returns>
        // GET: api/Chitiets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChiTiet>>> GetChitiets()
        {
            return await _context.ChiTiets.ToListAsync();
        }

        /// <summary>
        /// xem chitiet theo id của sanpham_id {id}
        /// </summary>
        /// <returns> xem chitiet theo id của sanpham_id {id}</returns>
        // GET: api/Chitiets/{sanphamsId}

        [HttpGet("{sanphamsId}")]
        public async Task<ActionResult<ChiTiet>> GetChitiet(int sanphamsId)
        {
            var chitiet = await _context.ChiTiets.FirstOrDefaultAsync(c => c.SanphamsId == sanphamsId);

            if (chitiet == null)
            {
                return NotFound(new { message = "Chi tiết sản phẩm không tồn tại" });
            }

            return chitiet;
        }
        /// <summary>
        /// xem chitiet theo id của sanpham_id {id}
        /// </summary>
        /// /// <returns> xem chitiet theo id của sanpham_id {id}</returns>
        // POST: api/Chitiets
        [HttpPost]
        public async Task<ActionResult<ChiTiet>> PostChitiet([FromBody] ChiTiet chitiet)
        {
            // Validation: Check if the related product exists
            var sanphamExists = await _context.Sanpham.AnyAsync(s => s.Id == chitiet.SanphamsId);
            if (!sanphamExists)
            {
                return BadRequest(new { message = "Sản phẩm không tồn tại" });
            }

            _context.ChiTiets.Add(chitiet);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetChitiet), new { sanphamsId = chitiet.SanphamsId }, chitiet);
        }
        /// <summary>
        /// chỉnh sửa chitiet sản phẩm theo sản phẩm {i}
        /// </summary>
        /// <returns> chỉnh sửa chitiet sản phẩm theo sản phẩm {id}</returns>

        // PUT: api/Chitiets/{sanphamsId}
        [HttpPut("{sanphamsId}")]
        public async Task<IActionResult> UpdateChitiet(int sanphamsId, [FromBody] ChiTiet updatedChitiet)
        {
            var chitiet = await _context.ChiTiets.FirstOrDefaultAsync(c => c.SanphamsId == sanphamsId);

            if (chitiet == null)
            {
                return NotFound(new { message = "Chi tiết sản phẩm không tồn tại" });
            }

            // Update fields
            chitiet.MoTaChung = updatedChitiet.MoTaChung;
            chitiet.HinhDang = updatedChitiet.HinhDang;
            chitiet.CongDung = updatedChitiet.CongDung;
            chitiet.XuatXu = updatedChitiet.XuatXu;
            chitiet.KhoiLuong = updatedChitiet.KhoiLuong;
            chitiet.BaoQuan = updatedChitiet.BaoQuan;
            chitiet.ThanhPhanDinhDuong = updatedChitiet.ThanhPhanDinhDuong;
            chitiet.NgayThuHoach = updatedChitiet.NgayThuHoach;
            chitiet.HuongVi = updatedChitiet.HuongVi;
            chitiet.NongDoDuong = updatedChitiet.NongDoDuong;
            chitiet.BaiViet = updatedChitiet.BaiViet;

            _context.Entry(chitiet).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã cập nhật chi tiết sản phẩm thành công", chitiet });
        }
        /// <summary>
        /// xóa chitiet sản phẩm theo {idsanpham}
        /// </summary>
        /// <returns> xóa chitiet sản phẩm theo {idsanpham}</returns>
        
        // DELETE: api/Chitiets/{sanphamsId}
        [HttpDelete("{sanphamsId}")]
        public async Task<IActionResult> DeleteChitiet(int sanphamsId)
        {
            var chitiet = await _context.ChiTiets.FirstOrDefaultAsync(c => c.SanphamsId == sanphamsId);

            if (chitiet == null)
            {
                return NotFound(new { message = "Chi tiết sản phẩm không tồn tại" });
            }

            _context.ChiTiets.Remove(chitiet);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa chi tiết sản phẩm thành công" });
        }
    }
}
