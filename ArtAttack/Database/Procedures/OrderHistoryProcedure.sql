drop proc if exists [GetDummyProductsFromOrderHistory]

go


create or alter proc [GetDummyProductsFromOrderHistory] @orderHistory int 
as
begin
    select 
        o.OrderID, p.ID as productID, p.name, p.price, p.SellerID, p.productType,p.startDate, p.endDate ,o.BuyerID, o.PaymentMethod, o.OrderDate
    from [Order] o join [DummyProduct] p on o.ProductID = p.ID where o.OrderHistoryID = @orderHistory

end


