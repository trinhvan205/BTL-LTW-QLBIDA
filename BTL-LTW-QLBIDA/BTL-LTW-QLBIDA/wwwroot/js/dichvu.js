$(document).ready(function () {

    /* ================================================================
       0. CLICK NÚT THÊM MỚI → RESET FORM & SINH MÃ DV
    ============================================================== */
    $("#openAddModal").on("click", function () {

        $("#addForm")[0].reset();
        $("#preview_add_img").attr("src", "/images/no-image.png");

        $.get("/Dichvus/GetNextId", function (id) {
            $("#newIddv").val(id);
        });
    });


    /* ================================================================
       1. BIẾN KHỞI TẠO
    ============================================================== */
    let page = 1;
    let loading = false;
    let hasMore = true;

    const tableBody = $("#dichvuTableBody");
    const tableScroll = $("#dichvuTable");
    const filterForm = $("#dvFilterForm");


    /* ================================================================
       2. HÀM LOAD DỮ LIỆU
    ============================================================== */
    loadData(true);

    function loadData(reset = false) {

        if (loading) return;
        loading = true;
        $("#dvLoading").removeClass("d-none");

        if (reset) {
            tableBody.html("");
            page = 1;
            hasMore = true;
        }

        const formData = filterForm.serializeArray();
        let filter = {};

        formData.forEach(x => {
            if (x.name === "status") {
                filter.status = (x.value === "" ? null : x.value === "true");
            }
            else filter[x.name] = x.value;
        });

        filter.page = page;

        $.ajax({
            url: "/Dichvus/LoadTable",
            method: "GET",
            data: filter,

            success: function (html) {
                tableBody.append(html);

                if (html.includes("no-more")) {
                    hasMore = false;
                } else {
                    hasMore = true;
                }
            },

            error: function () {
                Swal.fire("Lỗi", "Không thể tải dữ liệu dịch vụ!", "error");
            },

            complete: function () {
                loading = false;
                $("#dvLoading").addClass("d-none");
            }
        });
    }


    /* ================================================================
       3. INFINITE SCROLL TRONG TABLE
    ============================================================== */
    tableScroll.on("scroll", function () {
        if (!hasMore || loading) return;

        let st = tableScroll.scrollTop();
        let sh = tableScroll[0].scrollHeight;
        let ch = tableScroll.height();

        if (st + ch >= sh - 50) {
            page++;
            loadData(false);
        }
    });


    /* ================================================================
       4. FILTER SUBMIT
    ============================================================== */
    filterForm.on("submit", function (e) {
        e.preventDefault();
        loadData(true);
    });

    $("#clearFilter").click(function () {
        filterForm.trigger("reset");
        loadData(true);
    });


    /* ================================================================
       5. CLICK ROW → PANEL CHI TIẾT
    ============================================================== */
    tableBody.on("click", ".dv-row", function (e) {

        if ($(e.target).closest("button").length > 0) return;

        let id = $(this).data("id");

        $(".dv-row").removeClass("active-row");
        $(this).addClass("active-row");

        $("#dichvuDetail").html(`
            <div class="text-center py-4 text-muted">
                <div class="spinner-border text-success"></div>
            </div>`);

        $.ajax({
            url: "/Dichvus/DetailPartial",
            method: "GET",
            data: { id: id },
            success: function (html) {
                $("#dichvuDetail").html(html);
            }
        });
    });


    tableBody.on("click", ".btn-detail", function (e) {
        e.stopPropagation();
        let id = $(this).data("id");

        $("#dvDetailModalContent").html(`
            <div class="p-4 text-center text-muted">
                <div class="spinner-border text-primary"></div>
            </div>`);

        let modal = new bootstrap.Modal(document.getElementById("dvDetailModal"));
        modal.show();

        $.get("/Dichvus/DetailPartial", { id: id }, function (html) {
            $("#dvDetailModalContent").html(html);
        });
    });


    /* ================================================================
       6. SỬA DỊCH VỤ
    ============================================================== */
    tableBody.on("click", ".btn-edit", function (e) {
        e.stopPropagation();
        let id = $(this).data("id");

        $.get("/Dichvus/GetEditForm", { id: id }, function (html) {
            $("#editModalBody").html(html);
            $("#editModal").modal("show");
        });
    });

    $(document).on("click", "#btnSaveEdit", function () {

        let formData = new FormData();
        formData.append("Iddv", $("#editIddv").val());
        formData.append("Tendv", $("#editTendv").val());
        formData.append("Idloai", $("#editIdloai").val());
        formData.append("Giatien", $("#editGia").val());
        formData.append("Soluong", $("#editSoluong").val());
        formData.append("Hienthi", $("#editHienthi").val());

        let img = $("#editImage")[0].files[0];
        if (img) formData.append("imageFile", img);

        $.ajax({
            url: "/Dichvus/UpdateAjax",
            method: "POST",
            contentType: false,
            processData: false,
            data: formData,

            success: function (res) {
                if (!res.success) {
                    Swal.fire("Lỗi", res.message, "error");
                    return;
                }

                Swal.fire("Thành công", res.message, "success");
                $("#editModal").modal("hide");
                loadData(true);
            }
        });
    });


    /* ================================================================
       7. XÓA DỊCH VỤ
    ============================================================== */
    tableBody.on("click", ".btn-delete", function (e) {
        e.stopPropagation();

        let id = $(this).data("id");

        Swal.fire({
            title: "Xóa dịch vụ?",
            text: "Hành động này không thể hoàn tác!",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Xóa",
            cancelButtonText: "Hủy"
        }).then(r => {

            if (r.isConfirmed) {
                $.post("/Dichvus/DeleteAjax", { id: id }, function (res) {
                    if (res.success) {
                        Swal.fire("Đã xóa!", res.message, "success");
                        loadData(true);
                    } else {
                        Swal.fire("Lỗi", res.message, "error");
                    }
                });
            }
        });
    });


    /* ================================================================
       8. TOGGLE STATUS
    ============================================================== */
    tableBody.on("click", ".btn-toggle", function (e) {
        e.stopPropagation();

        let id = $(this).data("id");

        $.post("/Dichvus/ToggleStatus", { id: id }, function (res) {
            if (res.success) {
                Swal.fire("Thành công", "Đã cập nhật trạng thái!", "success");
                loadData(true);
            } else {
                Swal.fire("Lỗi", "Không thể cập nhật!", "error");
            }
        });
    });


    /* ================================================================
       9. CREATE
    ============================================================== */
    $("#addForm").submit(function (e) {
        e.preventDefault();

        let formData = new FormData(this);

        $.ajax({
            url: "/Dichvus/CreateAjax",
            method: "POST",
            contentType: false,
            processData: false,
            data: formData,

            success: function (res) {
                if (!res.success) {
                    Swal.fire("Lỗi", res.message, "error");
                    return;
                }

                Swal.fire("Thành công", res.message, "success");
                $("#addModal").modal("hide");
                loadData(true);
            }
        });
    });


    /* ================================================================
       10. PREVIEW ẢNH
    ============================================================== */
    $("#input_add_img").change(function () {
        let f = this.files[0];
        if (f) $("#preview_add_img").attr("src", URL.createObjectURL(f));
    });

    $(document).on("change", "#editImage", function () {
        let f = this.files[0];
        if (f) $("#editPreview").attr("src", URL.createObjectURL(f));
    });


    /* ================================================================
       11. EXPORT
    ============================================================== */
    $("#btnDvExportExcel").click(function () {
        let qs = filterForm.serialize();
        window.location = "/Dichvus/ExportExcel?" + qs;
    });


    /* ================================================================
   12. THÊM LOẠI DỊCH VỤ
================================================================ */
    $(document).on("click", "#btnSaveLoai", function () {

        let tenLoai = $("#tenLoaiMoi").val().trim();

        // Validate input
        if (tenLoai === "") {
            $("#loaiError")
                .text("Tên loại không được để trống")
                .removeClass("d-none");
            return;
        }

        // Gửi AJAX
        $.post("/Loaidichvus/CreateAjax", { tenLoai: tenLoai }, function (res) {

            if (res.success) {
                Swal.fire("Thành công", res.message, "success");

                // Ẩn modal
                $("#addLoaiModal").modal("hide");

                // Reset input
                $("#tenLoaiMoi").val("");
                $("#loaiError").addClass("d-none");

                // Reload trang để dropdown cập nhật
                location.reload();
            }
            else {
                $("#loaiError")
                    .text(res.message)
                    .removeClass("d-none");
            }
        });
    });
});
