// ==================== VARIABLES ====================
let searchTimeout;

// ==================== DOCUMENT READY ====================
$(document).ready(function () {

    // Load data khi trang vừa load
    loadNhanviens();

    // Tìm kiếm với debounce
    $('#searchInput').on('keyup', function () {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function () {
            loadNhanviens();
        }, 400);
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
});


// ==================== LOAD DATA FUNCTION ====================
function loadNhanviens() {
    const searchString = $('#searchInput').val();
    const trangThai = $('input[name="trangThai"]:checked').val();

    // Show loading
    $('#loadingSpinner').show();
    $('#nhanvienTable').hide();
    $('#emptyState').hide();

    $.ajax({
        url: '/Nhanviens/GetNhanviens',   // ✔ API chính xác
        type: 'GET',
        data: {
            searchString: searchString,
            trangThai: trangThai
        },
        success: function (response) {
            if (response.success) {
                renderTable(response.data);
            } else {
                alert("Lỗi: " + response.message);
            }
        },
        error: function () {
            alert("Không thể tải dữ liệu!");
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

    // Update total
    $('#totalCount').text(data.length);

    if (data.length === 0) {
        $('#emptyState').show();
        $('#nhanvienTable').hide();
        return;
    }

    $('#nhanvienTable').show();

    data.forEach(item => {

        // Avatar ký tự đầu
        const avatarLetter = item.hotennv
            ? item.hotennv.substring(0, 1).toUpperCase()
            : "N";

        // Giới tính
        const gioitinh = item.gioitinh === false ? "Nam" : "Nữ";

        // Badge quyền
        const quyenBadge = item.quyenadmin
            ? `<span class="badge-status badge-active">
                    <i class="fas fa-crown me-1"></i>Admin
               </span>`
            : `<span class="badge-status badge-off">
                    <i class="fas fa-user me-1"></i>Nhân viên
               </span>`;

        // Badge trạng thái
        const trangThaiBadge = item.nghiviec === false
            ? `<span class="badge-status badge-active">
                    <i class="fas fa-check-circle me-1"></i>Đang làm
               </span>`
            : `<span class="badge-status badge-off">
                    <i class="fas fa-times-circle me-1"></i>Đã nghỉ
               </span>`;

        // HTML dòng
        const row = `
            <tr>

                <td>
                    <div class="avatar-circle">
                        ${avatarLetter}
                    </div>
                </td>

                <td>
                    <a href="/Nhanviens/Details/${item.idnv}"
                       class="text-decoration-none fw-bold text-primary">
                        ${item.idnv}
                    </a>
                </td>

                <td>${item.tendangnhap || ""}</td>

                <td>
                    <div class="fw-bold">${item.hotennv || "Chưa có"}</div>
                    <small class="text-muted">${gioitinh}</small>
                </td>

                <td>${item.sodt || ""}</td>

                <td>${item.cccd || ""}</td>

                <td class="text-center">${quyenBadge}</td>

                <td class="text-center">${trangThaiBadge}</td>

                <td class="text-center">
                    <div class="d-flex justify-content-center">

                        <a href="/Nhanviens/Details/${item.idnv}" 
                           class="action-btn action-view" title="Chi tiết">
                            <i class="fas fa-eye"></i>
                        </a>

                        <a href="/Nhanviens/Edit/${item.idnv}"
                           class="action-btn action-edit" title="Sửa">
                            <i class="fas fa-edit"></i>
                        </a>

                        <a href="/Nhanviens/Delete/${item.idnv}"
                           class="action-btn action-delete" title="Xóa">
                            <i class="fas fa-trash"></i>
                        </a>

                    </div>
                </td>

            </tr>
        `;

        tbody.append(row);
    });
}

