go
create procedure uspDeleteOrderCheckpoint (@checkpointID bigint) as
begin
	begin try
		delete from OrderCheckpoints
		where [CheckpointID] = @checkpointID
	end try
	begin catch
		declare @err_message varchar(2000)
		declare @err_number int
		select @err_message = error_message(), @err_number = error_number()
		raiserror(N'uspDeleteOrderCheckpoint: Error %i: %s', 16, 1, @err_number, @err_message)
	end catch
end
go
