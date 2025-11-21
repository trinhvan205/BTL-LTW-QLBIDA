using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTW_QLBIDA.Models;

public partial class QlquanBilliardLtw2Context : DbContext
{
    public QlquanBilliardLtw2Context()
    {
    }

    public QlquanBilliardLtw2Context(DbContextOptions<QlquanBilliardLtw2Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Ban> Bans { get; set; }

    public virtual DbSet<Dichvu> Dichvus { get; set; }

    public virtual DbSet<Hoadon> Hoadons { get; set; }

    public virtual DbSet<Hoadondv> Hoadondvs { get; set; }

    public virtual DbSet<Khachhang> Khachhangs { get; set; }

    public virtual DbSet<Khuvuc> Khuvucs { get; set; }

    public virtual DbSet<Loaidichvu> Loaidichvus { get; set; }

    public virtual DbSet<Nhanvien> Nhanviens { get; set; }

    public virtual DbSet<Phienchoi> Phienchois { get; set; }

    // ← THÊM MỚI
    public virtual DbSet<Phuongthucthanhtoan> Phuongthucthanhtoans { get; set; }

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Ban>(entity =>
        {
            entity.HasKey(e => e.Idban).HasName("PK__BAN__9367225E8743EC36");

            entity.ToTable("BAN");


            // Code cũ giữ nguyên
            entity.Property(e => e.Idban)
                .HasMaxLength(50)
                .HasColumnName("IDBAN");
            entity.Property(e => e.Giatien)
                .HasColumnType("money")
                .HasColumnName("GIATIEN");
            entity.Property(e => e.Idkhu)
                .HasMaxLength(50)
                .HasColumnName("IDKHU");
            entity.Property(e => e.Trangthai).HasColumnName("TRANGTHAI");

            entity.HasOne(d => d.IdkhuNavigation).WithMany(p => p.Bans)
                .HasForeignKey(d => d.Idkhu)
                .HasConstraintName("FK__BAN__IDKHU__412EB0B6");
        });

        modelBuilder.Entity<Dichvu>(entity =>
        {
            entity.HasKey(e => e.Iddv).HasName("PK__DICHVU__B87DB8EA85480784");

            entity.ToTable("DICHVU");

            entity.Property(e => e.Iddv)
                .HasMaxLength(50)
                .HasColumnName("IDDV");
            entity.Property(e => e.Giatien)
                .HasColumnType("money")
                .HasColumnName("GIATIEN");
            entity.Property(e => e.Hienthi).HasColumnName("HIENTHI");
            entity.Property(e => e.Idloai)
                .HasMaxLength(50)
                .HasColumnName("IDLOAI");
            entity.Property(e => e.Soluong).HasColumnName("SOLUONG");
            entity.Property(e => e.Tendv)
                .HasMaxLength(50)
                .HasColumnName("TENDV");
            // ✅ THÊM DÒNG NÀY
            entity.Property(e => e.Imgpath)
                .HasMaxLength(255)
                .HasColumnName("IMGPATH");

            entity.HasOne(d => d.IdloaiNavigation).WithMany(p => p.Dichvus)
                .HasForeignKey(d => d.Idloai)
                .HasConstraintName("FK__DICHVU__IDLOAI__46E78A0C");
        });

        modelBuilder.Entity<Hoadon>(entity =>
        {
            entity.HasKey(e => e.Idhd).HasName("PK__HOADON__B87C1A077DAECE01");

            entity.ToTable("HOADON");

            entity.Property(e => e.Idhd)
                .HasMaxLength(50)
                .HasColumnName("IDHD");
            entity.Property(e => e.Idkh)
                .HasMaxLength(50)
                .HasColumnName("IDKH");
            entity.Property(e => e.Idnv)
                .HasMaxLength(50)
                .HasColumnName("IDNV");
            entity.Property(e => e.Idphien)
                .HasMaxLength(50)
                .HasColumnName("IDPHIEN");
            // ← THÊM MỚI
            entity.Property(e => e.Idpttt)
                .HasMaxLength(50)
                .HasColumnName("IDPTTT");
            entity.Property(e => e.Ngaylap)
                .HasColumnType("datetime")
                .HasColumnName("NGAYLAP");
            entity.Property(e => e.Tongtien)
                .HasColumnType("money")
                .HasColumnName("TONGTIEN");
            entity.Property(e => e.Trangthai).HasColumnName("TRANGTHAI");
            


            entity.HasOne(d => d.IdkhNavigation).WithMany(p => p.Hoadons)
                .HasForeignKey(d => d.Idkh)
                .HasConstraintName("FK__HOADON__IDKH__4AB81AF0");

            entity.HasOne(d => d.IdnvNavigation).WithMany(p => p.Hoadons)
                .HasForeignKey(d => d.Idnv)
                .HasConstraintName("FK__HOADON__IDNV__4BAC3F29");

            entity.HasOne(d => d.IdphienNavigation).WithMany(p => p.Hoadons)
                .HasForeignKey(d => d.Idphien)
                .HasConstraintName("FK__HOADON__IDPHIEN__49C3F6B7");
            // ← THÊM MỚI
            entity.HasOne(d => d.IdptttNavigation)
                .WithMany(p => p.Hoadons)
                .HasForeignKey(d => d.Idpttt)
                .HasConstraintName("FK_HOADON_PHUONGTHUCTHANHTOAN");
        });

        modelBuilder.Entity<Hoadondv>(entity =>
        {
            entity.HasKey(e => new { e.Idhd, e.Iddv }).HasName("PK__HOADONDV__03FBC189D16B7571");

            entity.ToTable("HOADONDV");

            entity.Property(e => e.Idhd)
                .HasMaxLength(50)
                .HasColumnName("IDHD");
            entity.Property(e => e.Iddv)
                .HasMaxLength(50)
                .HasColumnName("IDDV");
            entity.Property(e => e.Soluong).HasColumnName("SOLUONG");

            entity.HasOne(d => d.IddvNavigation).WithMany(p => p.Hoadondvs)
                .HasForeignKey(d => d.Iddv)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HOADONDV__IDDV__4F7CD00D");

            entity.HasOne(d => d.IdhdNavigation).WithMany(p => p.Hoadondvs)
                .HasForeignKey(d => d.Idhd)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HOADONDV__IDHD__4E88ABD4");
        });

        modelBuilder.Entity<Khachhang>(entity =>
        {
            entity.HasKey(e => e.Idkh).HasName("PK__KHACHHAN__B87DC1A77DD2ADA0");

            entity.ToTable("KHACHHANG");

            entity.Property(e => e.Idkh)
                .HasMaxLength(50)
                .HasColumnName("IDKH");
            entity.Property(e => e.Dchi)
                .HasMaxLength(100)
                .HasColumnName("DCHI");
            entity.Property(e => e.Hoten)
                .HasMaxLength(50)
                .HasColumnName("HOTEN");
            entity.Property(e => e.Sodt)
                .HasMaxLength(10)
                .HasColumnName("SODT");
        });

        modelBuilder.Entity<Khuvuc>(entity =>
        {
            entity.HasKey(e => e.Idkhu).HasName("PK__KHUVUC__939914D9EFC5D8B5");

            entity.ToTable("KHUVUC");

            // === THÊM CỘT GHICHU VÀO KHUVUC ===
            entity.Property(e => e.Ghichu)
                .HasColumnName("Ghichu");

            // Code cũ giữ nguyên
            entity.Property(e => e.Idkhu)
                .HasMaxLength(50)
                .HasColumnName("IDKHU");
            entity.Property(e => e.Tenkhu)
                .HasMaxLength(50)
                .HasColumnName("TENKHU");
        });

        modelBuilder.Entity<Loaidichvu>(entity =>
        {
            entity.HasKey(e => e.Idloai).HasName("PK__LOAIDICH__0CDEF519DDD0848E");

            entity.ToTable("LOAIDICHVU");

            entity.Property(e => e.Idloai)
                .HasMaxLength(50)
                .HasColumnName("IDLOAI");
            entity.Property(e => e.Tenloai)
                .HasMaxLength(50)
                .HasColumnName("TENLOAI");
        });

        modelBuilder.Entity<Nhanvien>(entity =>
        {
            entity.HasKey(e => e.Idnv).HasName("PK__NHANVIEN__B87DC9B2BC329A99");

            entity.ToTable("NHANVIEN");

            entity.HasIndex(e => e.Cccd, "UQ__NHANVIEN__A955A0AA70B1F6F9").IsUnique();

            entity.Property(e => e.Idnv)
                .HasMaxLength(50)
                .HasColumnName("IDNV");
            entity.Property(e => e.Cccd)
                .HasMaxLength(15)
                .HasColumnName("CCCD");
            entity.Property(e => e.Gioitinh).HasColumnName("GIOITINH");
            entity.Property(e => e.Hienthi).HasColumnName("HIENTHI");
            entity.Property(e => e.Hotennv)
                .HasMaxLength(50)
                .HasColumnName("HOTENNV");
            entity.Property(e => e.Matkhau)
                .HasMaxLength(50)
                .HasColumnName("MATKHAU");
            entity.Property(e => e.Ngaysinh)
                .HasColumnType("datetime")
                .HasColumnName("NGAYSINH");
            entity.Property(e => e.Nghiviec)
                .HasDefaultValue(true)
                .HasColumnName("NGHIVIEC");
            entity.Property(e => e.Quyenadmin).HasColumnName("QUYENADMIN");
            entity.Property(e => e.Sodt)
                .HasMaxLength(20)
                .HasColumnName("SODT");
            entity.Property(e => e.Tendangnhap)
                .HasMaxLength(50)
                .HasColumnName("TENDANGNHAP");
        });

        modelBuilder.Entity<Phienchoi>(entity =>
        {
            entity.HasKey(e => e.Idphien).HasName("PK__PHIENCHO__9CAAE3C0DAABE18B");

            entity.ToTable("PHIENCHOI");

            entity.Property(e => e.Idphien)
                .HasMaxLength(50)
                .HasColumnName("IDPHIEN");
            entity.Property(e => e.Giobatdau)
                .HasColumnType("datetime")
                .HasColumnName("GIOBATDAU");
            entity.Property(e => e.Gioketthuc)
                .HasColumnType("datetime")
                .HasColumnName("GIOKETTHUC");
            entity.Property(e => e.Idban)
                .HasMaxLength(50)
                .HasColumnName("IDBAN");

            entity.HasOne(d => d.IdbanNavigation).WithMany(p => p.Phienchois)
                .HasForeignKey(d => d.Idban)
                .HasConstraintName("FK__PHIENCHOI__IDBAN__440B1D61");
        });

        // ← THÊM MỚI: Cấu hình bảng PHUONGTHUCTHANHTOAN
        modelBuilder.Entity<Phuongthucthanhtoan>(entity =>
        {
            entity.HasKey(e => e.Idpttt);
            entity.ToTable("PHUONGTHUCTHANHTOAN");

            entity.Property(e => e.Idpttt)
                .HasMaxLength(50)
                .HasColumnName("IDPTTT");

            entity.Property(e => e.Tenpttt)
                .HasMaxLength(50)
                .HasColumnName("TENPTTT");

            entity.Property(e => e.Hienthi)
                .HasColumnName("HIENTHI");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}