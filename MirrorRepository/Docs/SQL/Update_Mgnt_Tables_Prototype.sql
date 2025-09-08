use SnowDbUnitTestMgnt;

alter table Synchronization add DatabaseSettingsId uniqueidentifier;
alter table Synchronization add InstanzSettingsId uniqueidentifier;

alter table Synchronization add SyncTypeId uniqueidentifier;

alter table Synchronization alter column SnowTables varchar(max);
alter table Synchronization alter column SnowColumns varchar(max);

alter table SyncProcess add SynchronizationId uniqueidentifier;
alter table SyncProcess add SyncTime datetime2;
alter table SyncProcess add EndTime datetime2;
alter table SyncProcess add RecordsInserted int not null default 0;
alter table SyncProcess add RecordsUpdated int not null default 0;
alter table SyncProcess add Pages int not null default 0;
alter table SyncProcess add [Page] int not null default 0;
alter table SyncProcess add Failures int not null default 0;
ALTER TABLE SyncProcess DROP CONSTRAINT [FK_SyncTypeSyncProcess]
alter table SyncProcess drop column SyncType;
alter table SyncProcess alter column RecordsSynchronized int;
alter table SyncProcess alter column RecordsFound int;



alter table InstanzSettings add Servername varchar(256);
alter table InstanzSettings add Port int not null default 0;

--alter table SyncProcess drop column RecordsInserted;
--alter table SyncProcess drop column RecordsUpdated;
--alter table SyncProcess drop column Pages;
--alter table SyncProcess drop column [Page];
--alter table SyncProcess drop column Failures;
--alter table InstanzSettings drop column Port;
--INSERT INTO [dbo].[SyncType] ([Id] ,[TypeName],[Created]) VALUES (newid() ,'Full' , getdate());
--INSERT INTO [dbo].[SyncType] ([Id] ,[TypeName],[Created]) VALUES (newid() ,'Consistency' , getdate());
--INSERT INTO [dbo].[SyncType] ([Id] ,[TypeName],[Created]) VALUES (newid() ,'Delta' , getdate());
--alter table Synchronization add SnowTables_ varbinary(max);
--alter table Synchronization add SnowColumns_ varbinary(max);
--update Synchronization set SnowTables_ = Convert(varbinary, SnowTables), SnowColumns_ = Convert(varbinary, SnowColumns);
--alter table Synchronization add SnowTables_ varbinary(max);
--alter table Synchronization drop column SnowTables;
--alter table Synchronization drop column SnowColumns;
--sp_rename 'Synchronization.SnowTables_', 'SnowTables', 'COLUMN';
--sp_rename 'Synchronization.SnowColumns_', 'SnowColumns', 'COLUMN';

-- validate after update of schema:
select * from Synchronization s left join DatabaseSettings db on s.DatabaseSettingsId = db.id left join InstanzSettings i on s.InstanzSettingsId = i.id;
