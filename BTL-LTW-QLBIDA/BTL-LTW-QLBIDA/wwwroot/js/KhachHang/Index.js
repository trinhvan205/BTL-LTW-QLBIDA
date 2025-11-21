// ==================== VARIABLES ====================
let searchTimeout;

// ==================== UTILITY FUNCTIONS ====================

// Hàm hiển thị thông báo alert tự động tắt (Sử dụng cho lỗi AJAX)
function showAutoCloseAlert(message, type = 'danger', duration = 5000) {
    const container = $('#apiAlertContainer');
    container.empty(); // Xóa thông báo cũ

    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;

    const alertElement = $(alertHtml).appendTo(container);

    // Tự động đóng sau thời gian quy định (5 giây cho lỗi)
    setTimeout(function () {
        alertElement.alert('close');
    }, duration);
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

// ==================== DOCUMENT READY (Gộp tất cả logic khởi tạo) ====================
$(document).ready(function () {
    // 1. Tải dữ liệu lần đầu
    loadKhachhangs();

    // 2. Tìm kiếm với debounce
    $('#searchInput').on('keyup', function () {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function () {
            loadKhachhangs();
        }, 500); // Đợi 500ms sau khi ngừng gõ
    });

    // 3. Sắp xếp
    $('input[name="sortBy"]').on('change', function () {
        loadKhachhangs();
    });

    // 4. Reset filter
    $('#btnReset').on('click', function () {
        $('#searchInput').val('');
        $('#sortIdAsc').prop('checked', true); // Mặc định: Mã KH tăng dần
        loadKhachhangs();
    });

    // 5. Check all checkbox
    $('#checkAll').on('change', function () {
        $('tbody input[type="checkbox"]').prop('checked', this.checked);
    });

    // 6. Tự động đóng alert cho thông báo TempData (ID: AutoCloseAlert)
    const TEMP_DATA_TIMEOUT = 2500; // 2.5 giây cho TempData

    var tempDataAlert = $('#AutoCloseAlert');

    if (tempDataAlert.length) {
        setTimeout(function () {
            tempDataAlert.alert('close');
        }, TEMP_DATA_TIMEOUT);
    }
});


// ==================== LOAD DATA FUNCTION ====================
function loadKhachhangs() {
    const searchString = $('#searchInput').val();
    const sortBy = $('input[name="sortBy"]:checked').val();

    // Xóa alert AJAX cũ trước khi gọi API mới
    $('#apiAlertContainer').empty();

    // Show loading
    $('#loadingSpinner').show();
    $('#khachhangTable').hide();
    $('#emptyState').hide();

    $.ajax({
        url: '/Khachhangs/GetKhachhangs',
        type: 'GET',
        data: {
            searchString: searchString,
            sortBy: sortBy
        },
        success: function (response) {
            if (response.success) {
                renderTable(response.data);
            } else {
                // SỬA: Thay alert() bằng showAutoCloseAlert()
                showAutoCloseAlert('Có lỗi xảy ra: ' + response.message, 'danger');
            }
        },
        error: function () {
            // SỬA: Thay alert() bằng showAutoCloseAlert()
            showAutoCloseAlert('Không thể tải dữ liệu. Vui lòng thử lại!', 'danger');
        },
        complete: function () {
            $('#loadingSpinner').hide();
        }
    });
}

// ==================== RENDER TABLE FUNCTION ====================
function renderTable(data) {
    const tbody = $('#khachhangTableBody');
    tbody.empty();

    // Update total count
    $('#totalCount').text(data.length);

    if (data.length === 0) {
        $('#emptyState').show();
        $('#khachhangTable').hide();
        return;
    }

    $('#khachhangTable').show();

    data.forEach(function (item) {
        const row = `
            <tr>
                <td class="text-center">
                    <input type="checkbox" class="form-check-input">
                </td>
                <td>
                    <a href="/Khachhangs/Details/${item.idkh}"
                       class="text-decoration-none fw-bold text-primary">
                        ${item.idkh}
                    </a>
                </td>
                <td>
                    <div class="fw-bold">${item.hoten || 'Chưa cập nhật'}</div>
                </td>
                <td>${item.sodt || 'Chưa có'}</td>
                <td>${item.dchi || 'Chưa có'}</td>
                <td class="text-end">
                    <span class="badge bg-info">${item.tongHoaDon}</span>
                </td>
                <td class="text-end">
                    <strong>${formatCurrency(item.tongTien)}</strong>
                </td>
                <td class="text-center">
                    <div class="btn-group btn-group-sm">
                        <a href="/Khachhangs/Edit/${item.idkh}"
                           class="btn btn-outline-primary" title="Sửa">
                            <i class="fas fa-edit"></i>
                        </a>
                        <a href="/Khachhangs/Delete/${item.idkh}"
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