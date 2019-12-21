Расширение библиотеки DBEngine предоставляющее возможность делать автоматические резервные копии или ручные резервные копии

Подготовка базы данных (для примера используеться Microsoft Sql Server)

Таблицы
-----------------------------------------
dbo.Backups
Id		int				Unchecked
Version	nvarchar(25)	Unchecked
Time	smalldatetime	Unchecked

Хранимые процедуры

-----------------------------------------
PROCEDURE [dbo].[create_backup]
@version nvarchar(42),
@path nvarchar(255)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @date smalldatetime = GETDATE();
	
	DECLARE @fileName nvarchar(255) = CONCAT(@version, '.', FORMAT(@date, 'yyyyMMddhhmmss'), '.bak')

	DECLARE @fullPath nvarchar(255) = CONCAT(@path, @fileName);

	DECLARE @SQL nvarchar(MAX) =  ('
	BACKUP DATABASE ' + DB_NAME() + ' TO  DISK =  ''' + @fullPath + '''
	WITH	RETAINDAYS = 14, 
			NOFORMAT, 
			NOINIT,  
			NAME = N''' + DB_NAME() +'-Полная База данных Резервное копирование'' , 
			SKIP, 
			NOREWIND, 
			NOUNLOAD,  
			STATS = 10;
			')

	exec sys.sp_executesql @SQL
	INSERT INTO dbo.Backups VALUES(@version,@date);

END

-----------------------------------------
PROCEDURE [dbo].[get_exists_version_backup]
@version nvarchar(42)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT Count(*) FROM dbo.Backups WHERE Version = @version;
END

-----------------------------------------
PROCEDURE [dbo].[get_latest_backup_time]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT MAX(Time) FROM dbo.Backups;
END
