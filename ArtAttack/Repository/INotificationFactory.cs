using System.Data;
using ArtAttack.Domain;
using Microsoft.Data.SqlClient;
using ArtAttack.Shared;

namespace ArtAttack.Repository
{
    public interface INotificationFactory
    {
        static abstract Notification CreateFromDataReader(IDataReader reader);
    }
}