let currentPage = 1;
let loading = false;
let hasMore = true;

function loadMoreRows() {
    if (loading || !hasMore) return;
    loading = true;
    $("#loadingRow").remove();
    $("#banBody").append(`<tr id="loadingRow"><td colspan="5" class="text-center py-3"><div class="spinner-border spinner-border-sm"></div> Đang tải...</td></tr>`);

    $.ajax({
        url: "/PhongBan/LoadTable", // Chú ý Controller tên là PhongBan
        type: "GET",
        data: $("#filterForm").serialize() + "&page=" + currentPage,
        success: function (html) {
            $("#loadingRow").remove();
            if (!html.trim()) {
                hasMore = false;
                $("#banBody").append(`<tr class="no-more"><td colspan="5" class="text-center text-muted py-3">Hết dữ liệu</td></tr>`);
                return;
            }
            $("#banBody").append(html);
            if (html.includes("no-more")) hasMore = false; else currentPage++;
        },
        complete: () => loading = false
    });
}

function reloadTable() { currentPage = 1; hasMore = true; $("#banBody").empty(); loadMoreRows(); }

$(document).ready(function () {
    reloadTable();
    $("#tableScrollArea").on("scroll", function () { if ($(this).scrollTop() + $(this).height() >= this.scrollHeight - 50) loadMoreRows(); });
    $("#filterForm").on("submit", function (e) { e.preventDefault(); reloadTable(); });
    $("#btnClearFilter").click(function () { $("#filterForm")[0].reset(); reloadTable(); });
});

// --- MODALS ---
$("#btnCreate").click(() => {
    let m = new bootstrap.Modal(document.getElementById("ajaxModal")); m.show();
    $("#ajaxModalContent").html('<div class="text-center p-4"><div class="spinner-border"></div></div>');
    $.get("/PhongBan/CreatePartial").done(h => $("#ajaxModalContent").html(h));
});

$(document).on("click", ".btn-edit", function () {
    let m = new bootstrap.Modal(document.getElementById("ajaxModal")); m.show();
    $.get("/PhongBan/EditPartial", { id: $(this).data("id") }).done(h => $("#ajaxModalContent").html(h));
});

$(document).on("click", ".btn-details", function () {
    let m = new bootstrap.Modal(document.getElementById("ajaxModal")); m.show();
    $.get("/PhongBan/DetailsPartial", { id: $(this).data("id") }).done(h => $("#ajaxModalContent").html(h));
});

// --- ACTIONS ---
$(document).on("click", "#btnSaveCreate", function () {
    $.post("/PhongBan/CreateAjax", $("#createForm").serialize(), rs => {
        if (rs.success) { bootstrap.Modal.getInstance(document.getElementById("ajaxModal")).hide(); Swal.fire("Thành công", "", "success"); reloadTable(); }
        else Swal.fire("Lỗi", rs.message, "error");
    });
});

$(document).on("click", "#btnSaveEdit", function () {
    $.post("/PhongBan/EditAjax", $("#editForm").serialize(), rs => {
        if (rs.success) { bootstrap.Modal.getInstance(document.getElementById("ajaxModal")).hide(); Swal.fire("Thành công", "", "success"); reloadTable(); }
        else Swal.fire("Lỗi", rs.message, "error");
    });
});

$(document).on("click", ".btn-delete", function () {
    let id = $(this).data("id");
    Swal.fire({ title: "Xóa bàn này?", text: "Không thể hoàn tác!", icon: "warning", showCancelButton: true, confirmButtonText: "Xóa" }).then(r => {
        if (r.isConfirmed) $.post("/PhongBan/DeleteAjax", { id }, rs => {
            if (rs.success) { Swal.fire("Đã xóa", "", "success"); reloadTable(); }
            else Swal.fire("Lỗi", rs.message, "error");
        });
    });
});