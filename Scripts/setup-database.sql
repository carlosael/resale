-- Script para configuração inicial do banco de dados Resale API
-- Execute este script no SQL Server Management Studio ou Azure Data Studio

-- Criar banco de dados (se não existir)
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'ResaleDb')
BEGIN
    CREATE DATABASE [ResaleDb]
END
GO

-- Usar o banco de dados
USE [ResaleDb]
GO

-- Verificar se as tabelas já existem
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Resellers' AND xtype='U')
BEGIN
    PRINT 'Banco de dados criado com sucesso!'
    PRINT 'Execute o comando "dotnet ef database update" para criar as tabelas.'
END
ELSE
BEGIN
    PRINT 'Banco de dados já existe e contém as tabelas da aplicação.'
END
GO

-- Criar usuário para a aplicação (opcional - para ambientes de produção)
-- Descomente e ajuste as credenciais conforme necessário

/*
-- Criar login
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'resale_api_user')
BEGIN
    CREATE LOGIN [resale_api_user] WITH PASSWORD = N'StrongPassword123!', 
    DEFAULT_DATABASE = [ResaleDb], 
    CHECK_EXPIRATION = OFF, 
    CHECK_POLICY = ON
END
GO

-- Usar o banco da aplicação
USE [ResaleDb]
GO

-- Criar usuário no banco
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'resale_api_user')
BEGIN
    CREATE USER [resale_api_user] FOR LOGIN [resale_api_user]
    ALTER ROLE [db_datareader] ADD MEMBER [resale_api_user]
    ALTER ROLE [db_datawriter] ADD MEMBER [resale_api_user]
    ALTER ROLE [db_ddladmin] ADD MEMBER [resale_api_user]
END
GO
*/

PRINT 'Script de setup executado com sucesso!'
PRINT 'Próximos passos:'
PRINT '1. Configure a connection string no appsettings.json'
PRINT '2. Execute: dotnet ef migrations add InitialCreate'
PRINT '3. Execute: dotnet ef database update'
PRINT '4. Execute: dotnet run' 