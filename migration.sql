IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250323015144_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [Products] (
        [Id] int NOT NULL,
        [Ean] nvarchar(50) NULL,
        [CategoryId] int NOT NULL,
        [CategoryName] nvarchar(100) NULL,
        [BrandId] int NOT NULL,
        [BrandName] nvarchar(100) NULL,
        [Name] nvarchar(250) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Price] decimal(18,2) NOT NULL,
        [InStock] bit NOT NULL,
        [ExpectedRestock] datetime2 NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250323015144_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_Products_BrandId] ON [Products] ([BrandId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250323015144_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250323015144_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_Products_Ean] ON [Products] ([Ean]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250323015144_InitialSqlServerSchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250323015144_InitialSqlServerSchema', N'8.0.11');
END;
GO

COMMIT;
GO

