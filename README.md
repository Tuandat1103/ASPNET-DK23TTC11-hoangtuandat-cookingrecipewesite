# ASPNET-DK23TTC11-hoangtuandat-cookingrecipewesite
# Cooking Recipe Website

Đây là đồ án môn học ASP.NET Core MVC, thực hiện bởi Hoàng Tuấn Đạt.

## Mục tiêu
Xây dựng website quản lý công thức nấu ăn với các chức năng:
- Đăng ký, đăng nhập, đăng xuất.
- Thêm, xem, chỉnh sửa công thức nấu ăn.
- Quản lý danh mục công thức (thêm, chỉnh sửa).
- Tìm kiếm theo tên, danh mục món ăn

## Công nghệ sử dụng
- **Backend**: ASP.NET Core MVC, Entity Framework Core
- **Database**: SQL Server
- **Frontend**: Bootstrap, HTML, CSS, JavaScript

## Hướng dẫn cài đặt
1. **Clone repository**:
   ```bash
   git clone https://github.com/Tuandat1103/ASPNET-DK23TTC11-hoangtuandat-cookingrecipewesite.git
2. **Cài đặt môi trường:**
   + Cài Visual Studio 2022 (hoặc phiên bản mới hơn) với workload ".NET desktop development".
   + Cài SQL Server Express.
3. **Cấu hình database:**
   + Cập nhật chuỗi kết nối trong appsettings.json:
      - "ConnectionStrings": {
        "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=CookingRecipeDB;Trusted_Connection=True;"
      }
   + Chạy migration:
     - dotnet ef database update
4. **Chạy dự án:**
   + Mở solution trong Visual Studio, nhấn F5.
## Tiến độ
Xem chi tiết trong thư mục /progress-report.
## Thông tin liên hệ 
Email: datht110302@sv-onuni.edu.vn
SĐT: 0374176831
