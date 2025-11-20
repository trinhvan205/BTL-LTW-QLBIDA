// ==================== VARIABLES ====================
let searchTimeout;

// ==================== DOCUMENT READY ====================
$(document).ready(function () {
    // Load data khi trang vừa load
    loadKhachhangs();

    // Tìm kiếm với debounce
    $('#searchInput').on('keyup', function () {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function () {
            loadKhachhangs();
        }, 500); // Đợi 500ms sau khi ngừng gõ
    });

    // Sắp xếp
    $('input[name="sortBy"]').on('change', function () {
        loadKhachhangs();
    });

    // Reset filter
    $('#btnReset').on('click', function () {
        $('#searchInput').val('');
        $('#sortIdAsc').prop('checked', true); // Mặc định: Mã KH tăng dần
        loadKhachhangs();
    });

    // Check all checkbox
    $('#checkAll').on('change', function () {
        $('tbody input[type="checkbox"]').prop('checked', this.checked);
    });
});

// ==================== LOAD DATA FUNCTION ====================
function loadKhachhangs() {
    const searchString = $('#searchInput').val();
    const sortBy = $('input[name="sortBy"]:checked').val();

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
                alert('Có lỗi xảy ra: ' + response.message);
            }
        },
        error: function () {
            alert('Không thể tải dữ liệu. Vui lòng thử lại!');
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

// ==================== FORMAT CURRENCY FUNCTION ====================
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}