-- Create schema for LongChau store
-- Adjust database name if needed
IF DB_ID(N'LongChauDb') IS NULL
BEGIN
    CREATE DATABASE [LongChauDb];
END;
GO

USE [LongChauDb];
GO

-- Products
CREATE TABLE dbo.Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Slug NVARCHAR(200) NULL,
    Brand NVARCHAR(200) NULL,
    ShortDescription NVARCHAR(1000) NULL,
    Description NVARCHAR(MAX) NULL,
    Price DECIMAL(18,2) NULL,
    OriginalPrice DECIMAL(18,2) NULL,
    Rating DECIMAL(3,2) NULL,
    ReviewsCount INT NULL,
    Ingredients NVARCHAR(MAX) NULL,
    UsageInfo NVARCHAR(MAX) NULL,
    Origin NVARCHAR(200) NULL,
    Packaging NVARCHAR(200) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- Product images
CREATE TABLE dbo.ProductImages (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    Url NVARCHAR(500) NOT NULL,
    AltText NVARCHAR(250) NULL,
    IsPrimary BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_ProductImages_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE
);
GO

-- Categories
CREATE TABLE dbo.Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Slug NVARCHAR(200) NULL,
    Description NVARCHAR(1000) NULL
);
GO

-- Many-to-many product-category
CREATE TABLE dbo.ProductCategories (
    ProductId INT NOT NULL,
    CategoryId INT NOT NULL,
    CONSTRAINT PK_ProductCategories PRIMARY KEY (ProductId, CategoryId),
    CONSTRAINT FK_ProductCategories_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ProductCategories_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id) ON DELETE CASCADE
);
GO

-- Contacts / footer info
CREATE TABLE dbo.Contacts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Organization NVARCHAR(250) NULL,
    Phone NVARCHAR(50) NULL,
    Email1 NVARCHAR(250) NULL,
    Email2 NVARCHAR(250) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- Customers (minimal)
CREATE TABLE dbo.Customers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(150) NULL,
    LastName NVARCHAR(150) NULL,
    Email NVARCHAR(250) NULL,
    Phone NVARCHAR(50) NULL,
    AddressLine NVARCHAR(500) NULL,
    City NVARCHAR(200) NULL,
    District NVARCHAR(200) NULL,
    PostalCode NVARCHAR(20) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- Orders and status
CREATE TABLE dbo.OrderStatus (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL
);
GO

CREATE TABLE dbo.Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NULL,
    OrderDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    StatusId INT NOT NULL DEFAULT 1,
    SubTotal DECIMAL(18,2) NOT NULL DEFAULT 0,
    Discount DECIMAL(18,2) NOT NULL DEFAULT 0,
    ShippingFee DECIMAL(18,2) NOT NULL DEFAULT 0,
    Total DECIMAL(18,2) NOT NULL DEFAULT 0,
    PaymentMethod NVARCHAR(100) NULL,
    Notes NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Orders_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id),
    CONSTRAINT FK_Orders_Status FOREIGN KEY (StatusId) REFERENCES dbo.OrderStatus(Id)
);
GO

CREATE TABLE dbo.OrderItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NULL,
    ProductName NVARCHAR(200) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    LineTotal AS (UnitPrice * Quantity) PERSISTED,
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
);
GO

-- Payments log
CREATE TABLE dbo.Payments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    PaidDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Amount DECIMAL(18,2) NOT NULL,
    Method NVARCHAR(100) NULL,
    TransactionId NVARCHAR(250) NULL,
    Status NVARCHAR(100) NULL,
    CONSTRAINT FK_Payments_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id) ON DELETE CASCADE
);
GO

-- Product reviews
CREATE TABLE dbo.Reviews (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    CustomerId INT NULL,
    Rating INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    Title NVARCHAR(250) NULL,
    Body NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Reviews_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Reviews_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id)
);
GO

-- Useful indexes
CREATE INDEX IX_Products_Slug ON dbo.Products(Slug);
CREATE INDEX IX_Products_Name ON dbo.Products(Name);
CREATE INDEX IX_Categories_Slug ON dbo.Categories(Slug);
GO

---seed data

USE [LongChauDb];
GO

-- Seed basic order statuses
INSERT INTO dbo.OrderStatus (Name) VALUES (N'Pending'), (N'Processing'), (N'Completed'), (N'Cancelled');
GO

-- Seed some categories
INSERT INTO dbo.Categories (Name, Slug) VALUES
(N'Vitamin & Khoáng chất', N'vitamin-khoang-chat'),
(N'Sức khỏe tim mạch', N'suc-khoe-tim-mach'),
(N'Hỗ trợ làm đẹp', N'ho-tro-lam-dep'),
(N'Thần kinh não', N'than-kinh-nao'),
(N'Thuốc', N'thuoc'),
(N'Sản phẩm nổi bật', N'san-pham-noi-bat');
GO

-- Seed products extracted from views
INSERT INTO dbo.Products (Name, Slug, Brand, ShortDescription, Description, Price, OriginalPrice, Rating, ReviewsCount, Ingredients, UsageInfo, Origin, Packaging)
VALUES
(N'Viên uống Omega-3 Orihiro', N'omega-3-orihiro', N'Orihiro', N'Omega-3 hỗ trợ tim mạch', N'Omega-3 Orihiro giúp hỗ trợ tim mạch, cải thiện trí nhớ, giảm mỡ máu và tăng cường sức khỏe não bộ.', 265000, 310000, 4.8, 102, N'Dầu cá tinh khiết, EPA, DHA, Vitamin E', N'Uống 2 viên mỗi ngày sau bữa ăn.', N'Nhật Bản', N'Túi 180 viên dùng trong 90 ngày'),
(N'Viên uống Vitamin C 500mg', N'vitamin-c-500mg', N'HealthPlus', N'Bổ sung vitamin C', N'Viên uống Vitamin C giúp tăng cường sức đề kháng, làm sáng da và chống oxy hóa.', 120000, 150000, 4.8, 42, N'Vitamin C 500mg, Bioflavonoids, Rosehip Extract', N'Uống 1 viên mỗi ngày sau bữa ăn.', N'Mỹ', N'Hộp 60 viên'),
(N'Viên uống Biotin 5mg Mediplantex', N'biotin-5mg-mediplantex', N'Mediplantex', N'Bổ sung Biotin', N'Biotin 5mg Mediplantex giúp bổ sung vitamin B7 cần thiết cho cơ thể, hỗ trợ điều trị rụng tóc, viêm da.', 28000, 35000, 4.8, 112, N'Biotin 5mg, Lactose, Cellulose', N'Uống 1 viên mỗi ngày', N'Việt Nam', N'Hộp 2 vỉ x 10 viên'),
(N'Thuốc ho Prospan', N'thuoc-ho-prospan', N'Prospan', N'Thuốc ho', N'Thuốc ho Prospan', 250000, NULL, 4.5, 50, NULL, NULL, N'Germany', N'Chai 100ml');
GO

-- Seed product images
INSERT INTO dbo.ProductImages (ProductId, Url, AltText, IsPrimary)
VALUES
(1, N'/hinh/Omega3.png', N'Omega 3 image', 1),
(2, N'/hinh/VitaminC.jpg', N'Vitamin C image', 1),
(3, N'/hinh/Biotin.jpg', N'Biotin image', 1),
(4, N'/hinh/Prospan.jpg', N'Prospan image', 1);
GO

-- Map products to categories
-- 1: Omega3 -> Sức khỏe tim mạch (Id may vary). For reproducibility we look up ids:
DECLARE @vit INT = (SELECT Id FROM dbo.Categories WHERE Slug='vitamin-khoang-chat');
DECLARE @tim INT = (SELECT Id FROM dbo.Categories WHERE Slug='suc-khoe-tim-mach');
DECLARE @dep INT = (SELECT Id FROM dbo.Categories WHERE Slug='ho-tro-lam-dep');
DECLARE @noibat INT = (SELECT Id FROM dbo.Categories WHERE Slug='san-pham-noi-bat');

INSERT INTO dbo.ProductCategories (ProductId, CategoryId)
VALUES
(1, @tim),
(1, @noibat),
(2, @vit),
(2, @noibat),
(3, @dep),
(3, @noibat),
(4, @noibat);
GO

-- Seed footer contact
INSERT INTO dbo.Contacts (Organization, Phone, Email1, Email2)
VALUES (N'Nhà thuốc Long Châu', N'0123456789', N'phamminhtriet18072006@gmail.com', N'binbin3653@gmail.com');
GO

-- Optional sample customer + order
INSERT INTO dbo.Customers (FirstName, LastName, Email, Phone, AddressLine, City, District)
VALUES (N'Nguyễn', N'Văn A', N'customer1@example.com', N'0901234567', N'123 Đường A', N'Ho Chi Minh', N'Quận 1');
DECLARE @custId INT = SCOPE_IDENTITY();

INSERT INTO dbo.Orders (CustomerId, StatusId, SubTotal, Discount, ShippingFee, Total, PaymentMethod, Notes)
VALUES (@custId, 1, 120000, 0, 20000, 140000, N'COD', N'Giao trong ngày');
DECLARE @ordId INT = SCOPE_IDENTITY();

INSERT INTO dbo.OrderItems (OrderId, ProductId, ProductName, UnitPrice, Quantity)
VALUES (@ordId, 2, N'Viên uống Vitamin C 500mg', 120000, 1);
GO

--drop
USE [LongChauDb];
GO

-- Drop objects in safe order
IF OBJECT_ID('dbo.OrderItems') IS NOT NULL DROP TABLE dbo.OrderItems;
IF OBJECT_ID('dbo.Payments') IS NOT NULL DROP TABLE dbo.Payments;
IF OBJECT_ID('dbo.Orders') IS NOT NULL DROP TABLE dbo.Orders;
IF OBJECT_ID('dbo.OrderStatus') IS NOT NULL DROP TABLE dbo.OrderStatus;
IF OBJECT_ID('dbo.Reviews') IS NOT NULL DROP TABLE dbo.Reviews;
IF OBJECT_ID('dbo.ProductCategories') IS NOT NULL DROP TABLE dbo.ProductCategories;
IF OBJECT_ID('dbo.Categories') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.ProductImages') IS NOT NULL DROP TABLE dbo.ProductImages;
IF OBJECT_ID('dbo.Products') IS NOT NULL DROP TABLE dbo.Products;
IF OBJECT_ID('dbo.Contacts') IS NOT NULL DROP TABLE dbo.Contacts;
IF OBJECT_ID('dbo.Customers') IS NOT NULL DROP TABLE dbo.Customers;
GO
-- Optionally drop database:
-- DROP DATABASE [LongChauDb];