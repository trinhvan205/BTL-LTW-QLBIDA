    // === Nội dung file: wwwroot/js/Ban.js ===

    $(document).ready(function () {

        // === 0. HÀM HỖ TRỢ: LẤY THAM SỐ PHÂN TRANG (CẢI TIẾN) ===
        function getPagingParams() {
            const tableContainer = $('#tableContainer');
            let page = 1;

            // --- 1. Lấy số trang hiện tại ---
            const activePageElement = tableContainer.find('.pagination .page-item.active a.pagination-link');
            if (activePageElement.length > 0) {
                page = activePageElement.data('page');
            } else {
                const nextPageLink = tableContainer.find('.pagination-link[title="Trang sau"]');
                const lastPageLink = tableContainer.find('.pagination-link[title="Trang cuối"]');

                if (nextPageLink.length > 0 && nextPageLink.parent().is(':not(.disabled)')) {
                    page = nextPageLink.data('page') - 1;
                } else if (lastPageLink.length > 0 && lastPageLink.parent().is('.disabled')) {
                    page = lastPageLink.data('page');
                }
            }

            // --- 2. Lấy PageSize ---
            const pageSize = $('#pageSizeButton').text().trim();

            return {
                page: page,
                pageSize: pageSize,
                data: { page: page, pageSize: pageSize }
            };
        }

        // Hàm phụ trợ để gọi applyFilters
        function getCurrentPageNumber() {
            return getPagingParams().page;
        }

        // === 0. HÀM TOAST CHUYÊN NGHIỆP (Giữ nguyên) ===
        var toastEl = document.getElementById('liveToast');
        var toast = new bootstrap.Toast(toastEl);

        function showCustomToast(message, isSuccess) {
            const $toast = $('#liveToast');
            const $toastMessage = $('#toastMessage');
            const $toastIcon = $('#toastIcon');

            $toastMessage.text(message);
            $toast.removeClass('bg-success bg-danger text-white');

            if (isSuccess) {
                $toast.addClass('bg-success');
                $toastIcon.removeClass('bi-x-octagon-fill').addClass('bi-check-circle-fill');
            } else {
                $toast.addClass('bg-danger');
                $toastIcon.removeClass('bi-check-circle-fill').addClass('bi-x-octagon-fill');
            }

            toast.show();
        }

        // === 1. KHỞI TẠO MODAL (Giữ nguyên) ===
        var modal = new bootstrap.Modal(document.getElementById('phongBanModal'));
        var modalBody = $('#modalBody');

        var khuVucModal = new bootstrap.Modal(document.getElementById('khuVucModal'));
        var formKhuVuc = $('#formThemKhuVuc');
        var inputTenKhuVuc = $('#inputTenKhuVuc');
        var khuVucError = $('#khuVucError');

        var editKhuVucModal = new bootstrap.Modal(document.getElementById('editKhuVucModal'));
        var formSuaKhuVuc = $('#formSuaKhuVuc');
        var inputEditTenKhuVuc = $('#editTenKhuVuc');
        var inputEditIdKhu = $('#editIdKhu');
        var inputEditGhiChu = $('#editGhiChu');
        var editKhuVucError = $('#editKhuVucError');

        var deleteKhuVucModal = new bootstrap.Modal(document.getElementById('deleteKhuVucModal'));
        var btnConfirmDeleteKhuVuc = $('#btnConfirmDeleteKhuVuc');
        var deleteModalMessage = $('#deleteModalMessage');

        var editBanModal = new bootstrap.Modal(document.getElementById('editBanModal'));
        var deleteBanModal = new bootstrap.Modal(document.getElementById('deleteBanModal'));

        // === 2. GÁN SỰ KIỆN & HÀM (Giữ nguyên) ===
        // Xử lý mũi tên
        var collapseToggle = $('[data-bs-target="#collapseTrangThai"]');
        var collapseIcon = collapseToggle.find('.collapse-arrow');
        $('#collapseTrangThai').on('show.bs.collapse', function () { collapseIcon.removeClass('bi-chevron-down').addClass('bi-chevron-up'); });
        $('#collapseTrangThai').on('hide.bs.collapse', function () { collapseIcon.removeClass('bi-chevron-up').addClass('bi-chevron-down'); });
        // Mở modal Thêm Khu Vực
        $('#btnAddKhuVuc').on('click', function () {
            formKhuVuc[0].reset();
            inputTenKhuVuc.removeClass('is-invalid');
            khuVucError.text('');
            khuVucModal.show();
        });

        // Ẩn/Hiện nút Sửa Khu Vực
        var editKhuVucButton = $('#btnEditKhuVuc');
        var khuVucSelect = $('#khuVucSelect');
        function updateEditButtonVisibility() {
            var selectedValue = khuVucSelect.val();
            if (selectedValue === "") { editKhuVucButton.hide(); }
            else { editKhuVucButton.show(); }
        }
        updateEditButtonVisibility();

        // Xử lý dropdown Số Bản Ghi
        $('#pageSizeMenu').on('click', '.dropdown-item', function (e) {
            e.preventDefault();
            var $this = $(this);
            var selectedValue = $this.data('value');
            $('#pageSizeButton').text(selectedValue);
            $('#pageSizeMenu .dropdown-item').removeClass('active').find('i.bi-check').remove();
            $this.addClass('active').append(' <i class="bi bi-check float-end"></i>');
            applyFilters(1);
        });

        // === 3. CODE LỌC AJAX (applyFilters) (Giữ nguyên) ===
        function applyFilters(page = 1) {
            var khuVuc = $('#khuVucSelect').val();
            var trangThai = $('input[name="SelectedTrangThai"]:checked').val();
            var timKiem = $('#searchString').val();
            var pageSize = $('#pageSizeButton').text().trim();

            $('#tableContainer').html('<div class="d-flex justify-content-center py-5"><div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div></div>');

            $.ajax({
                url: Urls.FilterBan,
                type: 'GET',
                data: {
                    khuVuc: khuVuc,
                    trangThai: trangThai,
                    timKiem: timKiem,
                    pageSize: pageSize,
                    page: page
                },
                success: function (result) {
                    $('#tableContainer').html(result);
                },
                error: function () {
                    $('#tableContainer').html('<p class="text-danger">Không thể tải dữ liệu.</p>');
                }
            });
        }

        // === 4. GÁN SỰ KIỆN LỌC (Giữ nguyên) ===
        $('#khuVucSelect').on('change', function () {
            applyFilters(1);
            updateEditButtonVisibility();
        });
        $('input[name="SelectedTrangThai"]').on('change', function () { applyFilters(1); });
        $('#searchString').on('keypress', function (e) {
            if (e.which == 13) {
                e.preventDefault();
                applyFilters(1);
            }
        });
        $('#tableContainer').on('click', '.pagination-link', function (e) {
            e.preventDefault();
            var page = $(this).data('page');
            if (page) {
                applyFilters(page);
            }
        });


        // === 5. CODE MODAL (THÊM BÀN) ===
        $('#btnShowCreateModal').on('click', function () {
            $.get(Urls.CreateBan, function (data) {
                modalBody.html(data);
                var form = $('#formThemBan');
                if (form.length > 0) {
                    $.validator.unobtrusive.parse(form);
                    // 🟢 GẮN LỌC SỐ KHI MODAL THÊM BÀN TẢI XONG
                    attachNumericInputFilter('#formThemBan input[name="Giatien"]');
                }
            });
        });
        $('#btnLuu').on('click', function () {
            var form = $('#formThemBan');
            if (form.valid()) {
                const pagingParams = getPagingParams();

                // 🟢 LẤY FORM DATA VÀ ĐÍNH KÈM THAM SỐ PHÂN TRANG
                var formDataArray = form.serializeArray();
                formDataArray.push({ name: 'page', value: pagingParams.page });
                formDataArray.push({ name: 'pageSize', value: pagingParams.pageSize });

                $.ajax({
                    url: Urls.CreateBan,
                    method: 'POST',
                    data: formDataArray, // 👈 GỬI DỮ LIỆU ĐÃ CÓ page VÀ pageSize
                    success: function (response) {
                        if (response.success) {
                            modal.hide();
                            showCustomToast("Thêm bàn thành công!", true);
                            applyFilters(pagingParams.page); // GIỮ TRANG HIỆN TẠI
                        } else {
                            modalBody.html(response);
                            $.validator.unobtrusive.parse(form);
                            // 🟢 GẮN LỌC SỐ LẠI NẾU CÓ LỖI VÀ FORM TẢI LẠI
                            attachNumericInputFilter('#formThemBan input[name="Giatien"]');
                        }
                    },
                    error: function (xhr) {
                        modal.hide();
                        showCustomToast("Lỗi hệ thống: Không thể thêm bàn.", false);
                    }
                });
            }
        });
        $('#phongBanModal').on('hidden.bs.modal', function () {
            modalBody.html('<div class="d-flex justify-content-center"><div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div></div>');
        });


        // === 6. CODE MODAL (THÊM KHU VỰC) ===
        $('#btnLuuKhuVuc').on('click', function () {
            var tenKhuVuc = inputTenKhuVuc.val().trim();
            if (tenKhuVuc === "") {
                inputTenKhuVuc.addClass('is-invalid');
                khuVucError.text('Tên khu vực không được để trống.');
                return;
            }

            const pagingParams = getPagingParams();

            // 🟢 LẤY FORM DATA VÀ ĐÍNH KÈM THAM SỐ PHÂN TRANG
            var postData = formKhuVuc.serializeArray();
            postData.push({ name: 'page', value: pagingParams.page });
            postData.push({ name: 'pageSize', value: pagingParams.pageSize });

            $.ajax({
                url: Urls.CreateKhuVuc,
                method: 'POST',
                data: postData, // 👈 GỬI DỮ LIỆU ĐÃ CÓ page VÀ pageSize
                success: function (response) {
                    if (response.success) {
                        var newKhu = response.newKhuVuc;
                        var newOption = new Option(newKhu.ten, newKhu.id);

                        $('#khuVucSelect').append(newOption);
                        $('#khuVucSelect').val(newKhu.id);

                        showCustomToast(response.message, true);
                        applyFilters(pagingParams.page); // GIỮ TRANG HIỆN TẠI
                        updateEditButtonVisibility();
                        khuVucModal.hide();

                        var newOptionClone = $(new Option(newKhu.ten, newKhu.id));
                        $('#modalBody').find('select[name="Idkhu"]').append(newOptionClone.clone());
                        $('#editBanModal').find('select[name="Idkhu"]').append(newOptionClone.clone());

                    } else {
                        inputTenKhuVuc.addClass('is-invalid');
                        khuVucError.text(response.message);
                        showCustomToast(response.message, false);
                    }
                },
                error: function () {
                    showCustomToast('Đã xảy ra lỗi không mong muốn, vui lòng thử lại.', false);
                }
            });
        });

        // === 7. CODE MỞ MODAL (SỬA KHU VỰC) (Giữ nguyên) ===
        $('#btnEditKhuVuc').on('click', function () {
            var selectedId = khuVucSelect.val();
            if (selectedId === "") return;

            inputEditTenKhuVuc.removeClass('is-invalid');
            editKhuVucError.text('');

            $.ajax({
                url: Urls.GetKhuVucDetails,
                type: 'GET',
                data: { id: selectedId },
                success: function (data) {
                    inputEditIdKhu.val(data.idkhu);
                    inputEditTenKhuVuc.val(data.tenkhu);
                    inputEditGhiChu.val(data.ghichu);
                    editKhuVucModal.show();
                },
                error: function () {
                    showCustomToast('Không thể tải thông tin khu vực.', false);
                }
            });
        });

        // === 8. CODE LƯU MODAL (SỬA KHU VỰC) ===
        $('#btnLuuKhuVucMoi').on('click', function () {
            var tenKhuVuc = inputEditTenKhuVuc.val().trim();
            if (tenKhuVuc === "") {
                inputEditTenKhuVuc.addClass('is-invalid');
                editKhuVucError.text('Tên khu vực không được để trống.');
                return;
            }

            const pagingParams = getPagingParams();

            // 🟢 LẤY FORM DATA VÀ ĐÍNH KÈM THAM SỐ PHÂN TRANG
            var postData = formSuaKhuVuc.serializeArray();
            postData.push({ name: 'page', value: pagingParams.page });
            postData.push({ name: 'pageSize', value: pagingParams.pageSize });


            $.ajax({
                url: Urls.UpdateKhuVuc,
                method: 'POST',
                data: postData, // 👈 GỬI DỮ LIỆU ĐÃ CÓ page VÀ pageSize
                success: function (response) {
                    if (response.success) {
                        var updatedId = inputEditIdKhu.val();
                        $(`option[value="${updatedId}"]`).text(tenKhuVuc);

                        editKhuVucModal.hide();
                        showCustomToast(response.message, true);
                        applyFilters(pagingParams.page); // GIỮ TRANG HIỆN TẠI
                    } else {
                        inputEditTenKhuVuc.addClass('is-invalid');
                        editKhuVucError.text(response.message);
                        showCustomToast(response.message, false);
                    }
                },
                error: function () {
                    showCustomToast('Đã xảy ra lỗi khi cập nhật.', false);
                }
            });
        });

        // === 9. CODE MỚI: MỞ MODAL XÁC NHẬN XÓA (Giữ nguyên) ===
        $('#btnXoaKhuVuc').on('click', function () {
            var idCanXoa = inputEditIdKhu.val();
            var tenKhuVuc = inputEditTenKhuVuc.val();

            deleteModalMessage.html('Bạn có chắc chắn muốn xóa khu vực <strong>' + tenKhuVuc + '</strong> không?');

            btnConfirmDeleteKhuVuc.data('id-to-delete', idCanXoa);

            editKhuVucModal.hide();
            deleteKhuVucModal.show();
        });

        // === 10. CODE MỚI: XÁC NHẬN XÓA (Click nút "Đồng ý") ===
        $('#btnConfirmDeleteKhuVuc').on('click', function () {
            var idCanXoa = $(this).data('id-to-delete');
            var token = formSuaKhuVuc.find('input[name="__RequestVerificationToken"]').val();

            const pagingParams = getPagingParams(); // 🟢 Lấy tất cả tham số phân trang

            // 🟢 GỬI DỮ LIỆU ĐÃ CÓ page VÀ pageSize (Dạng object)
            var postData = {
                id: idCanXoa,
                __RequestVerificationToken: token,
                page: pagingParams.page,
                pageSize: pagingParams.pageSize
            };

            $.ajax({
                url: Urls.DeleteKhuVuc,
                method: 'POST',
                data: postData, // 👈 GỬI DỮ LIỆU ĐÃ CÓ page VÀ pageSize
                success: function (response) {
                    deleteKhuVucModal.hide();

                    if (response.success) {
                        $(`option[value="${idCanXoa}"]`).remove();
                        khuVucSelect.val('');

                        showCustomToast(response.message, true);
                        applyFilters(pagingParams.page); // GIỮ TRANG HIỆN TẠI
                        updateEditButtonVisibility();
                    } else {
                        showCustomToast(response.message, false);
                    }
                },
                error: function () {
                    deleteKhuVucModal.hide();
                    showCustomToast('Đã xảy ra lỗi khi xóa khu vực.', false);
                }
            });
        });

        // ================================================================
        // BƯỚC 4: XỬ LÝ CLICK VÀO DÒNG ĐỂ HIỆN CHI TIẾT (Giữ nguyên)
        // ================================================================

        $('#tableContainer').on('click', 'tr.clickable-row', function () {
            var tr = $(this);
            var id = tr.data('id');
            var detailRow = $('#detail-row-' + id);
            var detailContent = $('#detail-content-' + id);

            if (detailRow.is(':visible')) {
                detailRow.hide();
                tr.removeClass('table-active');
            }
            else {
                $('.detail-row').hide();
                $('tr.clickable-row').removeClass('table-active');

                tr.addClass('table-active');
                detailRow.show();

                if (detailContent.html().trim() !== "") {
                    return;
                }

                detailContent.html('<div class="text-center p-3"><div class="spinner-border text-primary" role="status"></div></div>');

                $.ajax({
                    url: Urls.GetBanDetail,
                    type: 'GET',
                    data: { id: id },
                    success: function (result) {
                        detailContent.html(result);
                    },
                    error: function () {
                        detailContent.html('<div class="text-danger p-3">Lỗi tải dữ liệu chi tiết.</div>');
                    }
                });
            }
        });

        // ================================================================
        // XỬ LÝ SỬA & XÓA BÀN
        // ================================================================

        // 1. CLICK NÚT "CẬP NHẬT" (MỞ MODAL SỬA) (Giữ nguyên)
        $(document).on('click', '.btn-open-edit-ban', function () {
            var id = $(this).data('id');

            $('#formSuaBan').removeClass('was-validated');
            $('#formSuaBan').find('.text-danger').empty();

            $.ajax({
                url: Urls.GetBanForEdit,
                type: 'GET',
                data: { id: id },
                success: function (data) {
                    $('#editIdBan').val(data.id);
                    $('#editIdKhuBan').val(data.idKhu);
                    $('#editGiaTien').val(data.giaTien);

                    // 🟢 GẮN LỌC SỐ CHO MODAL SỬA KHI TẢI XONG
                    attachNumericInputFilter('#editGiaTien');

                    editBanModal.show();
                },
                error: function () { showCustomToast("Lỗi tải dữ liệu bàn.", false); }
            });
        });

        // 2. CLICK NÚT "LƯU" (THỰC HIỆN SỬA)
        $('#btnLuuSuaBan').on('click', function () {
            var form = $('#formSuaBan');

            if (form.valid()) {

                const pagingParams = getPagingParams();

                // 🟢 LẤY FORM DATA VÀ ĐÍNH KÈM THAM SỐ PHÂN TRANG
                var formDataArray = form.serializeArray();
                formDataArray.push({ name: 'page', value: pagingParams.page });
                formDataArray.push({ name: 'pageSize', value: pagingParams.pageSize });

                $.ajax({
                    url: Urls.UpdateBan,
                    type: 'POST',
                    data: formDataArray, // 👈 GỬI DỮ LIỆU ĐÃ CÓ page VÀ pageSize
                    success: function (response) {
                        if (response.success) {
                            editBanModal.hide();
                            showCustomToast(response.message, true);
                            applyFilters(pagingParams.page); // GIỮ TRANG HIỆN TẠI
                        } else {
                            showCustomToast(response.message, false);
                        }
                    },
                    error: function () { showCustomToast("Lỗi cập nhật.", false); }
                });
            }
        });

        // 3. CLICK NÚT "XÓA" (MỞ MODAL XÓA)
        var idBanCanXoa = "";
        $(document).on('click', '.btn-open-delete-ban', function () {
            idBanCanXoa = $(this).data('id');
            $('#deleteBanName').text(idBanCanXoa);
            deleteBanModal.show();
        });

        // 4. CLICK NÚT "ĐỒNG Ý" (THỰC HIỆN XÓA BÀN)
        $('#btnConfirmDeleteBan').on('click', function () {
            var token = $('input[name="__RequestVerificationToken"]').first().val();

            const pagingParams = getPagingParams();

            // 🟢 GỬI DỮ LIỆU ĐÃ CÓ page VÀ pageSize (Dạng object)
            var postData = {
                id: idBanCanXoa,
                __RequestVerificationToken: token,
                page: pagingParams.page,
                pageSize: pagingParams.pageSize
            };

            $.ajax({
                url: Urls.DeleteBan,
                type: 'POST',
                data: postData, // 👈 GỬI DỮ LIỆU ĐÃ CÓ page VÀ pageSize
                success: function (response) {
                    deleteBanModal.hide();
                    if (response.success) {
                        showCustomToast(response.message, true);
                        applyFilters(pagingParams.page); // GIỮ TRANG HIỆN TẠI
                    } else {
                        showCustomToast(response.message, false);
                    }
                },
                error: function () { showCustomToast("Lỗi khi xóa bàn.", false); }
            });
        });

        // 5. XỬ LÝ NÚT ĐỔI TRẠNG THÁI & TOAST
        $(document).on('click', '.btn-toggle-status', function () {
            var id = $(this).data('id');
            var token = $('input[name="__RequestVerificationToken"]').first().val();

            const pagingParams = getPagingParams(); // 🟢 Lấy tất cả tham số phân trang

            // 🟢 GỬI DỮ LIỆU ĐÃ CÓ page VÀ pageSize (Dạng object)
            var postData = {
                id: id,
                __RequestVerificationToken: token,
                page: pagingParams.page,
                pageSize: pagingParams.pageSize
            };

            $.ajax({
                url: Urls.ToggleStatusBan,
                type: 'POST',
                data: postData, // 👈 GỬI DỮ LIỆU ĐÃ CÓ page VÀ pageSize
                success: function (response) {
                    if (response.success) {
                        showCustomToast(response.message, true);
                        applyFilters(pagingParams.page); // GIỮ TRANG HIỆN TẠI
                    } else {
                        showCustomToast(response.message, false);
                    }
                },
                error: function () {
                    showCustomToast('Đã xảy ra lỗi khi cập nhật trạng thái.', false);
                }
            });
        });

        // ================================================================
        // 🆕 HÀM MỚI: CHỈ CHO PHÉP NHẬP SỐ THẬP PHÂN HOẶC SỐ NGUYÊN
        // Đơn giản, hiệu quả, không bị lỗi IME
        // ================================================================

        function attachNumericInputFilter(selector) {
            const $input = $(selector);

            // Hàm kiểm tra và làm sạch giá trị
            function sanitizeNumber(value) {
                // Bước 1: Chỉ giữ lại số và dấu chấm
                let cleaned = value.replace(/[^0-9.]/g, '');

                // Bước 2: Chỉ cho phép 1 dấu chấm duy nhất
                const parts = cleaned.split('.');
                if (parts.length > 2) {
                    cleaned = parts[0] + '.' + parts.slice(1).join('');
                }

                return cleaned;
            }

            // ========== XỬ LÝ KHI NHẬP KÝ TỰ (keydown) ==========
            $input.off('keydown').on('keydown', function (e) {
                const key = e.key;
                const currentValue = this.value;
                const cursorPos = this.selectionStart;

                // Cho phép: Backspace, Delete, Tab, Escape, Enter, mũi tên
                const allowedKeys = ['Backspace', 'Delete', 'Tab', 'Escape', 'Enter',
                    'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown',
                    'Home', 'End'];

                if (allowedKeys.includes(key)) {
                    return true; // Cho phép
                }

                // Cho phép: Ctrl+A, Ctrl+C, Ctrl+V, Ctrl+X
                if (e.ctrlKey || e.metaKey) {
                    return true;
                }

                // Kiểm tra nếu là số (0-9)
                if (/^[0-9]$/.test(key)) {
                    return true; // Cho phép số
                }

                // Kiểm tra dấu chấm
                if (key === '.') {
                    // Chỉ cho phép 1 dấu chấm
                    if (currentValue.includes('.')) {
                        e.preventDefault();
                        return false;
                    }
                    // Không cho dấu chấm ở đầu
                    if (currentValue.length === 0 || cursorPos === 0) {
                        e.preventDefault();
                        return false;
                    }
                    return true; // Cho phép dấu chấm
                }

                // Chặn tất cả ký tự khác
                e.preventDefault();
                return false;
            });

            // ========== XỬ LÝ SAU KHI NHẬP (input) ==========
            // Xử lý trường hợp paste hoặc IME nhập ký tự đặc biệt
            $input.off('input').on('input', function () {
                const cursorPos = this.selectionStart;
                const originalValue = this.value;
                const originalLength = originalValue.length;

                // Làm sạch giá trị
                const cleanedValue = sanitizeNumber(originalValue);

                // Chỉ cập nhật nếu có thay đổi
                if (originalValue !== cleanedValue) {
                    this.value = cleanedValue;

                    // Điều chỉnh vị trí con trỏ
                    const removedChars = originalLength - cleanedValue.length;
                    let newCursorPos = cursorPos - removedChars;

                    // Đảm bảo con trỏ nằm trong phạm vi hợp lệ
                    newCursorPos = Math.max(0, Math.min(newCursorPos, cleanedValue.length));

                    this.setSelectionRange(newCursorPos, newCursorPos);
                }
            });

            // ========== XỬ LÝ KHI PASTE (paste) ==========
            $input.off('paste').on('paste', function (e) {
                // Lấy dữ liệu paste
                const pastedData = (e.originalEvent || e).clipboardData.getData('text/plain');

                // Làm sạch dữ liệu
                const cleanedData = sanitizeNumber(pastedData);

                // Nếu có dữ liệu hợp lệ
                if (cleanedData) {
                    e.preventDefault();

                    const cursorPos = this.selectionStart;
                    const currentValue = this.value;

                    // Chèn dữ liệu đã làm sạch vào vị trí con trỏ
                    const beforeCursor = currentValue.substring(0, cursorPos);
                    const afterCursor = currentValue.substring(this.selectionEnd);
                    const newValue = beforeCursor + cleanedData + afterCursor;

                    // Làm sạch lại toàn bộ (phòng trường hợp có nhiều dấu chấm)
                    this.value = sanitizeNumber(newValue);

                    // Đặt con trỏ sau phần vừa paste
                    const newCursorPos = cursorPos + cleanedData.length;
                    this.setSelectionRange(newCursorPos, newCursorPos);
                } else {
                    e.preventDefault(); // Chặn paste nếu không có dữ liệu hợp lệ
                }
            });

            // ========== LÀM SẠCH GIÁ TRỊ BAN ĐẦU (nếu có) ==========
            if ($input.val()) {
                $input.val(sanitizeNumber($input.val()));
            }
        }
    });