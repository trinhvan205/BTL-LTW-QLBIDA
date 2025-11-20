// Hàm chọn chức năng
function selectFunction(chucNang) {
    // Kiểm tra nếu nút Quản trị bị disable
    const btnQuantri = document.getElementById('btn-quantri');
    if (chucNang === 'quantri' && btnQuantri.classList.contains('disabled')) {
        return; // Không làm gì nếu nút bị disable
    }

    // Bỏ active class khỏi tất cả nút
    document.getElementById('btn-banhang').classList.remove('active');
    document.getElementById('btn-quantri').classList.remove('active');

    // Thêm active class cho nút được chọn
    if (chucNang === 'ThuNgan') {
        document.getElementById('btn-banhang').classList.add('active');
    } else {
        document.getElementById('btn-quantri').classList.add('active');
    }

    // Cập nhật giá trị hidden input
    document.getElementById('chucNangInput').value = chucNang;
}

// Khởi tạo giá trị mặc định
document.getElementById('chucNangInput').value = 'ThuNgan';