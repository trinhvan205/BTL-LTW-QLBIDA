$(document).ready(function () {

    /* ================================================================
       0. NÚT THÊM MỚI → RESET & LẤY MÃ DV
    ================================================================ */
    $("#openAddModal").on("click", function () {
        $("#addForm")[0].reset();
        $("#preview_add_img").attr("src", "/images/no-image.png");

        $.get("/Dichvus/GetNextId", function (id) {
            $("#newIddv").val(id);
        });
    });


    /* ================================================================
       1. BIẾN
    ================================================================ */
    let page = 1;
    let loading = false;
    let hasMore = true;

    const tableBody = $("#dichvuTableBody");
    const tableScroll = $("#dichvuTable");
    const filterForm = $("#dvFilterForm");


    /* ================================================================
       2. LOAD DỮ LIỆU
    ================================================================ */
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

        let filter = {};
        filterForm.serializeArray().forEach(x => {

            if (x.name === "status") {
                filter.status = (x.value === "" ? null : x.value === "true");
            } else {
                filter[x.name] = x.value;
            }
        });

        filter.page = page;

        $.ajax({
            url: "/Dichvus/LoadTable",
            type: "GET",
            data: filter,

            success: function (html) {
                tableBody.append(html);
                hasMore = !html.includes("no-more");
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
       3. SCROLL
    ================================================================ */
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
       4. BỘ LỌC
    ================================================================ */
    filterForm.on("submit", function (e) {
        e.preventDefault();
        loadData(true);
    });

    $("#btnClearFilter").on("click", function () {
        filterForm.trigger("reset");
        loadData(true);
    });
    /* ======================================================
   AUTO FILTER – tự lọc khi thay đổi input/select
====================================================== */

    // Gõ tên dịch vụ → tự lọc sau 350ms
    let typingTimer;
    $("input[name='keyword']").on("keyup", function () {
        clearTimeout(typingTimer);
        typingTimer = setTimeout(() => loadData(true), 350);
    });

    // Chọn loại / trạng thái → lọc ngay
    $("select[name='loaiId'], select[name='status']").on("change", function () {
        loadData(true);
    });

    // Giá từ / giá đến → lọc ngay khi nhập số
    $("input[name='minPrice'], input[name='maxPrice']").on("input", function () {
        loadData(true);
    });


    /* ================================================================
       5. CLICK ROW XEM CHI TIẾT
    ================================================================ */
    tableBody.on("click", ".dv-row", function (e) {
        if ($(e.target).closest("button").length > 0) return;

        let id = $(this).data("id");

        $(".dv-row").removeClass("active-row");
        $(this).addClass("active-row");

        $("#dichvuDetail").html(`
            <div class="text-center py-4 text-muted">
                <div class="spinner-border text-success"></div>
            </div>`);

        $.get("/Dichvus/DetailPartial", { id: id }, function (html) {
            $("#dichvuDetail").html(html);
        });
    });


    /* ================================================================
       6. MODAL CHI TIẾT
    ================================================================ */
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
       7. SỬA
    ================================================================ */
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
            processData: false,
            contentType: false,
            data: formData,

            success: function (res) {
                if (!res.success) {
                    Swal.fire("Lỗi", res.message, "error");
                    return;
                }

                Swal.fire("Thành công", "Cập nhật thành công!", "success");
                $("#editModal").modal("hide");
                loadData(true);
            }
        });
    });


    /* ================================================================
       8. XÓA
    ================================================================ */
    tableBody.on("click", ".btn-delete", function (e) {
        e.stopPropagation();
        let id = $(this).data("id");

        Swal.fire({
            title: "Xóa dịch vụ?",
            text: "Không thể hoàn tác!",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Xóa",
            cancelButtonText: "Hủy"
        }).then(r => {

            if (!r.isConfirmed) return;

            $.post("/Dichvus/DeleteAjax", { id: id }, function (res) {
                if (!res.success) {
                    Swal.fire("Lỗi", res.message, "error");
                    return;
                }

                Swal.fire("Đã xoá!", "Dịch vụ đã được xoá.", "success");
                loadData(true);
            });
        });
    });


    /* ================================================================
       9. TOGGLE STATUS
    ================================================================ */
    tableBody.on("click", ".btn-toggle", function (e) {
        e.stopPropagation();

        let id = $(this).data("id");

        $.post("/Dichvus/ToggleStatus", { id: id }, function (res) {
            if (res.success) {
                Swal.fire("OK!", "Đã đổi trạng thái.", "success");
                loadData(true);
            }
        });
    });


    /* ================================================================
       10. VALIDATION + CREATE
    ================================================================ */
    $("#addForm").on("submit", function (e) {
        e.preventDefault();

        let ten = $("#add_Tendv").val()?.trim();
        let loai = $("#add_Idloai").val();
        let gia = parseFloat($("#add_Gia").val());
        let ton = parseInt($("#add_Soluong").val());

        if (!ten || ten.length < 2) {
            Swal.fire("Lỗi", "Tên dịch vụ phải từ 2 ký tự!", "error");
            return;
        }

        if (!loai) {
            Swal.fire("Lỗi", "Chọn loại dịch vụ!", "error");
            return;
        }

        if (isNaN(gia) || gia <= 0) {
            Swal.fire("Lỗi", "Giá phải > 0!", "error");
            return;
        }

        if (isNaN(ton) || ton < 0) {
            Swal.fire("Lỗi", "Tồn kho không hợp lệ!", "error");
            return;
        }

        let formData = new FormData(this);

        $.ajax({
            url: "/Dichvus/CreateAjax",
            method: "POST",
            processData: false,
            contentType: false,
            data: formData,

            success: function (res) {
                if (!res.success) {
                    Swal.fire("Lỗi", res.message, "error");
                    return;
                }

                Swal.fire("Thành công", "Thêm dịch vụ thành công!", "success");
                $("#addModal").modal("hide");
                loadData(true);
            }
        });
    });


    /* ================================================================
       11. PREVIEW IMAGE
    ================================================================ */
    $("#input_add_img").change(function () {
        let f = this.files[0];
        if (f) $("#preview_add_img").attr("src", URL.createObjectURL(f));
    });

    $(document).on("change", "#editImage", function () {
        let f = this.files[0];
        if (f) $("#editPreview").attr("src", URL.createObjectURL(f));
    });


    /* ================================================================
       12. EXPORT
    ================================================================ */
    $("#btnDvExportExcel").click(function () {
        window.location = "/Dichvus/ExportExcel?" + filterForm.serialize();
    });


    /* ================================================================
       13. THÊM LOẠI DỊCH VỤ
    ================================================================ */
    $(document).on("click", "#btnSaveLoai", function () {
        let tenLoai = $("#tenLoaiMoi").val().trim();

        if (tenLoai === "") {
            $("#loaiError")
                .text("Tên loại không được để trống")
                .removeClass("d-none");
            return;
        }

        $.post("/Loaidichvus/CreateAjax", { tenLoai: tenLoai }, function (res) {

            if (res.success) {
                Swal.fire("Thành công", res.message, "success");
                $("#addLoaiModal").modal("hide");
                $("#tenLoaiMoi").val("");
                location.reload();
            } else {
                $("#loaiError")
                    .text(res.message)
                    .removeClass("d-none");
            }
        });
    });

});
