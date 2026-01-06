namespace SchoolManagement.Common
{
    using Microsoft.AspNetCore.Mvc;

    public static class ToastExtensions
    {
        public static void ToastSuccess(this Controller controller, string message)
        {
            controller.TempData["ToastMessage"] = message;
            controller.TempData["ToastType"] = "success";
        }

        public static void ToastError(this Controller controller, string message)
        {
            controller.TempData["ToastMessage"] = message;
            controller.TempData["ToastType"] = "error";
        }

        public static void ToastWarning(this Controller controller, string message)
        {
            controller.TempData["ToastMessage"] = message;
            controller.TempData["ToastType"] = "warning";
        }

        public static void ToastInfo(this Controller controller, string message)
        {
            controller.TempData["ToastMessage"] = message;
            controller.TempData["ToastType"] = "info";
        }
    }

}
