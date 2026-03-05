// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .build();

connection.on("ReceiveNotification", function (data) {
    // Sử dụng thư viện như Toastr hoặc Swal để hiển thị thông báo đẹp mắt
    toastr.info(data.content, data.title);

    // Cập nhật số lượng thông báo trên header (nếu có)
    let badge = document.getElementById("notification-badge");
    if (badge) {
        badge.innerText = parseInt(badge.innerText) + 1;
    }
});

connection.start().catch(err => console.error(err.toString()));