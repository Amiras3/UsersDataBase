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
    public class SessionsManager : IRepository<Session>
    {
        private UsersDbContext _usersDbContext;
        private RedisClient _sessionsCache;

        public SessionsManager(UsersDbContext usersDbContext, RedisClient sessionsCache)
        {
            _usersDbContext = usersDbContext;
            _sessionsCache = sessionsCache;
        }

        public async Task<Session> Get(int id)
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

        public async Task Post(Session entity)
        {
            _sessionsCache.SetValue(entity.ID.ToString(), entity.ToJson());

            _usersDbContext.Sessions.Add(entity);

            await _usersDbContext.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var session = await _usersDbContext.Sessions.FirstOrDefaultAsync(s => s.ID == id);

            if (session != null)
            {
                _sessionsCache.Remove(id.ToString());

                _usersDbContext.Sessions.Remove(session);

                await _usersDbContext.SaveChangesAsync();
            }
        }
    }
}
