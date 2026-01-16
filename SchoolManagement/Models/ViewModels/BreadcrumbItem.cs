using System.Collections.Generic;

namespace SchoolManagement.Models.ViewModels
{
    public class BreadcrumbItem
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public bool IsActive { get; set; } = false;
    }

    public class PageHeaderViewModel
    {
        public string Title { get; set; } // Ví dụ: "Sổ Sinh Hoạt Chuyên Môn"
        public List<BreadcrumbItem> Breadcrumbs { get; set; }

        public PageHeaderViewModel()
        {
            Breadcrumbs = new List<BreadcrumbItem>();
        }
    }
}