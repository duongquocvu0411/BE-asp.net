using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using webapi.Model;

namespace webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LienHeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LienHeController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách liên hệ
        /// </summary>
        /// <returns> Lấy danh sách liên hệ </returns>

        // GET: api/LienHe
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Lienhe>>> GetLienHes()
        {
            return await _context.Lienhes.ToListAsync();
        }

        /// <summary>
        ///  Thêm mới liên hệ
        /// </summary>
        /// <returns> Thêm mới liên hệ  </returns>

        // POST: api/LienHe
        [HttpPost]
        public async Task<ActionResult<Lienhe>> PostLienHe(Lienhe lienHe)
        {
            lienHe.created_at = DateTime.UtcNow;
            lienHe.updated_at = DateTime.UtcNow;

            _context.Lienhes.Add(lienHe);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLienHes), new { id = lienHe.id }, lienHe);
        }

        /// <summary>
        /// Xóa liên hệ theo {id}
        /// </summary>
        /// <returns>  Xóa liên hệ theo {id} </returns>

        // DELETE: api/LienHe/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLienHe(int id)
        {
            var lienHe = await _context.Lienhes.FindAsync(id);
            if (lienHe == null)
            {
                return NotFound(new { message = "Không tìm thấy liên hệ!" });
            }

            _context.Lienhes.Remove(lienHe);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Liên hệ đã được xóa thành công!" });
        }
    }
}
