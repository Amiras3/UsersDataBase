using ServiceStack;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsersDataBase
{
    public class SessionsRepository : IRepository<Session>
    {
        private UsersDbContext _usersDbContext;
        private RedisClient _sessionsCache;

        public SessionsRepository(UsersDbContext usersDbContext, RedisClient sessionsCache)
        {
            _usersDbContext = usersDbContext;
            _sessionsCache = sessionsCache;
        }

        public async Task<Session> GetAsync(int id)
        {
            var cachedSession = _sessionsCache.GetValue(id.ToString())
                .FromJson<Session>();

            if (cachedSession != null)
            {
                return cachedSession;
            }

            var session = await _usersDbContext.Sessions
                .FirstOrDefaultAsync(s => s.ID == id);

            _sessionsCache.SetValue(id.ToString(), session.ToJson());

            return session;
        }

        public async Task PostAsync(Session entity)
        {
            _usersDbContext.Sessions.Add(entity);

            await _usersDbContext.SaveChangesAsync();

            _sessionsCache.SetValue(entity.ID.ToString(), entity.ToJson());
        }

        public async Task UpdateAsync(Session entity)
        {
            var session = await _usersDbContext.Sessions.FirstOrDefaultAsync(s => s.ID == entity.ID);

            session = entity;

            await _usersDbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var session = await _usersDbContext.Sessions.FirstOrDefaultAsync(s => s.ID == id);

            if (session != null)
            {
                _usersDbContext.Sessions.Remove(session);

                await _usersDbContext.SaveChangesAsync();

                _sessionsCache.Remove(id.ToString());
            }
        }
    }
}
