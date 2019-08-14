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
    public class UsersManager : IRepository<User>
    {
        private UsersDbContext _usersDbContext;
        private RedisClient _usersCache;
        private RobotsManager _robotsManager;

        public UsersManager(UsersDbContext usersDbContext, RedisClient usersCache,
            RobotsManager robotsManager)
        {
            _usersDbContext = usersDbContext;
            _usersCache = usersCache;
            _robotsManager = robotsManager;
        }

        public async Task<User> Get(int id)
        {
            var cachedUser = _usersCache.GetValue(id.ToString())
                .FromJson<User>();

            if (cachedUser != null)
            {
                return cachedUser;
            }

            var user = await _usersDbContext.Users
                .FirstOrDefaultAsync(u => u.ID == id);

            _usersCache.SetValue(id.ToString(), user.ToJson());

            return user;
        }

        public async Task Post(User entity)
        {
            _usersCache.SetValue(entity.ID.ToString(), entity.ToJson());

            _usersDbContext.Users.Add(entity);

            await _usersDbContext.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var robotsIds = await _usersDbContext.Robots.Join(
                _usersDbContext.Users.Where(u => u.ID == id),
                r => r.User == null ? 0 : r.User.ID,
                u => u.ID,
                (r, u) => new { User = u, Robot = r }
            ).Select(p => p.Robot.ID)
             .ToListAsync();

            if (robotsIds != null && robotsIds.Any())
            {
                foreach (var robotId in robotsIds)
                {
                    await _robotsManager.Delete(robotId);
                }
            }

            var user = await _usersDbContext.Users.FirstOrDefaultAsync(u => u.ID == id);

            if (user != null)
            {
                _usersCache.Remove(id.ToString());

                _usersDbContext.Users.Remove(user);

                await _usersDbContext.SaveChangesAsync();
            }
        }
    }
}
