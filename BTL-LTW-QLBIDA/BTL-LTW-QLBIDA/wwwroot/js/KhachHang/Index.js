let currentPage = 1;
let loading = false;
let hasMore = true;

// LOAD TABLE
function loadMoreRows() {
    if (loading || !hasMore) return;
    loading = true;
    $("#loadingRow").remove();
    $("#khachhangBody").append(`<tr id="loadingRow"><td colspan="7" class="text-center py-3"><div class="spinner-border spinner-border-sm"></div> Loading...</td></tr>`);

    $.ajax({
        url: "/Khachhangs/LoadTable",
        type: "GET",
        data: $("#filterForm").serialize() + "&page=" + currentPage,
        success: function (html) {
            $("#loadingRow").remove();
            if (!html.trim()) {
                hasMore = false;
                $("#khachhangBody").append(`<tr class="no-more"><td colspan="7" class="text-center text-muted py-3">Đã hết dữ liệu</td></tr>`);
                return;
            }
            $("#khachhangBody").append(html);
            if (html.includes("no-more")) hasMore = false;
            else currentPage++;
        },
        complete: () => loading = false
    });
}

function reloadTable() {
    currentPage = 1; hasMore = true;
    $("#khachhangBody").empty();
    loadMoreRows();
}

// INIT
$(document).ready(function () {
    reloadTable();
    $("#tableScrollArea").on("scroll", function () {
        if ($(this).scrollTop() + $(this).height() >= this.scrollHeight - 50) loadMoreRows();
    });
    $("#filterForm").on("submit", function (e) { e.preventDefault(); reloadTable(); });
    $("#btnClearFilter").click(function () { $("#filterForm")[0].reset(); reloadTable(); });
});

// ACTIONS
$("#btnCreate").click(function () {
    let modal = new bootstrap.Modal(document.getElementById("ajaxModal"));
    modal.show();
    $("#ajaxModalContent").html('<div class="text-center p-4"><div class="spinner-border"></div></div>');
    $.get("/Khachhangs/CreatePartial").done(html => $("#ajaxModalContent").html(html));
});

$(document).on("click", ".btn-edit", function () {
    let id = $(this).data("id");
    let modal = new bootstrap.Modal(document.getElementById("ajaxModal"));
    modal.show();
    $.get("/Khachhangs/EditPartial", { id }).done(html => $("#ajaxModalContent").html(html));
});

$(document).on("click", ".btn-details", function () {
    let id = $(this).data("id");
    let modal = new bootstrap.Modal(document.getElementById("ajaxModal"));
    modal.show();
    $.get("/Khachhangs/DetailsPartial", { id }).done(html => $("#ajaxModalContent").html(html));
});

$(document).on("click", "#btnSaveCreate", function () {
    if (!$("input[name='Hoten']").val()) { Swal.fire("Lỗi", "Nhập tên!", "warning"); return; }
    $.post("/Khachhangs/CreateAjax", $("#createForm").serialize(), function (rs) {
        if (rs.success) {
            bootstrap.Modal.getInstance(document.getElementById("ajaxModal")).hide();
            Swal.fire("Thành công", "Đã thêm!", "success");
            reloadTable();
        } else Swal.fire("Lỗi", rs.message, "error");
    });
});

$(document).on("click", "#btnSaveEdit", function () {
    $.post("/Khachhangs/EditAjax", $("#editForm").serialize(), function (rs) {
        if (rs.success) {
            bootstrap.Modal.getInstance(document.getElementById("ajaxModal")).hide();
            Swal.fire("Thành công", "Đã sửa!", "success");
            reloadTable();
        } else Swal.fire("Lỗi", rs.message, "error");
    });
});

$(document).on("click", ".btn-delete", function () {
    let id = $(this).data("id");
    Swal.fire({ title: "Xóa?", showCancelButton: true }).then(r => {
        if (r.isConfirmed) {
            $.post("/Khachhangs/DeleteAjax", { id }, rs => {
                if (rs.success) { Swal.fire("Đã xóa", "", "success"); reloadTable(); }
                else Swal.fire("Không thể xóa", rs.message, "error");
            });
        }
    });
});