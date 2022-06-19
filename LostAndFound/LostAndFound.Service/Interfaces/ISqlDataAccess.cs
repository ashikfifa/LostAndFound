using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LostAndFound.Service.Interfaces
{
    public interface ISqlDataAccess
    {
        Task<IEnumerable<T>> LoadData<T, U>(string storedProcedure, U paramiters);
        Task SaveData<T>(string storedProcedure, T paramiters);
        Task<T> SaveDataAndReturnId<T, U>(string storedProcedure, U paramiters);
    }
}
