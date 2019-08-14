using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsersDataBase
{
    public interface IRepository<T>
    {
        Task<T> GetAsync(int id);

        Task PostAsync(T entity);

        Task DeleteAsync(int id);

        Task UpdateAsync(T entity);
    }
}
