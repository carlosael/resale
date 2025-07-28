-- Script para marcar a migration inicial como aplicada
-- Execute este script no banco de dados antes de aplicar a migration de rename

-- Verificar se a tabela de histórico de migrations existe
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[__EFMigrationsHistory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END

-- Inserir o registro da migration inicial se ela não estiver na tabela
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250727205206_InitialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20250727205206_InitialCreate', '8.0.0');
    
    PRINT 'Migration inicial marcada como aplicada.';
END
ELSE
BEGIN
    PRINT 'Migration inicial já está marcada como aplicada.';
END

-- Verificar o status atual das migrations
SELECT * FROM [__EFMigrationsHistory] ORDER BY [MigrationId]; 