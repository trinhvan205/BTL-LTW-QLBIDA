// ==================== VARIABLES ====================
let searchTimeout;
let currentPage = 1; // ⭐ Biến lưu trang hiện tại
const pageSize = 10; // ⭐ Kích thước trang cố định

// ==================== UTILITY FUNCTIONS ====================

// ✅ HÀM: Hiển thị thông báo alert tự động tắt (Dùng cho lỗi AJAX)
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
    // Load data khi trang vừa load (trang 1)
    loadNhanviens(1);

    // Tìm kiếm với debounce
    $('#searchInput').on('keyup', function () {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function () {
            loadNhanviens(1); // ⭐ Reset về trang 1 khi tìm kiếm
        }, 500);
    });

    // Lọc theo trạng thái
    $('input[name="trangThai"]').on('change', function () {
        loadNhanviens(1); // ⭐ Reset về trang 1 khi lọc
    });

    // Reset filter
    $('#btnReset').on('click', function () {
        $('#searchInput').val('');
        $('#dangLam').prop('checked', true);
        loadNhanviens(1); // ⭐ Reset về trang 1
    });

    // Check all checkbox (Logic cũ, có thể bỏ qua)
    $('#checkAll').on('change', function () {
        $('tbody input[type="checkbox"]').prop('checked', this.checked);
    });

    // Tự động đóng alert cho thông báo TempData
    const TEMP_DATA_TIMEOUT = 2000; // 2 giây

    var tempDataAlert = $('#AutoCloseAlert');

    if (tempDataAlert.length) {
        setTimeout(function () {
            tempDataAlert.alert('close');
        }, TEMP_DATA_TIMEOUT);
    }
});

// ==================== LOAD DATA FUNCTION ====================
// ⭐ Nhận tham số page
function loadNhanviens(page = 1) {
    const searchString = $('#searchInput').val();
    const trangThai = $('input[name="trangThai"]:checked').val();

    // Lưu lại trang hiện tại
    currentPage = page;

    // Xóa alert AJAX cũ trước khi gọi API mới
    $('#apiAlertContainer').empty();

    // Show loading
    $('#loadingSpinner').show();
    $('#nhanvienTable').hide();
    $('#emptyState').hide();
    $('#paginationContainer').empty(); // Xóa phân trang cũ

    $.ajax({
        url: '/Nhanviens/GetNhanviens',
        type: 'GET',
        data: {
            searchString: searchString,
            trangThai: trangThai,
            page: currentPage, // ⭐ Gửi trang hiện tại
            pageSize: pageSize // ⭐ Gửi kích thước trang
        },
        success: function (response) {
            if (response.success) {
                // response.data là object chứa items và pagination info
                const data = response.data.items;
                const totalItems = response.data.totalItems;
                const totalPages = response.data.totalPages;
                const currentPageFromResponse = response.data.currentPage;

                renderTable(data, totalItems);
                renderPagination(currentPageFromResponse, totalPages); // ⭐ Render phân trang
            } else {
                showAutoCloseAlert('Có lỗi xảy ra: ' + response.message, 'danger');
            }
        },
        error: function () {
            showAutoCloseAlert('Không thể tải dữ liệu. Vui lòng thử lại!', 'danger');
        },
        complete: function () {
            $('#loadingSpinner').hide();
        }
    });
}

// ==================== RENDER TABLE FUNCTION ====================
// ⭐ Nhận thêm tham số totalItems
function renderTable(data, totalItems) {
    const tbody = $('#nhanvienTableBody');
    tbody.empty();

    // Update total count
    $('#totalCount').text(totalItems);

    if (data.length === 0 && totalItems === 0) {
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


// ==================== RENDER PAGINATION FUNCTION ====================
// ⭐ HÀM MỚI: Dựng các nút phân trang
function renderPagination(currentPage, totalPages) {
    const paginationContainer = $('#paginationContainer');
    paginationContainer.empty();

    if (totalPages <= 1) {
        return;
    }

    let html = '<ul class="pagination justify-content-center">';

    // Nút "Trước" (Previous)
    html += `<li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
                <a class="page-link" href="#" data-page="${currentPage - 1}">Trước</a>
             </li>`;

    // Hiển thị tối đa 5 trang xung quanh trang hiện tại
    const maxPagesToShow = 5;
    let startPage = Math.max(1, currentPage - Math.floor(maxPagesToShow / 2));
    let endPage = Math.min(totalPages, startPage + maxPagesToShow - 1);

    // Điều chỉnh nếu số lượng trang nhỏ hơn maxPagesToShow
    if (endPage - startPage + 1 < maxPagesToShow) {
        startPage = Math.max(1, endPage - maxPagesToShow + 1);
    }

    // Hiển thị các số trang
    for (let i = startPage; i <= endPage; i++) {
        html += `<li class="page-item ${i === currentPage ? 'active' : ''}">
                    <a class="page-link" href="#" data-page="${i}">${i}</a>
                 </li>`;
    }

    // Nút "Sau" (Next)
    html += `<li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
                <a class="page-link" href="#" data-page="${currentPage + 1}">Sau</a>
             </li>`;

    html += '</ul>';
    paginationContainer.append(html);

    // Gắn sự kiện click cho các nút phân trang
    paginationContainer.off('click', 'a.page-link').on('click', 'a.page-link', function (e) {
        e.preventDefault();
        const newPage = parseInt($(this).data('page'));

        // Kiểm tra tính hợp lệ trước khi tải
        if (!isNaN(newPage) && newPage > 0 && newPage <= totalPages && newPage !== currentPage) {
            loadNhanviens(newPage); // Gọi lại hàm tải dữ liệu với trang mới
        }
    });
}