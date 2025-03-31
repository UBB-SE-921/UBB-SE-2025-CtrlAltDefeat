

create or alter proc [AddWallet] 
@balance float as

begin 
insert into [DummyWallet] 
values (@balance)
end
go




create or alter proc [DeleteWallet] @id int as
begin
 delete from [DummyWallet] where [ID]=@id
end
go

create or alter proc [UpdateWalletBalance]
@id int,
@balance float
as
begin
update [DummyWallet] 
set [balance]=@balance
where [ID]=@id
end
go


create or alter proc [GetWalletBalance]
@id int
as
begin
select balance from [DummyWallet] where [ID]=@ID
end
go