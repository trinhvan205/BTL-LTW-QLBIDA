// ==================== VARIABLES ====================
let searchTimeout;

// ==================== UTILITY FUNCTIONS ====================

// ✅ HÀM MỚI: Hiển thị thông báo alert tự động tắt (Dùng cho lỗi AJAX)
function showAutoCloseAlert(message, type = 'danger', duration = 2000) { // 2000ms = 2 giây
    const container = $('#apiAlertContainer');
    container.empty(); // Xóa thông báo cũ

    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;

    const alertElement = $(alertHtml).appendTo(container);

    // Tự động đóng sau 2 giây
    setTimeout(function () {
        alertElement.alert('close');
    }, duration);
}


// ==================== DOCUMENT READY ====================
$(document).ready(function () {
    // Load data khi trang vừa load
    loadNhanviens();

    // Tìm kiếm với debounce
    $('#searchInput').on('keyup', function () {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function () {
            loadNhanviens();
        }, 500);
    });

    // Lọc theo trạng thái
    $('input[name="trangThai"]').on('change', function () {
        loadNhanviens();
    });

    // Reset filter
    $('#btnReset').on('click', function () {
        $('#searchInput').val('');
        $('#dangLam').prop('checked', true);
        loadNhanviens();
    });

    // Check all checkbox
    $('#checkAll').on('change', function () {
        $('tbody input[type="checkbox"]').prop('checked', this.checked);
    });

    // ✅ LOGIC MỚI: Tự động đóng alert cho thông báo TempData (ID: AutoCloseAlert)
    const TEMP_DATA_TIMEOUT = 2000; // 2 giây

    var tempDataAlert = $('#AutoCloseAlert');

    if (tempDataAlert.length) {
        setTimeout(function () {
            // Sử dụng hàm đóng alert của Bootstrap
            tempDataAlert.alert('close');
        }, TEMP_DATA_TIMEOUT);
    }
});

// ==================== LOAD DATA FUNCTION ====================
function loadNhanviens() {
    const searchString = $('#searchInput').val();
    const trangThai = $('input[name="trangThai"]:checked').val();

    // Xóa alert AJAX cũ trước khi gọi API mới
    $('#apiAlertContainer').empty();

    // Show loading
    $('#loadingSpinner').show();
    $('#nhanvienTable').hide();
    $('#emptyState').hide();

    $.ajax({
        url: '/Nhanviens/GetNhanviens',
        type: 'GET',
        data: {
            searchString: searchString,
            trangThai: trangThai
        },
        success: function (response) {
            if (response.success) {
                renderTable(response.data);
            } else {
                // ✅ SỬA: Thay alert() bằng showAutoCloseAlert()
                showAutoCloseAlert('Có lỗi xảy ra: ' + response.message, 'danger');
            }
        },
        error: function () {
            // ✅ SỬA: Thay alert() bằng showAutoCloseAlert()
            showAutoCloseAlert('Không thể tải dữ liệu. Vui lòng thử lại!', 'danger');
        },
        complete: function () {
            $('#loadingSpinner').hide();
        }
    });
}

// ==================== RENDER TABLE FUNCTION ====================
function renderTable(data) {
    const tbody = $('#nhanvienTableBody');
    tbody.empty();

    // Update total count
    $('#totalCount').text(data.length);

    if (data.length === 0) {
        $('#emptyState').show();
        $('#nhanvienTable').hide();
        return;
    }

    $('#nhanvienTable').show();

    data.forEach(function (item) {
        const avatarLetter = item.hotennv ? item.hotennv.substring(0, 1).toUpperCase() : 'N';
        const gioitinh = item.gioitinh === false ? 'Nam' : 'Nữ';
        const quyenBadge = item.quyenadmin === true
            ? '<span class="badge bg-danger"><i class="fas fa-crown me-1"></i>Admin</span>'
            : '<span class="badge bg-secondary"><i class="fas fa-user me-1"></i>Nhân viên</span>';
        const trangThaiBadge = item.nghiviec === false
            ? '<span class="badge bg-success"><i class="fas fa-check-circle me-1"></i>Đang làm</span>'
            : '<span class="badge bg-secondary"><i class="fas fa-times-circle me-1"></i>Đã nghỉ</span>';

        const row = `
            <tr>
                <td class="text-center">
                    <input type="checkbox" class="form-check-input">
                </td>
                <td>
                    <div class="avatar-circle bg-primary text-white">
                        ${avatarLetter}
                    </div>
                </td>
                <td>
                    <a href="/Nhanviens/Details/${item.idnv}"
                       class="text-decoration-none fw-bold text-primary">
                        ${item.idnv}
                    </a>
                </td>
                <td>${item.tendangnhap || ''}</td>
                <td>
                    <div class="fw-bold">${item.hotennv || 'Chưa có'}</div>
                    <small class="text-muted">${gioitinh}</small>
                </td>
                <td>${item.sodt || ''}</td>
                <td>${item.cccd || ''}</td>
                <td class="text-center">${quyenBadge}</td>
                <td class="text-center">${trangThaiBadge}</td>
                <td class="text-center">
                    <div class="btn-group btn-group-sm">
                        <a href="/Nhanviens/Edit/${item.idnv}"
                           class="btn btn-outline-primary" title="Sửa">
                            <i class="fas fa-edit"></i>
                        </a>
                        <a href="/Nhanviens/Delete/${item.idnv}"
                           class="btn btn-outline-danger" title="Xóa">
                            <i class="fas fa-trash"></i>
                        </a>
                    </div>
                </td>
            </tr>
        `;
        tbody.append(row);
    });
}