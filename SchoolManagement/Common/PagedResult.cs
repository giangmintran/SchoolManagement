namespace SchoolManagement.Common
{
    // Ví dụ class chứa dữ liệu phân trang
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } // Danh sách dữ liệu
        public int PageIndex { get; set; } // Trang hiện tại
        public int TotalPages { get; set; } // Tổng số trang
        public int TotalRecords { get; set; } // Tổng số trang
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

    }
}
