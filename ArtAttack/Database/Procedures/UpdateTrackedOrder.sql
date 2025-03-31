go
create procedure uspUpdateTrackedOrder (@trackedOrderID bigint, @estimatedDeliveryDate date, @orderStatus varchar(100)) as
begin
	begin try
		update TrackedOrders
		set [EstimatedDeliveryDate] = @estimatedDeliveryDate
		where [TrackedOrderID] = @trackedOrderID
	end try
	begin catch
		declare @err_message varchar(2000)
		declare @err_number int
		select @err_message = error_message(), @err_number = error_number()
		raiserror(N'uspUpdateTrackedOrder: Error %i: %s', 16, 1, @err_number, @err_message)
	end catch
end
go

go
alter procedure uspUpdateTrackedOrder (@trackedOrderID bigint, @estimatedDeliveryDate date, @orderStatus varchar(100)) as
begin
	begin try
		update TrackedOrders
		set [EstimatedDeliveryDate] = @estimatedDeliveryDate, [OrderStatus] = @orderStatus
		where [TrackedOrderID] = @trackedOrderID
	end try
	begin catch
		declare @err_message varchar(2000)
		declare @err_number int
		select @err_message = error_message(), @err_number = error_number()
		raiserror(N'uspUpdateTrackedOrder: Error %i: %s', 16, 1, @err_number, @err_message)
	end catch
end
go
