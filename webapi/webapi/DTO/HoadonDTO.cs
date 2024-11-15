namespace webapi.DTO
{
    public class HoadonDTO
    {
        public class HoaDonDto
        {
            public int KhachHangId { get; set; }
            public List<int> SanphamIds { get; set; }
            public List<int> Quantities { get; set; }
        }
    }
}
