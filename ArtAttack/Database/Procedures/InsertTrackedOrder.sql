
go
create procedure uspInsertTrackedOrder (@estimatedDeliveryDate date, @deliveryAddress varchar(255), @orderStatus varchar(100), @orderID bigint) as
begin
	begin try
		insert into TrackedOrders values (@estimatedDeliveryDate, @deliveryAddress, @orderStatus, @orderID)
	end try
	begin catch
		declare @err_message varchar(2000)
		declare @err_number int
		select @err_message = error_message(), @err_number = error_number()
		raiserror(N'uspInsertTrackedOrder: Error %i: %s', 16, 1, @err_number, @err_message)
	end catch
end
go

go
alter procedure uspInsertTrackedOrder 
    @estimatedDeliveryDate date, 
    @deliveryAddress varchar(255), 
    @orderStatus varchar(100), 
    @orderID bigint,
    @newTrackedOrderID int output
as
begin
    begin try
        insert into TrackedOrders (EstimatedDeliveryDate, DeliveryAddress, OrderStatus, OrderID) 
        output inserted.TrackedOrderID 
        values (@estimatedDeliveryDate, @deliveryAddress, @orderStatus, @orderID)

        set @newTrackedOrderID = scope_identity()
    end try
    begin catch
        declare @err_message varchar(2000)
        declare @err_number int
        select @err_message = error_message(), @err_number = error_number()
        raiserror(N'uspInsertTrackedOrder: Error %i: %s', 16, 1, @err_number, @err_message)
        set @newTrackedOrderID = -1
    end catch
end
go

