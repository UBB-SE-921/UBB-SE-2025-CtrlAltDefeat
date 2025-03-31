
go
create procedure uspUpdateOrderCheckpoint (@checkpointID bigint, @timestamp datetime, @location varchar(255), @description varchar(255), @checkpointStatus varchar(100)) as
begin
	begin try
		update OrderCheckpoints
		set [Timestamp] = @timestamp, [Location] = @location, [Description] = @description, [CheckpointStatus] = @checkpointStatus
		where [CheckpointID] = @checkpointID
	end try
	begin catch
		declare @err_message varchar(2000)
		declare @err_number int
		select @err_message = error_message(), @err_number = error_number()
		raiserror(N'uspUpdateOrderCheckpoint: Error %i: %s', 16, 1, @err_number, @err_message)
	end catch
end
go