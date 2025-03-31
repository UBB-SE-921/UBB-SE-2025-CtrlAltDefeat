

create or alter proc [AddCard] 
@cname varchar(50),
@cnumber varchar(20),
@cvc varchar(3),
@mon varchar(2), 
@yr varchar(2),
@country varchar(30),
@balance float as
begin 
insert into [DummyCard] 
values (@cname,@cnumber,@cvc,@mon,@yr,@country,@balance)
end
go


create or alter proc [DeleteCard] @cardnumber varchar(20) as
begin

 delete from [DummyCard] where [cardNumber]=@cardnumber

end
go

create or alter proc [UpdateCardBalance]
@cnumber varchar(20),
@balance float
as
begin
update [DummyCard] 
set [balance]=@balance
where [cardNumber]=@cnumber
end
go

create or alter proc [GetBalance]
@cnumber varchar(20)
as
begin
select [balance] from [DummyCard] where [cardNumber]=@cnumber
end
go
