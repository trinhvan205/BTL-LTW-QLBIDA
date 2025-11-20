$(document).ready(function () {

    /* ================================================================
       0. CLICK NÚT THÊM MỚI → RESET FORM & SINH MÃ DV
    ============================================================== */
    $("#openAddModal").on("click", function () {

        $("#addForm")[0].reset();                      // reset form
        $("#preview_add_img").attr("src", "/images/no-image.png");

        // SINH IDDV MỚI MỖI LẦN MỞ MODAL
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

        // CHUYỂN FILTER SANG OBJECT
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
       5. CLICK HÀNG → XEM CHI TIẾT
    ============================================================== */
    /* ================================================================
       5. CLICK ROW → HIỂN THỊ PANEL CHI TIẾT BÊN PHẢI
    ================================================================ */
    tableBody.on("click", ".dv-row", function (e) {

        // Nếu click vào nút thao tác thì bỏ qua
        if ($(e.target).closest("button").length > 0) return;

        let id = $(this).data("id");

        // Highlight row đang chọn
        $(".dv-row").removeClass("active-row");
        $(this).addClass("active-row");

        // Load vào PANEL BÊN PHẢI
        $("#dichvuDetail").html(`
        <div class="text-center py-4 text-muted">
            <div class="spinner-border text-success"></div>
        </div>
    `);

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
        e.stopPropagation();  // ngăn click row
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

    // Sửa trong panel
    $(document).on("click", ".btn-edit-detail", function () {
        let id = $(this).data("id");

        $("#dvEditModalContent").html(`<div class="p-4 text-center"><div class="spinner-border"></div></div>`);

        let modal = new bootstrap.Modal(document.getElementById("dvEditModal"));
        modal.show();

        $.get("/Dichvus/GetEditForm", { id: id }, function (html) {
            $("#dvEditModalContent").html(html);
        });
    });





    function loadDetail(id) {
        $.ajax({
            url: "/Dichvus/DetailPartial",
            method: "GET",
            data: { id: id },
            success: function (html) {
                $("#dichvuDetail").html(html);
            }
        });
    }


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
       8. TOGGLE TRẠNG THÁI
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
       9. THÊM DỊCH VỤ (CREATE)
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
       11. EXPORT EXCEL
    ============================================================== */
    $("#btnDvExportExcel").click(function () {
        let qs = filterForm.serialize();
        window.location = "/Dichvus/ExportExcel?" + qs;
    });


    /* ================================================================
       12. THÊM LOẠI DỊCH VỤ
    ============================================================== */
    $("#btnSaveLoai").click(function () {

        let tenLoai = $("#tenLoaiMoi").val().trim();

        if (tenLoai === "") {
            $("#loaiError").text("Tên loại không được để trống").removeClass("d-none");
            return;
        }

        $.post("/Loaidichvus/CreateAjax", { Tenloai: tenLoai }, function (res) {
            if (res.success) {
                Swal.fire("Thành công", "Đã thêm loại!", "success");
                $("#addLoaiModal").modal("hide");
                location.reload();
            } else {
                $("#loaiError").text(res.message).removeClass("d-none");
            }
        });
    });

});
