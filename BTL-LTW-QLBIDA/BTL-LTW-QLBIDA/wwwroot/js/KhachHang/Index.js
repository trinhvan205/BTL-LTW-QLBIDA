// ==================== VARIABLES ====================
let searchTimeout;
let currentPage = 1; // ⭐ KHẮC PHỤC 1: Thêm biến trang hiện tại
const pageSize = 10;

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
        currentPage = 1; // ⭐ Reset về trang 1 khi tìm kiếm
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function () {
            loadKhachhangs();
        }, 500); // Đợi 500ms sau khi ngừng gõ
    });

    // 3. Sắp xếp
    $('input[name="sortBy"]').on('change', function () {
        currentPage = 1; // ⭐ Reset về trang 1 khi sắp xếp
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
    $('#paginationContainer').empty(); // ⭐ NEW: Xóa phân trang cũ

    $.ajax({
        url: '/Khachhangs/GetKhachhangs',
        type: 'GET',
        data: {
            searchString: searchString,
            sortBy: sortBy,
            page: currentPage, // ⭐ NEW: Gửi trang hiện tại
            pageSize: pageSize // ⭐ NEW: Gửi kích thước trang
        },
        success: function (response) {
            if (response.success) {
                // ⭐ NEW: Lấy dữ liệu và metadata phân trang
                const data = response.data.items;
                const totalPages = response.data.totalPages;
                const totalItems = response.data.totalItems;
                const page = response.data.currentPage;

                currentPage = page; // Cập nhật lại currentPage

                renderTable(data, totalItems);
                renderPagination(page, totalPages); // ⭐ NEW: Render phân trang
            } else {
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

/// ==================== RENDER TABLE FUNCTION (Đã Sửa) ====================
function renderTable(data, totalItems) { // ✅ THÊM THAM SỐ totalItems
    const tbody = $('#khachhangTableBody');
    tbody.empty();

    // Update total count
    $('#totalCount').text(totalItems); // ✅ Giờ totalItems đã có giá trị
    $('#currentPageCount').text(data.length);

    // Kiểm tra Empty State
    if (data.length === 0 && totalItems === 0) {
        $('#emptyState').show();
        $('#khachhangTable').hide();
        $('#paginationContainer').empty(); // Ẩn phân trang khi không có dữ liệu
        return;
    }

    $('#khachhangTable').show();

    data.forEach(function (item) {
        const row = `
            <tr>
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

// File: Index.js - Thêm vào cuối file

// ==================== NEW: RENDER PAGINATION FUNCTION ====================
function renderPagination(page, totalPages) {
    const paginationContainer = $('#paginationContainer');
    paginationContainer.empty();

    if (totalPages <= 1) {
        return;
    }

    let paginationHtml = `<ul class="pagination pagination-sm justify-content-end mb-0">`;

    // Nút Previous
    paginationHtml += `
        <li class="page-item ${page === 1 ? 'disabled' : ''}">
            <a class="page-link" href="#" data-page="${page - 1}">Trước</a>
        </li>
    `;

    // Hiển thị các nút số trang (tối đa 5 trang gần trang hiện tại)
    let startPage = Math.max(1, page - 2);
    let endPage = Math.min(totalPages, page + 2);

    if (startPage > 1) {
        paginationHtml += `<li class="page-item"><a class="page-link" href="#" data-page="1">1</a></li>`;
        if (startPage > 2) {
            paginationHtml += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
        }
    }

    for (let i = startPage; i <= endPage; i++) {
        paginationHtml += `
            <li class="page-item ${i === page ? 'active' : ''}">
                <a class="page-link" href="#" data-page="${i}">${i}</a>
            </li>
        `;
    }

    if (endPage < totalPages) {
        if (endPage < totalPages - 1) {
            paginationHtml += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
        }
        paginationHtml += `<li class="page-item"><a class="page-link" href="#" data-page="${totalPages}">${totalPages}</a></li>`;
    }

    // Nút Next
    paginationHtml += `
        <li class="page-item ${page === totalPages ? 'disabled' : ''}">
            <a class="page-link" href="#" data-page="${page + 1}">Sau</a>
        </li>
    `;

    paginationHtml += `</ul>`;

    paginationContainer.html(paginationHtml);

    // Xử lý sự kiện click cho các nút phân trang
    paginationContainer.find('.page-link').on('click', function (e) {
        e.preventDefault();
        const newPage = parseInt($(this).data('page'));

        if (newPage > 0 && newPage <= totalPages && newPage !== currentPage) {
            currentPage = newPage;
            loadKhachhangs();
        }
    });
}