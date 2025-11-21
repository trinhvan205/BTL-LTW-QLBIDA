let currentPage = 1;
let loading = false;
let hasMore = true;

function loadMoreRows() {

    if (loading || !hasMore) return;
    loading = true;

    $("#loadingRow").remove();

    $("#invoiceBody").append(`
        <tr id="loadingRow">
            <td colspan="7" class="text-center text-muted py-2">
                <div class="spinner-border spinner-border-sm me-2"></div>
                Đang tải thêm...
            </td>
        </tr>
    `);

    $.ajax({
        url: "/Hoadons/LoadTable",
        type: "GET",
        data: $("#filterForm").serialize() + "&page=" + currentPage,
        cache: false,
        success: function (html) {

            $("#loadingRow").remove();

            if (!html.trim()) {
                hasMore = false;
                $("#invoiceBody").append(`
                    <tr class="no-more">
                        <td colspan="7" class="text-center text-muted py-3">
                            — Hết dữ liệu —
                        </td>
                    </tr>
                `);
                return;
            }

            $("#invoiceBody").append(html);

            hasMore = !html.includes("no-more");

            if (hasMore) currentPage++;

            // 🔥 AUTO LOAD UNTIL SCROLL APPEARS
            setTimeout(() => {
                let area = $("#tableScrollArea");
                if (area[0].scrollHeight <= area.height() + 50 && hasMore) {
                    loadMoreRows();
                }
            }, 150);
        },

        complete: function () {
            loading = false;
        }
    });
}


function reloadTable() {
    currentPage = 1;
    hasMore = true;

    $("#invoiceBody").html(`
        <tr id="loadingRow">
            <td colspan="7" class="text-center text-muted py-3">
                <div class="spinner-border spinner-border-sm me-2"></div>
                Đang tải...
            </td>
        </tr>
    `);

    loadMoreRows();
}
// Auto load nếu bảng chưa đủ chiều cao
setTimeout(() => {
    let area = $("#tableScrollArea");
    if (area[0].scrollHeight <= area.height() + 50) {
        loadMoreRows();
    }
}, 200);

document.addEventListener("DOMContentLoaded", () => {

    reloadTable();

    $("#tableScrollArea").on("scroll", function () {
        if ($(this).scrollTop() + $(this).height() >= this.scrollHeight - 50) {
            loadMoreRows();
        }
    });

    $("#filterForm").on("submit", e => {
        e.preventDefault();
        reloadTable();
    });

    $("#btnClearFilter").on("click", () => {
        $("#filterForm")[0].reset();
        reloadTable();
    });
});


// =============================
//   CHI TIẾT
// =============================
$(document).on("click", ".btn-details", function () {
    let id = $(this).data("id");

    $("#ajaxModalContent").html(`
        <div class="text-center py-4">
            <div class="spinner-border text-info"></div>
        </div>
    `);

    let modal = new bootstrap.Modal(document.getElementById("ajaxModal"));
    modal.show();

    $.get("/Hoadons/DetailsPartial", { id }, function (html) {
        $("#ajaxModalContent").html(html);
    });
});

// =============================
//   SỬA
// =============================
$(document).on("click", ".btn-edit", function () {
    let id = $(this).data("id");

    $("#ajaxModalContent").html(`
        <div class="text-center py-4">
            <div class="spinner-border text-warning"></div>
        </div>
    `);

    let modal = new bootstrap.Modal(document.getElementById("ajaxModal"));
    modal.show();

    $.get("/Hoadons/EditPartial", { id }, function (html) {
        $("#ajaxModalContent").html(html);
    });
});


//$(document).on("click", ".btn-toggle", function () {
//    let id = $(this).data("id");
//    $.post("/Hoadons/ToggleStatusAjax", { id }, function () {
//        reloadTable();
//    });
//});

// =============================
//   SAVE EDIT
// =============================
$(document).on("click", "#btnSaveEdit", function () {

    let data = $("#editForm").serialize();

    $.post("/Hoadons/EditAjax", data, function (rs) {

        if (!rs.success) {
            Swal.fire("Lỗi", rs.message, "error");
            return;
        }

        Swal.fire("Thành công", "Đã cập nhật!", "success");
        bootstrap.Modal.getInstance(document.getElementById("ajaxModal")).hide();

        reloadTable();
    });
});

// =============================
//   DELETE
// =============================
$(document).on("click", ".btn-delete", function () {

    if ($(this).prop("disabled")) return;

    let id = $(this).data("id");

    Swal.fire({
        title: "Xóa hóa đơn?",
        text: "Không thể khôi phục!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Xóa",
        cancelButtonText: "Hủy"
    }).then(result => {

        if (!result.isConfirmed) return;

        $.post("/Hoadons/DeleteAjax", { id }, function (rs) {

            if (!rs.success) {
                Swal.fire("Không thể xóa", rs.message, "error");
                return;
            }

            Swal.fire("Đã xóa!", "", "success");

            reloadTable();
        });
    });
});


// =============================
//   PRINT
// =============================
$(document).on("click", ".btn-print", function () {
    let id = $(this).data("id");
    window.open("/Hoadons/Print?id=" + id, "_blank");
});

$(document).on("click", ".btn-print-detail", function () {
    let id = $(this).data("id");
    window.open("/Hoadons/Print?id=" + id, "_blank");
});

// ===============================
// IN HÓA ĐƠN – HIỆN MODAL PDF
// ===============================



// =============================
//   EXPORT EXCEL
// =============================
$("#btnExportExcel").on("click", function () {
    let qs = $("#filterForm").serialize();
    window.location = "/Hoadons/ExportExcel?" + qs;
});
