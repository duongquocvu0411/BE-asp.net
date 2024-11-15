using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Model
{
    [Table("khachhangs")]
    public class KhachHang
    {
        public int Id { get; set; }

        [Required]
        [Column("ten")]
        public string Ten { get; set; }

        [Required]
        [Column("ho")]
        public string Ho { get; set; }
        [Column("diachicuthe")]
        public string DiaChiCuThe { get; set; }
        [Column("thanhpho")]
        public string ThanhPho { get; set; }
        [Column("sdt")]
        public string Sdt { get; set; }
        [Column("Emaildiachi")]
        public string EmailDiaChi { get; set; }
        [Column("ghichu")]
        public string GhiChu { get; set; }
    
        // Thuộc tính ngày tạo
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        // Định nghĩa quan hệ một-nhiều với Hóa Đơn (HoaDon)
        public ICollection<HoaDon>? HoaDons { get; set; }
    }
}
