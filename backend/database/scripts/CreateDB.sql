IF EXISTS(select 1 from sys.databases where [name]='BIEDB')
	DROP DATABASE [BIEDB]
GO
USE [master]
CREATE DATABASE [BIEDB]
GO

USE [BIEDB]
GO
