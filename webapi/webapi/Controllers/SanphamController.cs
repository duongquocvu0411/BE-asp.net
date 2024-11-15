using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using webapi.DTO;
using webapi.Model;

namespace webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SanphamController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public SanphamController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        private string GetImageUrl(string relativePath)
        {
            var relativePathWithoutRoot = relativePath.Replace("wwwroot\\", "").Replace("wwwroot/", "");
            return $"{Request.Scheme}://{Request.Host}/{relativePathWithoutRoot.Replace("\\", "/")}";
        }

        /// <summary>
        /// Lấy danh sách sản phẩm
        /// </summary>
        /// <returns> Lấy danh sách sản phẩm </returns>

        // GET: api/Sanpham

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetSanphams()
        {
            var sanphams = await _context.Sanpham
                 .Include(s => s.Danhmucsanpham)
                
                .Include(s => s.Images)
                .ToListAsync();

            var result = sanphams.Select(s => new
            {
                s.Id,
                s.Tieude,
                s.Giatien,
                Hinhanh = !string.IsNullOrEmpty(s.Hinhanh) ? GetImageUrl(s.Hinhanh) : string.Empty,
                s.Trangthai,
                s.DonViTinh,
                s.DanhmucsanphamId,
                DanhmucsanphamName = s.Danhmucsanpham?.Name,
                Images = s.Images
                    .Where(img => img != null) // Bỏ qua các image có giá trị null
                    .Select(img => new
                    {
                        img.Id,
                        img.SanphamsId,
                        Hinhanh = !string.IsNullOrEmpty(img.Hinhanh) ? GetImageUrl(img.Hinhanh) : string.Empty
                    })
                    .ToList()
            }).ToList();

            return Ok(result);
        }


        /// <summary>
        /// Lấy sản phẩm theo {id} xem chi tiết sản phẩm
        /// </summary>
        /// <returns> Lấy sản phẩm theo {id} xem chi tiết sản phẩm </returns>

        // GET: api/Sanpham/{id}

        [HttpGet("{id}")]
        public async Task<ActionResult> GetSanphamById(int id)
        {
            var sanpham = await _context.Sanpham
                .Include(s => s.ChiTiet)
                .Include(s => s.Images)
                .Include(s => s.Danhgiakhachhangs)
                .Include(s => s.Danhmucsanpham)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sanpham == null)
            {
                return NotFound(new { message = "Sản phẩm không tồn tại" });
            }

            // Trả về JSON mà không có "sanpham" trong `images`
            return new JsonResult(new
            {
                sanpham.Id,
                sanpham.Tieude,
                sanpham.Giatien,
                sanpham.Hinhanh,
                sanpham.Trangthai,
                sanpham.DonViTinh,
               sanpham.DanhmucsanphamId,
                DanhmucsanphamName = sanpham.Danhmucsanpham?.Name, // Lấy tên danh mục sản phẩm
                Danhgiakhachhangs = sanpham.Danhgiakhachhangs.Select(dg => new
                {
                    dg.Id,
                    dg.SanphamsId,
                    dg.HoTen,
                    dg.TieuDe,
                    dg.SoSao,
                    dg.NoiDung
                }),
                ChiTiet = sanpham.ChiTiet == null ? null : new
                {
                    sanpham.ChiTiet.Id,
                    sanpham.ChiTiet.SanphamsId,
                    sanpham.ChiTiet.MoTaChung,
                    sanpham.ChiTiet.HinhDang,
                    sanpham.ChiTiet.CongDung,
                    sanpham.ChiTiet.XuatXu,
                    sanpham.ChiTiet.KhoiLuong,
                    sanpham.ChiTiet.BaoQuan,
                    sanpham.ChiTiet.ThanhPhanDinhDuong,
                    sanpham.ChiTiet.NgayThuHoach,
                    sanpham.ChiTiet.HuongVi,
                    sanpham.ChiTiet.NongDoDuong,
                    sanpham.ChiTiet.BaiViet
                },
                Images = sanpham.Images.Select(img => new
                {
                    img.Id,
                    img.SanphamsId,
                    img.Hinhanh // Loại bỏ trường `sanpham` để không hiển thị `sanpham: null`
                })
            });
        }


        /// <summary>
        /// Thêm mới sản phẩm
        /// </summary>
        /// <returns>  Thêm mới sản phẩm</returns>

        // POST: api/Sanpham
        [HttpPost]
        public async Task<ActionResult<Sanpham>> PostSanpham([FromForm] SanphamDTO.SanphamCreateRequest request)
        {
            var sanpham = new Sanpham
            {
                Tieude = request.Tieude,
                Giatien = request.Giatien,
                Trangthai = request.Trangthai,
                DonViTinh = request.DonViTinh,
                DanhmucsanphamId = request.DanhmucsanphamId
            };

            // Lưu hình ảnh chính nếu có
            if (request.Hinhanh != null)
            {
                var imagePath = Path.Combine(_environment.WebRootPath, "image", request.Hinhanh.FileName);
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await request.Hinhanh.CopyToAsync(stream);
                }
                sanpham.Hinhanh = Path.Combine("image", request.Hinhanh.FileName);
            }

            // Lưu chi tiết sản phẩm nếu có
            if (request.ChiTiet != null)
            {
                sanpham.ChiTiet = new ChiTiet
                {
                    MoTaChung = request.ChiTiet.MoTaChung,
                    HinhDang = request.ChiTiet.HinhDang,
                    CongDung = request.ChiTiet.CongDung,
                    XuatXu = request.ChiTiet.XuatXu,
                    KhoiLuong = request.ChiTiet.KhoiLuong,
                    BaoQuan = request.ChiTiet.BaoQuan,
                    ThanhPhanDinhDuong = request.ChiTiet.ThanhPhanDinhDuong,
                    NgayThuHoach = request.ChiTiet.NgayThuHoach,
                    HuongVi = request.ChiTiet.HuongVi,
                    NongDoDuong = request.ChiTiet.NongDoDuong,
                    BaiViet = request.ChiTiet.BaiViet
                };
            }

            // Lưu hình ảnh phụ nếu có
            if (request.Images != null)
            {
                foreach (var image in request.Images)
                {
                    var imagePath = Path.Combine(_environment.WebRootPath, "hinhanhphu", image.FileName);
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    sanpham.Images.Add(new HinhAnhSanPham { Hinhanh = Path.Combine("hinhanhphu", image.FileName) });
                }
            }

            _context.Sanpham.Add(sanpham);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetSanphams), new { id = sanpham.Id }, sanpham);
        }


        /// <summary>
        ///  Chỉnh sửa sản phẩm theo {id}
        /// </summary>
        /// <returns> Chỉnh sửa sản phẩm theo {id}  </returns>
        // PUT: api/Sanpham/{id} api put sản phẩm
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSanpham(int id, [FromForm] SanphamDTO.SanphamUpdateRequest request)
        {
            var sanpham = await _context.Sanpham.Include(s => s.ChiTiet).Include(s => s.Images).FirstOrDefaultAsync(s => s.Id == id);
            if (sanpham == null) return NotFound(new { message = "Sản phẩm không tồn tại" });

            // Only update fields if they are present in the request
            if (!string.IsNullOrEmpty(request.Tieude))
            {
                sanpham.Tieude = request.Tieude;
            }

            //if (request.Giatien.HasValue)
            //{
            //    sanpham.Giatien = request.Giatien.Value;
            //}

            if (request.Giatien != 0) // Kiểm tra nếu Giatien khác 0, có thể thay 0 bằng giá trị mặc định của bạn nếu cần
            {
                sanpham.Giatien = request.Giatien;
            }

            if (!string.IsNullOrEmpty(request.Trangthai))
            {
                sanpham.Trangthai = request.Trangthai;
            }

            if (!string.IsNullOrEmpty(request.DonViTinh))
            {
                sanpham.DonViTinh = request.DonViTinh;
            }

            //if (request.DanhmucsanphamId.HasValue)
            //{
            //    sanpham.DanhmucsanphamId = request.DanhmucsanphamId.Value;
            //}
            if (request.DanhmucsanphamId != 0) // Kiểm tra nếu DanhmucsanphamId khác 0
            {
                sanpham.DanhmucsanphamId = request.DanhmucsanphamId;
            }

            // Update main image only if provided
            if (request.Hinhanh != null)
            {
                var imagePath = Path.Combine(_environment.WebRootPath, "image", request.Hinhanh.FileName);
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await request.Hinhanh.CopyToAsync(stream);
                }
                sanpham.Hinhanh = Path.Combine("image", request.Hinhanh.FileName);
            }
            // Update product details if provided
            if (request.ChiTiet != null)
            {
                if (sanpham.ChiTiet == null)
                {
                    sanpham.ChiTiet = new ChiTiet();
                    _context.ChiTiets.Add(sanpham.ChiTiet);
                }

                // Cập nhật từng trường riêng biệt
                sanpham.ChiTiet.MoTaChung = request.ChiTiet.MoTaChung; // Cho phép `null`
                sanpham.ChiTiet.HinhDang = request.ChiTiet.HinhDang;
                sanpham.ChiTiet.CongDung = request.ChiTiet.CongDung;
                sanpham.ChiTiet.XuatXu = request.ChiTiet.XuatXu;
                sanpham.ChiTiet.KhoiLuong = request.ChiTiet.KhoiLuong;
                sanpham.ChiTiet.BaoQuan = request.ChiTiet.BaoQuan;
                sanpham.ChiTiet.ThanhPhanDinhDuong = request.ChiTiet.ThanhPhanDinhDuong;
                sanpham.ChiTiet.NgayThuHoach = request.ChiTiet.NgayThuHoach;
                sanpham.ChiTiet.HuongVi = request.ChiTiet.HuongVi;
                sanpham.ChiTiet.NongDoDuong = request.ChiTiet.NongDoDuong;
                sanpham.ChiTiet.BaiViet = request.ChiTiet.BaiViet;
            }


            //// Update product details if provided
            //if (request.ChiTiet != null)
            //{
            //    if (sanpham.ChiTiet == null)
            //    {
            //        sanpham.ChiTiet = new ChiTiet();
            //        _context.ChiTiets.Add(sanpham.ChiTiet);
            //    }
            //    sanpham.ChiTiet.MoTaChung = request.ChiTiet.MoTaChung ?? sanpham.ChiTiet.MoTaChung;
            //    sanpham.ChiTiet.HinhDang = request.ChiTiet.HinhDang ?? sanpham.ChiTiet.HinhDang;
            //    sanpham.ChiTiet.CongDung = request.ChiTiet.CongDung ?? sanpham.ChiTiet.CongDung;
            //    sanpham.ChiTiet.XuatXu = request.ChiTiet.XuatXu ?? sanpham.ChiTiet.XuatXu;
            //    sanpham.ChiTiet.KhoiLuong = request.ChiTiet.KhoiLuong ?? sanpham.ChiTiet.KhoiLuong;
            //    sanpham.ChiTiet.BaoQuan = request.ChiTiet.BaoQuan ?? sanpham.ChiTiet.BaoQuan;
            //    sanpham.ChiTiet.ThanhPhanDinhDuong = request.ChiTiet.ThanhPhanDinhDuong ?? sanpham.ChiTiet.ThanhPhanDinhDuong;
            //    sanpham.ChiTiet.NgayThuHoach = request.ChiTiet.NgayThuHoach ?? sanpham.ChiTiet.NgayThuHoach;
            //    sanpham.ChiTiet.HuongVi = request.ChiTiet.HuongVi ?? sanpham.ChiTiet.HuongVi;
            //    sanpham.ChiTiet.NongDoDuong = request.ChiTiet.NongDoDuong ?? sanpham.ChiTiet.NongDoDuong;
            //    sanpham.ChiTiet.BaiViet = request.ChiTiet.BaiViet ?? sanpham.ChiTiet.BaiViet;
            //}

            // Remove old images and add new ones if provided
            if (sanpham.Images.Any() && request.Images != null)
            {
                _context.HinhAnhSanPhams.RemoveRange(sanpham.Images);
            }

            if (request.Images != null)
            {
                foreach (var image in request.Images)
                {
                    var imagePath = Path.Combine(_environment.WebRootPath, "hinhanhphu", image.FileName);
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    sanpham.Images.Add(new HinhAnhSanPham { Hinhanh = Path.Combine("hinhanhphu", image.FileName) });
                }
            }

            await _context.SaveChangesAsync();

            // Return the updated product with a 200 OK response
            return Ok(sanpham);
        }


        /// <summary>
        ///  Xóa sản phẩm theo {id}
        /// </summary>
        /// <returns> Xóa sản phẩm theo {id}  </returns>

        // DELETE: api/Sanpham/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSanpham(int id)
        {
            var sanpham = await _context.Sanpham
                .Include(s => s.Images)
                .Include(s => s.ChiTiet)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sanpham == null)
            {
                return NotFound(new { message = "Sản phẩm không tồn tại" });
            }

            // Xóa ảnh chính
            if (!string.IsNullOrEmpty(sanpham.Hinhanh))
            {
                var mainImagePath = Path.Combine(_environment.WebRootPath, sanpham.Hinhanh);
                if (System.IO.File.Exists(mainImagePath))
                {
                    System.IO.File.Delete(mainImagePath);
                }
            }

            // Xóa ảnh phụ
            if (sanpham.Images.Any())
            {
                foreach (var image in sanpham.Images)
                {
                    var imagePath = Path.Combine(_environment.WebRootPath, image.Hinhanh);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
            }

            // Xóa chi tiết sản phẩm
            if (sanpham.ChiTiet != null)
            {
                _context.ChiTiets.Remove(sanpham.ChiTiet);
            }

            _context.Sanpham.Remove(sanpham);
            await _context.SaveChangesAsync();

            return NoContent();
        }



        /// <summary>
        ///  Xóa hình ảnh phụ của sản phẩm
        /// </summary>
        /// <returns> Xóa hình ảnh phụ của sản phẩm  </returns>

        // DELETE: api/Sanpham/images/{imageId} api xóa hinhanhphu
        [HttpDelete("images/{imageId}")]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            // Tìm ảnh phụ theo ID
            var image = await _context.HinhAnhSanPhams.FindAsync(imageId);
            if (image == null)
            {
                return NotFound(new { message = "Ảnh phụ không tồn tại" });
            }

            // Xóa ảnh khỏi thư mục vật lý nếu tồn tại
            var imagePath = Path.Combine(_environment.WebRootPath, image.Hinhanh);
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            // Xóa ảnh khỏi cơ sở dữ liệu
            _context.HinhAnhSanPhams.Remove(image);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        ///  Lấy sản phẩm theo danh mục {danhmucid}
        /// </summary>
        /// <returns> Lấy sản phẩm theo danh mục {danhmucid}  </returns>

        // lấy sản phẩm theo danhmuc sản phẩm
        [HttpGet("danhmuc/{danhmucId}")]
        public async Task<ActionResult<IEnumerable<Sanpham>>> GetSanphamsByDanhMuc(int danhmucId)
        {
            var sanphams = await _context.Sanpham
                .Where(s => s.DanhmucsanphamId == danhmucId)
                .Include(s => s.Danhmucsanpham)
                .ToListAsync();

            if (!sanphams.Any())
            {
                return NotFound(new { message = "Không có sản phẩm nào thuộc danh mục này." });
            }

            // Chỉ trả về danh sách sản phẩm với các thông tin cơ bản mà không bao gồm chi tiết và đánh giá
            var result = sanphams.Select(s => new
            {
                s.Id,
                s.Tieude,
                s.Giatien,
                Hinhanh = !string.IsNullOrEmpty(s.Hinhanh) ? GetImageUrl(s.Hinhanh) : string.Empty,
                s.Trangthai,
                s.DonViTinh,
                s.DanhmucsanphamId,
                DanhmucsanphamName = s.Danhmucsanpham?.Name,
            });

            return Ok(result);
        }


        /// <summary>
        ///  lấy Tổng sản phẩm đang có trong bảng
        /// </summary>
        /// <returns> lấy Tổng sản phẩm đang có trong bảng  </returns>

        // GET: api/Sanpham/TongSanPham
        [HttpGet("TongSanPham")]
        public async Task<ActionResult<object>> GetTongSanPham()
        {
            var tongSanPham = await _context.Sanpham.CountAsync();
            return Ok(new { TongSanPham = tongSanPham });
        }


        /// <summary>
        /// thêm mới hình ảnh ở chitiet sản phẩm
        /// </summary>
        /// <returns> thêm mới hình ảnh ở chitiet sản phẩm </returns>

        // API uploadImage dành cho bài viết chi tiết
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile upload)
        {
            if (upload == null || upload.Length == 0)
            {
                return BadRequest(new { uploaded = false, error = new { message = "Không có tệp nào được tải lên" } });
            }

            try
            {
                // Lưu tệp vào thư mục wwwroot/upload
                var uploadPath = Path.Combine(_environment.WebRootPath, "upload");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var fileName = $"{DateTime.Now.Ticks}_{upload.FileName}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(stream);
                }

                var url = $"{Request.Scheme}://{Request.Host}/upload/{fileName}";

                return Ok(new { uploaded = true, url });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { uploaded = false, error = new { message = "Lỗi khi tải lên tệp", details = ex.Message } });
            }
        }
    }
    //public class SanphamCreateRequest
    //{
    //    public string Tieude { get; set; }
    //    public decimal Giatien { get; set; } // Make this nullable
    //    //public decimal? Giatien { get; set; } // Make this nullable
    //    public string Trangthai { get; set; }
    //    public string DonViTinh { get; set; }
    //    public int DanhmucsanphamId { get; set; }
    //    //public int? DanhmucsanphamId { get; set; } // Make this nullable
    //    public IFormFile Hinhanh { get; set; } // Main image
    //    public IFormFileCollection? Images { get; set; } // Secondary images
    //    public ChiTietDto? ChiTiet { get; set; } // Product details
    //}

    //public class SanphamUpdateRequest
    //{
    //    public string Tieude { get; set; }
    //    public decimal Giatien { get; set; }
    //    public string Trangthai { get; set; }
    //    public string DonViTinh { get; set; }
    //    public int DanhmucsanphamId { get; set; }
    //    public IFormFile? Hinhanh { get; set; } // Main image, optional for PUT
    //    public IFormFileCollection? Images { get; set; } // Secondary images
    //    public ChiTietDto? ChiTiet { get; set; } // Product details
    //}

    //public class ChiTietDto
    //{
    //    public string? MoTaChung { get; set; }
    //    public string? HinhDang { get; set; }
    //    public string? CongDung { get; set; }
    //    public string? XuatXu { get; set; }
    //    public string? KhoiLuong { get; set; }
    //    public string? BaoQuan { get; set; }
    //    public string? ThanhPhanDinhDuong { get; set; }
    //    public DateTime? NgayThuHoach { get; set; }
    //    public string? HuongVi { get; set; }
    //    public string? NongDoDuong { get; set; }
    //    public string? BaiViet { get; set; }
    //}
}
