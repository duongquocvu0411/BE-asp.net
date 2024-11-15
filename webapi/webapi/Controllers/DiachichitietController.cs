using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.Model;

namespace webapi.Controllers
{
    [Route("api/[controller]")] 
    [ApiController]
    public class DiachichitietController:ControllerBase
    {
        private readonly AppDbContext _context;

        public DiachichitietController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách tất cả các địa chỉ chi tiết.
        /// </summary>
        /// <returns>Danh sách các đối tượng Diachichitiet</returns>
        // GET: api/Diachichitiet
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Diachichitiet>>> Index()
        {
            return await _context.Diachichitiets.ToListAsync();
        }

        /// <summary>
        /// thêm 1  địa chỉ chi tiết.
        /// </summary>
        /// <returns>thêm 1  địa chỉ chi tiết.</returns>
        // POST: api/Diachichitiet
        [HttpPost]
        public async Task<ActionResult<Diachichitiet>> Store(Diachichitiet diachi)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Diachichitiets.Add(diachi);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Show), new { id = diachi.Id }, diachi);
        }
        /// <summary>
        /// xem id  địa chỉ chi tiết.
        /// </summary>
        /// <returns>xem id  địa chỉ chi tiết.</returns>
        // GET: api/Diachichitiet/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Diachichitiet>> Show(int id)
        {
            var diachi = await _context.Diachichitiets.FindAsync(id);
            
            if(diachi == null)
            {
                return NotFound(new { message = " không tìm thấy địa chỉ với id này " });
            }
            return Ok(diachi);
        }

        /// <summary>
        /// chỉnh sửa  địa chỉ chi tiết theo {id}.
        /// </summary>
        /// <returns>chỉnh sửa  địa chỉ chi tiết theo {id}.</returns>
        // PUT: api/Diachichitiet/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Diachichitiet diachi)
        {
            // Tìm đối tượng trong cơ sở dữ liệu dựa trên `id` từ URL
            var existingDiachi = await _context.Diachichitiets.FindAsync(id);

            if (existingDiachi == null)
            {
                return NotFound();
            }

            // Cập nhật các thuộc tính từ `diachi` (ngoại trừ Id)
            existingDiachi.Diachi = diachi.Diachi;
            existingDiachi.Sdt = diachi.Sdt;
            existingDiachi.Email = diachi.Email;
            existingDiachi.Status = diachi.Status;

            _context.Entry(existingDiachi).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DiachichitietExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(existingDiachi);
        }
        /// <summary>
        /// Xóa địa chỉ chi tiết theo {id}.
        /// </summary>
        /// <returns>Xóa địa chỉ chi tiết theo {id}..</returns>

        // DELETE: api/Diachichitiet/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Destroy(int id)
        {
            var diachi = await _context.Diachichitiets.FindAsync(id);

            if(diachi == null)
            {
                return NotFound(new { message = " không tìm thấy địa chỉ chi tiết với id này" });
            }
            _context.Diachichitiets.Remove(diachi);
            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();
            // trả về thông báo nếu xóa thành công
            return Ok(new { message = "Xóa địa chỉ chi tiết thành công" });
        }

        /// <summary>
        /// post địa chỉ chi tiết {id} status thành: "đang sử dụng"
        /// </summary>
        /// <returns>post địa chỉ chi tiết {id} status thành: "đang sử dụng"</returns>
        // Custom endpoint: Set a specific address as "đang sử dụng"
        [HttpPost("setDiaChiHien/{id}")]
        public async Task<IActionResult> SetDiaChiHien(int id)
        {
            // Set tất cả địa chỉ khác thành "không sử dụng"
            await _context.Diachichitiets.ForEachAsync(d => d.Status = "không sử dụng");
            await _context.SaveChangesAsync();

            // Cập nhật địa chỉ với id cụ thể thành "đang sử dụng"
            var diachi = await _context.Diachichitiets.FindAsync(id);
            if (diachi == null)
            {
                return NotFound();
            }

            diachi.Status = "đang sử dụng";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Địa chỉ đã được chọn làm địa chỉ đang sử dụng" });
        }
       
        /// <summary>
        /// lấy danh sách địa chỉ chi tiết đang có status : "đang sử dụng"
        /// </summary>
        /// <returns> lấy danh sách địa chỉ chi tiết đang có status : "đang sử dụng"</returns>
        // Custom endpoint: Get the address that is currently "đang sử dụng"
        [HttpGet("getDiaChiHien")]
        public async Task<ActionResult<Diachichitiet>> GetDiaChiHien()
        {
            var diachi = await _context.Diachichitiets.FirstOrDefaultAsync(d => d.Status == "đang sử dụng");

            if (diachi == null)
            {
                return NotFound(new { message = "Không có địa chỉ đang sử dụng" });
            }

            return diachi;
        }

        private bool DiachichitietExists(int id)
        {
            return _context.Diachichitiets.Any(e => e.Id == id);
        }
    }
}