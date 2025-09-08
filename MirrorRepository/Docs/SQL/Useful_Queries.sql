select * from DatabaseSettings;
select * from InstanzSettings;
select * from SyncType st inner join Synchronization s on s.SyncTypeId = st.Id;
--update SyncType set TypeName = 'Consistency' where id in (select SyncTypeId from Synchronization);

select * from Synchronization;
select [Key], StartTime, SyncTime, EndTime, RecordsFound,RecordsSynchronized, RecordsInserted, RecordsUpdated,Pages, Page, Failures from SyncProcess
order by SyncTime desc;


