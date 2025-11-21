// ===========================
// BIẾN TOÀN CỤC
// ===========================
let banDangChon = null;
let viewHienTai = 'ban'; // 'ban' hoặc 'dichvu'
let timeUpdateInterval = null;
let khuVucHienTai = ''; // ← THÊM: Lưu khu vực đang chọn
let loaiDvHienTai = ''; // ← THÊM: Lưu loại dịch vụ đang chọn

// ===========================
// KHỞI TẠO KHI TRANG LOAD
// ===========================
$(document).ready(function () {
    console.log('Thu Ngân System Ready ✓');

    // ← THÊM: Khởi tạo khu vực mặc định là "Tất cả"
    khuVucHienTai = '';
    loaiDvHienTai = '';

    // Load danh sách bàn mặc định
    loadDanhSachBan();


    // Xử lý nút chuyển tab (Phòng bàn / Thực đơn)
    $('#btnShowBan').click(function () {
        if (viewHienTai !== 'ban') {
            viewHienTai = 'ban';
            $(this).addClass('active');
            $('#btnShowDichVu').removeClass('active');

            $('#tabsKhuVuc').show();
            $('#tabsLoaiDv').hide();

            // ← SỬA: Load lại khu vực đã chọn trước đó
            loadDanhSachBan(khuVucHienTai);
        }
    });

    $('#btnShowDichVu').click(function () {
        // Kiểm tra đã chọn bàn chưa
        if (!banDangChon) {
            showToast('⚠️ Vui lòng chọn bàn trước khi xem thực đơn!');
            return;
        }

        if (viewHienTai !== 'dichvu') {
            viewHienTai = 'dichvu';
            $(this).addClass('active');
            $('#btnShowBan').removeClass('active');

            $('#tabsKhuVuc').hide();
            $('#tabsLoaiDv').show();

            // ← SỬA: Load lại loại dịch vụ đã chọn trước đó
            loadDanhSachDichVu(loaiDvHienTai);
        }
    });

    // Xử lý click chọn bàn
    $(document).on('click', '.ban-item', function () {
        let idBan = $(this).data('id');
        let tenBan = $(this).data('ten');
        chonBan(idBan, tenBan);
    });

    // Xử lý nút BẮT ĐẦU TÍNH GIỜ
    $(document).on('click', '.btn-batdau', function () {
        let idBan = $(this).data('ban');
        if (confirm('🎱 Bắt đầu tính giờ cho bàn này?')) {
            batDauChoi(idBan);
        }
    });

    // Xử lý click chọn dịch vụ
    $(document).on('click', '.dichvu-item:not(.disabled)', function () {
        if (!banDangChon) {
            showToast('⚠️ Vui lòng chọn bàn trước!');
            return;
        }
        let idDv = $(this).data('id');
        let tenDv = $(this).data('ten');
        themDichVu(idDv, tenDv);
    });

    // Xử lý click vào dịch vụ hết hàng
    $(document).on('click', '.dichvu-item.disabled', function () {
        showToast('❌ Dịch vụ này đã hết hàng');
    });

    // Xử lý nút +/- số lượng
    $(document).on('click', '.btn-tang', function () {
        let idHd = $(this).data('hd');
        let idDv = $(this).data('dv');
        let slHienTai = parseInt($(this).closest('.item-controls').find('.item-quantity').text());
        capNhatSoLuong(idHd, idDv, slHienTai + 1);
    });

    $(document).on('click', '.btn-giam', function () {
        let idHd = $(this).data('hd');
        let idDv = $(this).data('dv');
        let slHienTai = parseInt($(this).closest('.item-controls').find('.item-quantity').text());

        if (slHienTai > 1) {
            capNhatSoLuong(idHd, idDv, slHienTai - 1);
        } else {
            if (confirm('🗑️ Bạn có muốn xóa món này?')) {
                capNhatSoLuong(idHd, idDv, 0);
            }
        }
    });

    // Xử lý nút THANH TOÁN
    $(document).on('click', '.btn-thanhtoan', function () {
        let idHd = $(this).data('hd');
        let idBan = $(this).data('ban');

        if (!idHd) {
            showToast('❌ Không tìm thấy hóa đơn!');
            return;
        }

        // Mở modal thanh toán (không confirm)
        loadModalThanhToan(idHd, idBan);
        $('#modalThanhToan').modal('show');
    });

    // ← THAY ĐỔI: Auto refresh 2 phút (vì đã có đồng hồ chạy mỗi giây)
    setInterval(function () {
        if (banDangChon) {
            loadHoaDonChiTiet(banDangChon);
        }
    }, 120000); // 2 phút
});

// ===========================
// XỬ LÝ DROPDOWN KHU VỰC
// ===========================

// Toggle dropdown KHU VỰC
$(document).on('click', '#dropdownMoreKhuVuc', function (e) {
    e.preventDefault();
    e.stopPropagation();

    // Đóng dropdown loại DV nếu đang mở
    $('#dropdownMoreLoaiDv').removeClass('open');
    $('#menuMoreLoaiDv').removeClass('show');

    $(this).toggleClass('open');
    $('#menuMoreKhuVuc').toggleClass('show');

    console.log('Dropdown Khu Vực toggled:', $('#menuMoreKhuVuc').hasClass('show'));
});

// Click item trong dropdown KHU VỰC
$(document).on('click', '.dropdown-item-inline[data-khu]', function (e) {
    e.preventDefault();
    e.stopPropagation();

    let khuVucId = $(this).data('khu');
    let tenKhu = $(this).text().trim();

    // Cập nhật active
    $('#tabsKhuVuc .filter-btn').removeClass('active');
    $('.dropdown-item-inline[data-khu]').removeClass('active');
    $(this).addClass('active');

    // Đóng dropdown
    $('#dropdownMoreKhuVuc').removeClass('open');
    $('#menuMoreKhuVuc').removeClass('show');

    // ← THÊM: Lưu khu vực đang chọn
    khuVucHienTai = khuVucId;

    loadDanhSachBan(khuVucId);
});


// Click tabs khu vực thông thường
$(document).on('click', '#tabsKhuVuc .filter-btn:not(.dropdown-toggle-inline)', function () {
    $('#tabsKhuVuc .filter-btn').removeClass('active');
    $('.dropdown-item-inline[data-khu]').removeClass('active');
    $(this).addClass('active');

    $('#dropdownMoreKhuVuc').removeClass('active').html('<i class="bi bi-chevron-down"></i>');

    let khuVucId = $(this).data('khu');

    // ← THÊM: Lưu khu vực đang chọn
    khuVucHienTai = khuVucId;

    loadDanhSachBan(khuVucId);
});

// ===========================
// XỬ LÝ DROPDOWN LOẠI DỊCH VỤ
// ===========================

// Toggle dropdown LOẠI DỊCH VỤ
$(document).on('click', '#dropdownMoreLoaiDv', function (e) {
    e.preventDefault();
    e.stopPropagation();

    // Đóng dropdown khu vực nếu đang mở
    $('#dropdownMoreKhuVuc').removeClass('open');
    $('#menuMoreKhuVuc').removeClass('show');

    $(this).toggleClass('open');
    $('#menuMoreLoaiDv').toggleClass('show');

    console.log('Dropdown Loại DV toggled:', $('#menuMoreLoaiDv').hasClass('show'));
});

// Click item trong dropdown LOẠI DỊCH VỤ
$(document).on('click', '.dropdown-item-inline[data-loai]', function (e) {
    e.preventDefault();
    e.stopPropagation();

    let loaiDvId = $(this).data('loai');
    let tenLoai = $(this).text().trim();

    // Cập nhật active
    $('#tabsLoaiDv .filter-btn').removeClass('active');
    $('.dropdown-item-inline[data-loai]').removeClass('active');
    $(this).addClass('active');

    // Đóng dropdown
    $('#dropdownMoreLoaiDv').removeClass('open');
    $('#menuMoreLoaiDv').removeClass('show');

    // ← THÊM: Lưu loại dịch vụ đang chọn
    loaiDvHienTai = loaiDvId;

    loadDanhSachDichVu(loaiDvId);
});

// Click tabs loại dịch vụ thông thường
$(document).on('click', '#tabsLoaiDv .filter-btn:not(.dropdown-toggle-inline)', function () {
    $('#tabsLoaiDv .filter-btn').removeClass('active');
    $('.dropdown-item-inline[data-loai]').removeClass('active');
    $(this).addClass('active');

    $('#dropdownMoreLoaiDv').removeClass('active').html('<i class="bi bi-chevron-down"></i>');

    let loaiDv = $(this).data('loai');

    // ← THÊM: Lưu loại dịch vụ đang chọn
    loaiDvHienTai = loaiDv;

    loadDanhSachDichVu(loaiDv);
});

// ===========================
// ĐÓNG DROPDOWN KHI CLICK BÊN NGOÀI
// ===========================
$(document).click(function (e) {
    // Đóng dropdown khu vực
    if (!$(e.target).closest('.khuvuc-dropdown-inline').length) {
        $('#dropdownMoreKhuVuc').removeClass('open');
        $('#menuMoreKhuVuc').removeClass('show');
    }

    // Đóng dropdown loại dịch vụ
    if (!$(e.target).closest('.loaidv-dropdown-inline').length) {
        $('#dropdownMoreLoaiDv').removeClass('open');
        $('#menuMoreLoaiDv').removeClass('show');
    }
});

// ===========================
// HÀM LOAD DANH SÁCH BÀN
// ===========================
function loadDanhSachBan(khuVucId = '') {
    $.ajax({
        url: '/ThuNgan/GetDanhSachBan',
        type: 'GET',
        data: { khuVucId: khuVucId },
        success: function (html) {
            $('#contentArea').html(html);

        },
        error: function (xhr, status, error) {
            console.error('Load bàn error:', error);
            showToast('❌ Lỗi khi tải danh sách bàn!');
        }
    });
}

// ===========================
// HÀM LOAD DANH SÁCH DỊCH VỤ
// ===========================
function loadDanhSachDichVu(loaiDv = '') {
    $.ajax({
        url: '/ThuNgan/GetDanhSachDichVu',
        type: 'GET',
        data: { loaiDv: loaiDv },
        success: function (html) {
            $('#contentArea').html(html);
        },
        error: function (xhr, status, error) {
            console.error('Load dịch vụ error:', error);
            showToast('❌ Lỗi khi tải danh sách dịch vụ!');
        }
    });
}

// ===========================
// HÀM CHỌN BÀN
// ===========================
function chonBan(idBan, tenBan) {
    banDangChon = idBan;

    // Highlight bàn đã chọn
    $('.ban-item').removeClass('selected');
    $(`.ban-item[data-id="${idBan}"]`).addClass('selected');

    // Cập nhật tên bàn ở header
    $('#tenBanHienTai').text(tenBan);


    // Load hóa đơn chi tiết
    loadHoaDonChiTiet(idBan);
}

// ===========================
// HÀM LOAD HÓA ĐƠN CHI TIẾT
// ===========================
function loadHoaDonChiTiet(idBan) {
    $.ajax({
        url: '/ThuNgan/GetHoaDonChiTiet',
        type: 'GET',
        data: { idBan: idBan },
        success: function (html) {
            $('#hoaDonArea').html(html);
            // ← THÊM: Khởi động đồng hồ sau khi load hóa đơn
            startTimeUpdate();
        },
        error: function (xhr, status, error) {
            console.error('Load hóa đơn error:', error);
            showToast('❌ Lỗi khi tải hóa đơn!');
        }
    });
}

// ===========================
// CẬP NHẬT THỜI GIAN CHƠI (MỖI GIÂY)
// ===========================
function startTimeUpdate() {
    // Clear interval cũ nếu có
    if (timeUpdateInterval) {
        clearInterval(timeUpdateInterval);
    }

    // Kiểm tra xem có đang chơi không
    const gioBatDauStr = $('#gioBatDau').val();
    if (!gioBatDauStr) {
        return; // Bàn chưa mở, không cần update
    }

    // ← CẬP NHẬT MỖI 1 GIÂY
    timeUpdateInterval = setInterval(function () {
        if (banDangChon) {
            updateTimeDisplay();
        }
    }, 1000); // 1000ms = 1 giây
}

function updateTimeDisplay() {
    // Lấy giờ bắt đầu từ DOM
    const gioBatDauStr = $('#gioBatDau').val();
    if (!gioBatDauStr) return;

    const gioBatDau = new Date(gioBatDauStr);
    const now = new Date();
    const diffMs = now - gioBatDau;

    // Tính giờ, phút, giây
    const totalSeconds = Math.floor(diffMs / 1000);
    const hours = Math.floor(totalSeconds / 3600);
    const minutes = Math.floor((totalSeconds % 3600) / 60);
    const seconds = totalSeconds % 60;

    // Tổng phút
    const totalMinutes = Math.floor(totalSeconds / 60);

    // ← TÍNH THEO BLOCK 15 PHÚT (Chỉ cần > 0 thì +1 block)
    // VD: 0 phút 1 giây → totalMinutes = 0 → 0 / 15 = 0 → + 1 = 1 block → 15 phút
    // VD: 15 phút 1 giây → totalMinutes = 15 → 15 / 15 = 1 → + 1 = 2 block → 30 phút
    const blocks15Min = Math.floor(totalMinutes / 15) + 1;
    const phutTinhTien = blocks15Min * 15;

    // Cập nhật hiển thị thời gian có giây
    $('#thoiGianDisplay').html(
        `<strong>${hours} giờ ${minutes} phút ${seconds} giây</strong>`
    );

    // Tính tiền giờ theo block 15 phút
    const giaTien = parseFloat($('#giaTienBan').val()) || 0;
    const gioTinhTien = phutTinhTien / 60;
    const tienGio = gioTinhTien * giaTien;

    // Cập nhật tiền giờ
    $('#tienGioDisplay').text(tienGio.toLocaleString('vi-VN') + 'đ');

    // Cập nhật tổng tiền
    const tienDichVu = parseFloat($('#tienDichVuHidden').val()) || 0;
    const tongTien = tienGio + tienDichVu;
    $('#tongTienDisplay').text(tongTien.toLocaleString('vi-VN') + 'đ');
}

// ===========================
// HÀM BẮT ĐẦU CHƠI (MỞ BÀN)
// ===========================
function batDauChoi(idBan) {
    $.ajax({
        url: '/ThuNgan/BatDauChoi',
        type: 'POST',
        data: { idBan: idBan },
        success: function (response) {
            if (response.success) {
                showToast('✅ Đã bắt đầu tính giờ!');

                // ← SỬA: Chỉ load bàn nếu đang ở tab bàn
                if (viewHienTai === 'ban') {
                    loadDanhSachBan(khuVucHienTai);
                }

                // Load hóa đơn chi tiết (luôn load)
                if (banDangChon === idBan) {
                    loadHoaDonChiTiet(idBan);
                }
            } else {
                showToast('❌ ' + response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error('Bắt đầu chơi error:', error);
            showToast('❌ Lỗi khi mở bàn!');
        }
    });
}

// ===========================
// HÀM THÊM DỊCH VỤ
// ===========================
function themDichVu(idDv, tenDv) {
    $.ajax({
        url: '/ThuNgan/ThemDichVu',
        type: 'POST',
        data: {
            idBan: banDangChon,
            idDv: idDv,
            soLuong: 1
        },
        success: function (response) {
            if (response.success) {
                loadHoaDonChiTiet(banDangChon);
                showToast(`✅ Đã thêm ${tenDv}`);

                if (viewHienTai === 'dichvu') {
                    loadDanhSachDichVu();
                }
            } else {
                showToast('❌ ' + response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error('Thêm dịch vụ error:', error);
            showToast('❌ Lỗi khi thêm dịch vụ!');
        }
    });
}

// ===========================
// HÀM CẬP NHẬT SỐ LƯỢNG
// ===========================
function capNhatSoLuong(idHd, idDv, soLuong) {
    $.ajax({
        url: '/ThuNgan/CapNhatSoLuong',
        type: 'POST',
        data: {
            idHoaDon: idHd,
            idDv: idDv,
            soLuong: soLuong
        },
        success: function (response) {
            if (response.success) {
                loadHoaDonChiTiet(banDangChon);

                if (viewHienTai === 'dichvu') {
                    loadDanhSachDichVu();
                }
            } else {
                showToast('❌ ' + (response.message || 'Lỗi cập nhật số lượng'));
            }
        },
        error: function (xhr, status, error) {
            console.error('Cập nhật số lượng error:', error);
            showToast('❌ Lỗi khi cập nhật số lượng!');
        }
    });
}

// ===========================
// HÀM THANH TOÁN
// ===========================
// HÀM THANH TOÁN
function thanhToan(idHd, idBan) {
    // ← LẤY phương thức thanh toán
    const phuongThuc = $('#phuongThucThanhToan').val() || 'PTTT001';

    $.ajax({
        url: '/ThuNgan/ThanhToan',
        type: 'POST',
        data: {
            idHoaDon: idHd,
            phuongThucThanhToan: phuongThuc // ← THÊM
        },
        success: function (response) {
            if (response.success) {
                showToast(`✅ Thanh toán thành công!\nTổng tiền: ${response.tongTien.toLocaleString('vi-VN')}đ`);

                // ← THÊM: Mở PDF trong tab mới
                if (response.pdfUrl) {
                    window.open(response.pdfUrl, '_blank');
                }

                if (timeUpdateInterval) {
                    clearInterval(timeUpdateInterval);
                    timeUpdateInterval = null;
                }

                banDangChon = null;
                $('#tenBanHienTai').text('Chưa chọn bàn');
                loadDanhSachBan();
                $('#hoaDonArea').html(`
                    <div class="empty-state">
                        <i class="bi bi-cart-x"></i>
                        <p>Vui lòng chọn bàn</p>
                    </div>
                `);

                $('#btnShowBan').click();
            } else {
                showToast('❌ ' + response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error('Thanh toán error:', error);
            showToast('❌ Lỗi khi thanh toán!');
        }
    });
}

// ===========================
// HÀM HIỂN THỊ TOAST NOTIFICATION
// ===========================
function showToast(message) {
    $('.toast-notification').remove();

    let toast = $(`
        <div class="toast-notification">
            ${message}
        </div>
    `);

    $('body').append(toast);

    setTimeout(() => {
        toast.addClass('show');
    }, 100);

    setTimeout(() => {
        toast.removeClass('show');
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

// ===========================
// CLEANUP KHI RỜI KHỎI TRANG
// ===========================
$(window).on('beforeunload', function () {
    if (timeUpdateInterval) {
        clearInterval(timeUpdateInterval);
    }
});


// ===========================
// XỬ LÝ MODAL THANH TOÁN & PREVIEW PDF
// ===========================

let currentHoaDonId = null; // Lưu ID hóa đơn đang preview

// Khi click nút thanh toán, mở modal
$(document).on('click', '.btn-thanhtoan', function () {
    let idHd = $(this).data('hd');
    let idBan = $(this).data('ban');

    if (!idHd) {
        showToast('❌ Không tìm thấy hóa đơn!');
        return;
    }

    // Load dữ liệu vào modal
    loadModalThanhToan(idHd, idBan);

    // Hiển thị modal
    $('#modalThanhToan').modal('show');
});

function loadModalThanhToan(idHd, idBan) {
    const tenBan = $('#tenBanHienTai').text() || idBan;
    const tongTien = parseFloat($('#tongTienDisplay').text().replace(/[^\d]/g, '')) || 0;
    const tienGio = parseFloat($('#tienGioDisplay').text().replace(/[^\d]/g, '')) || 0;
    const tienDichVu = parseFloat($('#tienDichVuHidden').val()) || 0;

    // Cập nhật header
    $('#maHoaDonModal').text(idHd);
    $('#tenBanModal').text(tenBan);
    $('#thoiGianModal').text(new Date().toLocaleString('vi-VN'));

    // Load chi tiết
    let htmlChiTiet = '';
    let stt = 1;
    let soLuongItem = 0;

    // Tiền giờ
    // Tiền giờ - Tính số lượng giờ và đơn giá
    const thoiGianChoi = $('#thoiGianDisplay').text() || '0 giờ 0 phút';
    const giaTienBan = parseFloat($('#giaTienBan').val()) || 0; // Lấy giá bàn

    // Tính số giờ (số lượng)
    let soGioDisplay = 0;
    const gioBatDau = $('#gioBatDau').val();
    if (gioBatDau) {
        const startTime = new Date(gioBatDau);
        const now = new Date();
        const diffMs = now - startTime;
        const totalMinutes = Math.floor(diffMs / 60000);

        // Tính block 15 phút
        const blocks = Math.floor(totalMinutes / 15) + 1;
        const phutTinhTien = blocks * 15;
        soGioDisplay = phutTinhTien / 60; // Chuyển sang giờ (VD: 15p = 0.25, 60p = 1)
    }

    htmlChiTiet += `
    <tr>
        <td>
            <div style="font-weight: 500;">${stt}. Tiền giờ chơi</div>
            <div style="font-size: 11px; color: #6b7280;">
                <i class="bi bi-clock"></i> ${thoiGianChoi}
            </div>
        </td>
        <td class="text-center">${soGioDisplay.toFixed(2)}</td>
        <td class="text-end">${giaTienBan.toLocaleString('vi-VN')}đ/giờ</td>
        <td class="text-end"><strong>${tienGio.toLocaleString('vi-VN')}đ</strong></td>
    </tr>
    `;
    stt++;
    soLuongItem++;

    // Dịch vụ
    $('.dichvu-row').each(function () {
        const tenDv = $(this).find('.dichvu-name-hd').text().trim();
        const giaDv = $(this).find('.dichvu-price-hd').text().replace(/[^\d]/g, '');
        const soLuong = $(this).find('.item-quantity').text().trim();
        const thanhTien = $(this).find('.dichvu-total').text().replace(/[^\d]/g, '');

        if (tenDv && soLuong) {
            htmlChiTiet += `
                <tr>
                    <td><div style="font-weight: 500;">${stt}. ${tenDv}</div></td>
                    <td class="text-center">${soLuong}</td>
                    <td class="text-end">${parseInt(giaDv).toLocaleString('vi-VN')}</td>
                    <td class="text-end"><strong>${parseInt(thanhTien).toLocaleString('vi-VN')}</strong></td>
                </tr>
            `;
            stt++;
            soLuongItem++;
        }
    });

    $('#chiTietDichVuModal').html(htmlChiTiet);

    // Cập nhật tổng tiền
    $('#soLuongItemModal').text(soLuongItem);
    $('#tongTienFooterModal').text(tongTien.toLocaleString('vi-VN'));
    $('#tongTienHangModal').text(tongTien.toLocaleString('vi-VN'));
    $('#khachCanTraModal').text(tongTien.toLocaleString('vi-VN'));

    // Set input
    $('#khachThanhToanInput').val(tongTien.toLocaleString('vi-VN')).data('raw-value', tongTien);

    updateTienThua();

    $('#btnXacNhanThanhToan').data('hd', idHd).data('ban', idBan);
}

// Xử lý quick money buttons
$(document).on('click', '.btn-quick-money', function () {
    const value = parseInt($(this).data('value'));
    $('#khachThanhToanInput').val(value.toLocaleString('vi-VN')).data('raw-value', value);
    updateTienThua();
});

// Xử lý input tiền khách thanh toán
$(document).on('input', '#khachThanhToanInput', function () {
    const value = $(this).val().replace(/[^\d]/g, '');
    const numberValue = parseInt(value) || 0;
    $(this).val(numberValue.toLocaleString('vi-VN')).data('raw-value', numberValue);
    updateTienThua();
});

// Tính tiền thừa
function updateTienThua() {
    const tongTien = parseFloat($('#tongTienDisplay').text().replace(/[^\d]/g, '')) || 0;
    const khachTra = $('#khachThanhToanInput').data('raw-value') || 0;
    const tienThua = khachTra - tongTien;

    if (tienThua >= 0) {
        $('#tienThuaModal').text(tienThua.toLocaleString('vi-VN') + 'đ');
    } else {
        $('#tienThuaModal').html('<span style="color: #ef4444;">Chưa đủ</span>');
    }
}

// Xác nhận thanh toán → Thanh toán & Hiện preview PDF
$(document).on('click', '#btnXacNhanThanhToan', function () {
    const idHd = $(this).data('hd');
    const idBan = $(this).data('ban');
    const phuongThuc = $('input[name="phuongThucTT"]:checked').val() || 'PTTT001';
    const khachTra = $('#khachThanhToanInput').data('raw-value') || 0;
    const tongTien = parseFloat($('#tongTienDisplay').text().replace(/[^\d]/g, '')) || 0;

    if (khachTra < tongTien) {
        showToast('❌ Số tiền khách trả không đủ!');
        return;
    }

    // Gọi thanh toán (đã đóng bàn + tạo PDF tạm)
    thanhToanVaPreview(idHd, idBan, phuongThuc);
});

function thanhToanVaPreview(idHd, idBan, phuongThuc) {
    $.ajax({
        url: '/ThuNgan/ThanhToan',
        type: 'POST',
        data: {
            idHoaDon: idHd,
            phuongThucThanhToan: phuongThuc
        },
        success: function (response) {
            if (response.success) {
                // Bàn đã đóng rồi!
                showToast(`✅ ${response.message}\nTổng tiền: ${response.tongTien.toLocaleString('vi-VN')}đ`);

                // Đóng modal thanh toán
                $('#modalThanhToan').modal('hide');

                // Lưu ID
                currentHoaDonId = response.idHoaDon;

                // Hiển thị PDF trong iframe
                $('#pdfPreviewFrame').attr('src', response.pdfUrl);

                // Mở modal preview
                $('#modalPreviewPdf').modal('show');
            } else {
                showToast('❌ ' + response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error('Thanh toán error:', error);
            showToast('❌ Lỗi khi thanh toán!');
        }
    });
}

// Click "Lưu & In PDF" → Lưu PDF + Mở để in
$(document).on('click', '#btnXacNhanIn', function () {
    if (!currentHoaDonId) {
        showToast('❌ Không tìm thấy hóa đơn!');
        return;
    }

    $.ajax({
        url: '/ThuNgan/XacNhanIn',
        type: 'POST',
        data: { idHoaDon: currentHoaDonId },
        success: function (response) {
            if (response.success) {
                showToast('✅ Đã lưu hóa đơn PDF');

                // Đóng modal preview
                $('#modalPreviewPdf').modal('hide');

                // Mở PDF để in
                window.open(response.pdfUrl, '_blank');

                // Reset
                resetAfterPayment();
            } else {
                showToast('❌ ' + response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error('Xác nhận in error:', error);
            showToast('❌ Lỗi khi lưu PDF!');
        }
    });
});

// Click "Không lưu PDF" → Xóa PDF tạm
$(document).on('click', '#btnHuyIn', function () {
    if (!currentHoaDonId) {
        $('#modalPreviewPdf').modal('hide');
        resetAfterPayment();
        return;
    }

    $.ajax({
        url: '/ThuNgan/HuyIn',
        type: 'POST',
        data: { idHoaDon: currentHoaDonId },
        success: function (response) {
            showToast('ℹ️ Đã thanh toán nhưng không lưu PDF');

            // Đóng modal preview
            $('#modalPreviewPdf').modal('hide');

            // Reset (bàn đã đóng rồi)
            resetAfterPayment();
        },
        error: function (xhr, status, error) {
            console.error('Hủy in error:', error);
            showToast('❌ Lỗi khi hủy lưu PDF!');
        }
    });
});

// Reset sau khi thanh toán
function resetAfterPayment() {
    if (timeUpdateInterval) {
        clearInterval(timeUpdateInterval);
        timeUpdateInterval = null;
    }

    currentHoaDonId = null;
    banDangChon = null;

    $('#tenBanHienTai').text('Chưa chọn bàn');
    loadDanhSachBan();
    $('#hoaDonArea').html(`
        <div class="empty-state">
            <i class="bi bi-cart-x"></i>
            <p>Vui lòng chọn bàn</p>
        </div>
    `);

    $('#btnShowBan').click();
}

// Keyboard shortcuts cho modal preview
$('#modalPreviewPdf').on('keydown', function (e) {
    if (e.key === 'Enter') {
        e.preventDefault();
        $('#btnXacNhanIn').click();
    }
    if (e.key === 'Escape') {
        e.preventDefault();
        $('#btnHuyIn').click();
    }
});

// Keyboard shortcuts cho modal thanh toán
$('#modalThanhToan').on('keydown', function (e) {
    // Enter - Xác nhận thanh toán
    if (e.key === 'Enter' && !$(e.target).is('button')) {
        e.preventDefault();
        $('#btnXacNhanThanhToan').click();
    }

    // F8 - Focus vào input số tiền
    if (e.key === 'F8') {
        e.preventDefault();
        $('#khachThanhToanInput').focus().select();
    }
});

// Auto focus khi mở modal thanh toán
$('#modalThanhToan').on('shown.bs.modal', function () {
    $('#khachThanhToanInput').focus().select();
});

// Cleanup khi đóng modal preview
$('#modalPreviewPdf').on('hidden.bs.modal', function () {
    $('#pdfPreviewFrame').attr('src', '');
    currentHoaDonId = null;
});


// ===========================
// XỬ LÝ KHI ĐÓNG MODAL (HỦY THANH TOÁN)
// ===========================

// Khi modal đóng (bấm X hoặc Hủy), tiếp tục đếm giờ
$('#modalThanhToan').on('hidden.bs.modal', function () {
    console.log('Modal đã đóng - Tiếp tục tính giờ');
    // Đồng hồ vẫn chạy bình thường (không cần làm gì)
    // Vì timeUpdateInterval vẫn đang chạy
});

// Khi modal mở, tạm dừng đồng hồ (tùy chọn)
$('#modalThanhToan').on('shown.bs.modal', function () {
    console.log('Modal thanh toán đã mở');
    // Focus vào input số tiền
    $('#khachThanhToanInput').focus().select();
});


// ===========================
// KEYBOARD SHORTCUTS CHO MODAL THANH TOÁN
// ===========================

// Xử lý phím tắt trong modal
$('#modalThanhToan').on('keydown', function (e) {
    // Enter - Xác nhận thanh toán
    if (e.key === 'Enter' && !$(e.target).is('button')) {
        e.preventDefault();
        $('#btnXacNhanThanhToan').click();
    }

    // ESC - Hủy (đã có sẵn trong Bootstrap)
    // F8 - Focus vào input số tiền
    if (e.key === 'F8') {
        e.preventDefault();
        $('#khachThanhToanInput').focus().select();
    }
});

// Xử lý phím số khi modal mở
$('#modalThanhToan').on('shown.bs.modal', function () {
    // Bắt phím số từ 0-9 để nhập nhanh
    $(document).on('keypress.modal', function (e) {
        if (e.key >= '0' && e.key <= '9') {
            if (!$('#khachThanhToanInput').is(':focus')) {
                $('#khachThanhToanInput').focus();
            }
        }
    });
});

// Cleanup khi đóng modal
$('#modalThanhToan').on('hidden.bs.modal', function () {
    $(document).off('keypress.modal');
});



// ===========================
// TÌM KIẾM KHÁCH HÀNG - CẢI TIẾN
// ===========================

let searchTimeout = null;
let selectedKhachHang = null;

// Xử lý input tìm kiếm
$(document).on('input', '#searchKhachHang', function () {
    const keyword = $(this).val().trim();

    clearTimeout(searchTimeout);

    if (keyword.length < 2) {
        $('#searchResults').removeClass('show');
        return;
    }

    searchTimeout = setTimeout(function () {
        timKiemKhachHang(keyword);
    }, 300);
});

// Focus vào search khi nhấn F4
$(document).on('keydown', function (e) {
    if (e.key === 'F4') {
        e.preventDefault();
        $('#searchKhachHang').focus();
    }
});

// Gọi API tìm kiếm
function timKiemKhachHang(keyword) {
    $.ajax({
        url: '/ThuNgan/SearchKhachHang',
        type: 'GET',
        data: { keyword: keyword },
        success: function (response) {
            if (response.success) {
                hienThiKetQuaTimKiem(response.data, keyword);
            }
        },
        error: function () {
            showToast('❌ Lỗi khi tìm kiếm');
        }
    });
}

// Hiển thị kết quả
// Hiển thị kết quả
function hienThiKetQuaTimKiem(data, keyword) {
    let html = '';

    if (data.length > 0) {
        data.forEach(kh => {
            html += `
                <div class="search-result-item" data-id="${kh.idKh}" data-ten="${kh.tenKh}" data-sdt="${kh.sdt}">
                    <i class="bi bi-person-circle"></i>
                    <div class="search-result-info">
                        <div class="search-result-name-phone">
                            <span class="name">${kh.tenKh}</span>
                            <span class="separator">•</span>
                            <span class="phone">${kh.sdt}</span>
                        </div>
                        <div class="search-result-id">Mã: ${kh.idKh}</div>
                    </div>
                </div>
            `;
        });
    } else {
        // ← SỬA: Chỉ hiện thông báo, không có nút
        html = `
            <div class="search-no-result">
                <i class="bi bi-search"></i>
                <div>Không tìm thấy kết quả phù hợp</div>
            </div>
        `;
    }

    $('#searchResults').html(html).addClass('show');
}

// Click chọn khách hàng
$(document).on('click', '.search-result-item', function () {
    const idKh = $(this).data('id');
    const tenKh = $(this).data('ten');
    const sdt = $(this).data('sdt');

    chonKhachHang(idKh, tenKh, sdt);
});

// Chọn khách hàng
function chonKhachHang(idKh, tenKh, sdt) {
    // Lấy ID hóa đơn hiện tại
    const idHd = $('.btn-thanhtoan').data('hd');

    if (!idHd) {
        showToast('⚠️ Vui lòng chọn bàn trước');
        return;
    }

    $.ajax({
        url: '/ThuNgan/GanKhachHang',
        type: 'POST',
        data: {
            idHoaDon: idHd,
            idKhachHang: idKh
        },
        success: function (response) {
            if (response.success) {
                selectedKhachHang = { idKh, tenKh, sdt };

                // Chuyển sang state đã chọn
                $('#customerSearchBox').hide();
                $('#customerSelectedBox').show();
                $('#selectedCustomerName').text(tenKh);

                // Ẩn kết quả
                $('#searchResults').removeClass('show');
                $('#searchKhachHang').val('');

                showToast(`✅ Đã chọn: ${tenKh}`);
            } else {
                showToast('❌ ' + response.message);
            }
        },
        error: function () {
            showToast('❌ Lỗi khi gán khách hàng');
        }
    });
}

// Xóa khách hàng đã chọn
function xoaKhachHang() {
    selectedKhachHang = null;

    // Chuyển về state chưa chọn
    $('#customerSelectedBox').hide();
    $('#customerSearchBox').show();
    $('#searchKhachHang').val('').focus();

    showToast('ℹ️ Đã bỏ khách hàng');
}

// Mở modal thêm khách hàng mới
function moModalThemKhachHang(keyword) {
    // Kiểm tra keyword có phải SĐT không
    const isPhone = /^[0-9]+$/.test(keyword);

    const tenKh = prompt('Nhập tên khách hàng:', '');
    if (tenKh === null) return;

    const sdtInput = prompt('Nhập SĐT:', isPhone ? keyword : '');
    if (!sdtInput) {
        showToast('❌ Vui lòng nhập SĐT');
        return;
    }

    $.ajax({
        url: '/ThuNgan/ThemKhachHangNhanh',
        type: 'POST',
        data: {
            tenKh: tenKh || 'Khách hàng',
            sdt: sdtInput
        },
        success: function (response) {
            if (response.success) {
                showToast('✅ ' + response.message);

                // Tự động chọn khách hàng vừa thêm
                chonKhachHang(
                    response.khachHang.idKh,
                    response.khachHang.tenKh,
                    response.khachHang.sdt
                );
            } else {
                showToast('❌ ' + response.message);
            }
        },
        error: function () {
            showToast('❌ Lỗi khi thêm khách hàng');
        }
    });
}

// Đóng dropdown khi click bên ngoài
$(document).click(function (e) {
    if (!$(e.target).closest('.customer-search-container').length) {
        $('#searchResults').removeClass('show');
    }
});

// Reset khi thanh toán xong
function resetAfterPayment() {
    if (timeUpdateInterval) {
        clearInterval(timeUpdateInterval);
        timeUpdateInterval = null;
    }

    currentHoaDonId = null;
    banDangChon = null;

    $('#tenBanHienTai').text('Chưa chọn bàn');
    loadDanhSachBan();
    $('#hoaDonArea').html(`
        <div class="empty-state">
            <i class="bi bi-cart-x"></i>
            <p>Vui lòng chọn bàn</p>
        </div>
    `);

    $('#btnShowBan').click();

    // ← THÊM: Reset khách hàng
    selectedKhachHang = null;
    $('#customerSelectedBox').hide();
    $('#customerSearchBox').show();
    $('#searchKhachHang').val('');
}




