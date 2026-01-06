using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace SchoolManagement.Common
{
    public static class TempDataExtensions
    {
        public static void SetToast(this ITempDataDictionary tempData, string message, string type = "success")
        {
            tempData["ToastMessage"] = message;
            tempData["ToastType"] = type;
        }

        // Tạo thêm hàm viết tắt cho nhanh
        public static void ToastSuccess(this ITempDataDictionary tempData, string message)
            => SetToast(tempData, message, "success");

        public static void ToastError(this ITempDataDictionary tempData, string message)
            => SetToast(tempData, message, "danger");

        public static void ToastWarning(this ITempDataDictionary tempData, string message)
            => SetToast(tempData, message, "warning");
    }
}
