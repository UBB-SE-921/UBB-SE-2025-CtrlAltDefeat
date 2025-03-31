
go
create procedure uspInsertOrderCheckpoint (@timestamp datetime, @location varchar(255), @description varchar(255), @checkpointStatus varchar(100), @trackedOrderID int) as
begin
	begin try
		insert into OrderCheckpoints values(@timestamp, @location, @description, @checkpointStatus, @trackedOrderID)
		update TrackedOrders
		set OrderStatus = @checkpointStatus
		where TrackedOrderID = @trackedOrderID
	end try
	begin catch
		declare @err_message varchar(2000)
		declare @err_number int
		select @err_message = error_message(), @err_number = error_number()
		raiserror(N'uspInsertOrderCheckpoint: Error %i: %s', 16, 1, @err_number, @err_message)
	end catch
end
go

go
alter procedure uspInsertOrderCheckpoint 
    @timestamp datetime, 
    @location varchar(255), 
    @description varchar(255), 
    @checkpointStatus varchar(100), 
    @trackedOrderID int,
    @newCheckpointID int output
as
begin
    begin try
        insert into OrderCheckpoints (Timestamp, Location, Description, CheckpointStatus, TrackedOrderID) 
        output inserted.CheckpointID 
        values (@timestamp, @location, @description, @checkpointStatus, @trackedOrderID)

        set @newCheckpointID = scope_identity()
    end try
    begin catch
        declare @err_message varchar(2000)
        declare @err_number int
        select @err_message = error_message(), @err_number = error_number()
        raiserror(N'uspInsertOrderCheckpoint: Error %i: %s', 16, 1, @err_number, @err_message)
        set @newCheckpointID = -1
    end catch
end
go

