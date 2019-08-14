using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsersDataBase
{
    public interface IRepository<T>
    {
        Task<T> Get(int id);

        Task Post(T entity);

        Task Delete(int id);
    }
}
