let currentPage = 1;
let loading = false;
let hasMore = true;

function loadMoreRows() {
    if (loading || !hasMore) return;
    loading = true;
    $("#loadingRow").remove();
    $("#dichvuBody").append(`<tr id="loadingRow"><td colspan="8" class="text-center py-3"><div class="spinner-border spinner-border-sm"></div> Loading...</td></tr>`);

    $.ajax({
        url: "/Dichvus/LoadTable",
        type: "GET",
        data: $("#filterForm").serialize() + "&page=" + currentPage,
        success: function (html) {
            $("#loadingRow").remove();
            if (!html.trim()) {
                hasMore = false;
                $("#dichvuBody").append(`<tr class="no-more"><td colspan="8" class="text-center text-muted py-3">Hết dữ liệu</td></tr>`);
                return;
            }
            $("#dichvuBody").append(html);
            if (html.includes("no-more")) hasMore = false; else currentPage++;
        },
        complete: () => loading = false
    });
}

function reloadTable() { currentPage = 1; hasMore = true; $("#dichvuBody").empty(); loadMoreRows(); }

$(document).ready(function () {
    reloadTable();
    $("#tableScrollArea").on("scroll", function () { if ($(this).scrollTop() + $(this).height() >= this.scrollHeight - 50) loadMoreRows(); });
    $("#filterForm").on("submit", function (e) { e.preventDefault(); reloadTable(); });
    $("#btnClearFilter").click(function () { $("#filterForm")[0].reset(); reloadTable(); });
});

// MODALS
$("#btnCreate").click(() => {
    let m = new bootstrap.Modal(document.getElementById("ajaxModal")); m.show();
    $.get("/Dichvus/CreatePartial").done(h => $("#ajaxModalContent").html(h));
});
$(document).on("click", ".btn-edit", function () {
    let m = new bootstrap.Modal(document.getElementById("ajaxModal")); m.show();
    $.get("/Dichvus/EditPartial", { id: $(this).data("id") }).done(h => $("#ajaxModalContent").html(h));
});
$(document).on("click", ".btn-details", function () {
    let m = new bootstrap.Modal(document.getElementById("ajaxModal")); m.show();
    $.get("/Dichvus/DetailsPartial", { id: $(this).data("id") }).done(h => $("#ajaxModalContent").html(h));
});

// SUBMIT (Dùng FormData để upload ảnh)
$(document).on("click", "#btnSaveCreate", function () {
    if (!$("input[name='Tendv']").val()) { Swal.fire("Lỗi", "Nhập tên!", "warning"); return; }

    var formData = new FormData($("#createForm")[0]); // Lấy toàn bộ form bao gồm file

    $.ajax({
        url: "/Dichvus/CreateAjax",
        type: 'POST',
        data: formData,
        processData: false, // Bắt buộc cho upload file
        contentType: false, // Bắt buộc cho upload file
        success: function (rs) {
            if (rs.success) { bootstrap.Modal.getInstance(document.getElementById("ajaxModal")).hide(); Swal.fire("Thành công", "", "success"); reloadTable(); }
            else Swal.fire("Lỗi", rs.message, "error");
        }
    });
});

$(document).on("click", "#btnSaveEdit", function () {
    var formData = new FormData($("#editForm")[0]);

    $.ajax({
        url: "/Dichvus/EditAjax",
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (rs) {
            if (rs.success) { bootstrap.Modal.getInstance(document.getElementById("ajaxModal")).hide(); Swal.fire("Thành công", "", "success"); reloadTable(); }
            else Swal.fire("Lỗi", rs.message, "error");
        }
    });
});

$(document).on("click", ".btn-delete", function () {
    let id = $(this).data("id");
    Swal.fire({ title: "Xóa?", showCancelButton: true }).then(r => {
        if (r.isConfirmed) $.post("/Dichvus/DeleteAjax", { id }, rs => {
            if (rs.success) { Swal.fire("Đã xóa", "", "success"); reloadTable(); }
            else Swal.fire("Lỗi", rs.message, "error");
        });
    });
});