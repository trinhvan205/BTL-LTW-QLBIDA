let currentPage = 1;
let loading = false;
let hasMore = true;

// ============================================================
// 1. LOAD TABLE (Infinite Scroll)
// ============================================================
function loadMoreRows() {
    if (loading || !hasMore) return;
    loading = true;

    // Xoá loading cũ nếu có
    $("#loadingRow").remove();

    // Thêm loading spinner vào body bảng nhân viên
    $("#nhanvienBody").append(`
        <tr id="loadingRow">
            <td colspan="7" class="text-center text-muted py-3">
                <div class="spinner-border spinner-border-sm me-2"></div>
                Đang tải dữ liệu...
            </td>
        </tr>
    `);

    $.ajax({
        url: "/Nhanviens/LoadTable",
        type: "GET",
        data: $("#filterForm").serialize() + "&page=" + currentPage,
        success: function (html) {
            $("#loadingRow").remove();

            // Nếu không có dữ liệu trả về
            if (!html.trim()) {
                hasMore = false;
                $("#nhanvienBody").append(`
                    <tr class="no-more">
                        <td colspan="7" class="text-center text-muted py-4">
                            <i class="bi bi-inbox me-1"></i> Đã hết dữ liệu
                        </td>
                    </tr>
                `);
                return;
            }

            $("#nhanvienBody").append(html);

            // Kiểm tra xem server đã báo hết dữ liệu chưa
            if (html.includes("no-more")) {
                hasMore = false;
            } else {
                currentPage++;
            }
        },
        error: function () {
            $("#loadingRow").html(`
                <td colspan="7" class="text-center text-danger py-3">
                    <i class="bi bi-exclamation-circle me-1"></i> Lỗi tải dữ liệu. Vui lòng thử lại.
                </td>
            `);
        },
        complete: function () {
            loading = false;
        }
    });
}

function reloadTable() {
    currentPage = 1;
    hasMore = true;
    $("#nhanvienBody").empty(); // Xoá sạch bảng
    loadMoreRows();
}

// ============================================================
// INIT & EVENTS
// ============================================================
$(document).ready(function () {
    reloadTable();

    // Sự kiện cuộn trong vùng bảng
    $("#tableScrollArea").on("scroll", function () {
        if ($(this).scrollTop() + $(this).height() >= this.scrollHeight - 50) {
            loadMoreRows();
        }
    });

    // Sự kiện submit form lọc
    $("#filterForm").on("submit", function (e) {
        e.preventDefault();
        reloadTable();
    });

    // Sự kiện nút Reset lọc
    $("#btnClearFilter").on("click", function () {
        $("#filterForm")[0].reset();
        reloadTable();
    });
});

// ============================================================
// 2. MODAL ACTIONS (Create / Edit / Details)
// ============================================================

// --- MỞ MODAL THÊM MỚI ---
$("#btnCreate").click(function () {
    let modal = new bootstrap.Modal(document.getElementById("ajaxModal"));
    modal.show();

    $("#ajaxModalContent").html('<div class="text-center p-5"><div class="spinner-border text-success"></div><p class="mt-2 text-muted">Đang tải form...</p></div>');

    $.get("/Nhanviens/CreatePartial")
        .done(function (html) {
            $("#ajaxModalContent").html(html);
        })
        .fail(function (res) {
            showErrorInModal(res);
        });
});

// --- MỞ MODAL SỬA ---
$(document).on("click", ".btn-edit", function () {
    let id = $(this).data("id");
    let modal = new bootstrap.Modal(document.getElementById("ajaxModal"));
    modal.show();

    $("#ajaxModalContent").html('<div class="text-center p-5"><div class="spinner-border text-warning"></div><p class="mt-2 text-muted">Đang tải thông tin...</p></div>');

    $.get("/Nhanviens/EditPartial", { id })
        .done(function (html) {
            $("#ajaxModalContent").html(html);
        })
        .fail(function (res) {
            showErrorInModal(res);
        });
});

// --- MỞ MODAL CHI TIẾT ---
$(document).on("click", ".btn-details", function () {
    let id = $(this).data("id");
    let modal = new bootstrap.Modal(document.getElementById("ajaxModal"));
    modal.show();

    $("#ajaxModalContent").html('<div class="text-center p-5"><div class="spinner-border text-info"></div></div>');

    $.get("/Nhanviens/DetailsPartial", { id })
        .done(function (html) {
            $("#ajaxModalContent").html(html);
        })
        .fail(function (res) {
            showErrorInModal(res);
        });
});

// Hàm hiển thị lỗi chung trong Modal
function showErrorInModal(response) {
    $("#ajaxModalContent").html(`
        <div class="text-center p-5 text-danger">
            <i class="bi bi-exclamation-triangle-fill fs-1"></i>
            <h5 class="mt-3">Đã xảy ra lỗi!</h5>
            <p>Mã lỗi: ${response.status} ${response.statusText}</p>
            <button class="btn btn-secondary mt-2" data-bs-dismiss="modal">Đóng</button>
        </div>
    `);
}

// ============================================================
// 3. SUBMIT ACTIONS (Lưu / Xóa / Đổi trạng thái)
// ============================================================

// --- LƯU THÊM MỚI ---
$(document).on("click", "#btnSaveCreate", function () {
    // Validate cơ bản phía Client
    if (!$("input[name='Hotennv']").val()) {
        Swal.fire("Thiếu thông tin", "Vui lòng nhập họ và tên nhân viên", "warning");
        return;
    }

    let data = $("#createForm").serialize();

    $.post("/Nhanviens/CreateAjax", data, function (rs) {
        if (rs.success) {
            bootstrap.Modal.getInstance(document.getElementById("ajaxModal")).hide();
            Swal.fire({
                icon: 'success',
                title: 'Thành công',
                text: 'Thêm nhân viên mới thành công!',
                timer: 1500,
                showConfirmButton: false
            });
            reloadTable();
        } else {
            Swal.fire("Lỗi", rs.message, "error");
        }
    }).fail(function () {
        Swal.fire("Lỗi Server", "Không thể kết nối đến máy chủ", "error");
    });
});

// --- LƯU CẬP NHẬT ---
$(document).on("click", "#btnSaveEdit", function () {
    if (!$("input[name='Hotennv']").val()) {
        Swal.fire("Thiếu thông tin", "Họ tên không được để trống", "warning");
        return;
    }

    let data = $("#editForm").serialize();

    $.post("/Nhanviens/EditAjax", data, function (rs) {
        if (rs.success) {
            bootstrap.Modal.getInstance(document.getElementById("ajaxModal")).hide();
            Swal.fire({
                icon: 'success',
                title: 'Đã cập nhật',
                text: 'Thông tin nhân viên đã được lưu!',
                timer: 1500,
                showConfirmButton: false
            });
            reloadTable();
        } else {
            Swal.fire("Lỗi", rs.message, "error");
        }
    }).fail(function () {
        Swal.fire("Lỗi Server", "Không thể kết nối đến máy chủ", "error");
    });
});

// --- XÓA NHÂN VIÊN ---
$(document).on("click", ".btn-delete", function () {
    let id = $(this).data("id");

    Swal.fire({
        title: "Xóa nhân viên?",
        text: "Hành động này sẽ xóa vĩnh viễn nhân viên khỏi hệ thống!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Xóa ngay",
        cancelButtonText: "Hủy",
        confirmButtonColor: "#dc3545"
    }).then((result) => {
        if (result.isConfirmed) {
            $.post("/Nhanviens/DeleteAjax", { id }, function (rs) {
                if (rs.success) {
                    Swal.fire("Đã xóa!", "Nhân viên đã bị xóa khỏi danh sách.", "success");
                    reloadTable();
                } else {
                    Swal.fire("Không thể xóa", rs.message, "error");
                }
            });
        }
    });
});

// --- ĐỔI TRẠNG THÁI (Nghỉ/Làm) ---
$(document).on("click", ".btn-toggle", function () {
    let id = $(this).data("id");

    Swal.fire({
        title: "Đổi trạng thái?",
        text: "Chuyển đổi trạng thái làm việc của nhân viên này?",
        icon: "question",
        showCancelButton: true,
        confirmButtonText: "Đồng ý",
        cancelButtonText: "Hủy"
    }).then((result) => {
        if (result.isConfirmed) {
            $.post("/Nhanviens/ToggleStatusAjax", { id }, function (rs) {
                if (rs.success) {
                    reloadTable();
                    const Toast = Swal.mixin({
                        toast: true,
                        position: 'top-end',
                        showConfirmButton: false,
                        timer: 3000
                    });
                    Toast.fire({
                        icon: 'success',
                        title: 'Đã cập nhật trạng thái'
                    });
                } else {
                    Swal.fire("Lỗi", "Không thể cập nhật trạng thái", "error");
                }
            });
        }
    });
});