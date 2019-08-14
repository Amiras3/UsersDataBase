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
    public class UsersRepository : IRepository<User>
    {
        private UsersDbContext _usersDbContext;
        private RedisClient _usersCache;
        private RobotsRepository _robotsManager;

        public UsersRepository(UsersDbContext usersDbContext, RedisClient usersCache,
            RobotsRepository robotsManager)
        {
            _usersDbContext = usersDbContext;
            _usersCache = usersCache;
            _robotsManager = robotsManager;
        }

        public async Task<User> GetAsync(int id)
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

        public async Task PostAsync(User entity)
        {
            _usersDbContext.Users.Add(entity);

            await _usersDbContext.SaveChangesAsync();

            _usersCache.SetValue(entity.ID.ToString(), entity.ToJson());
        }

        public async Task UpdateAsync(User entity)
        {
            var user = await _usersDbContext.Users.FirstOrDefaultAsync(u => u.ID == entity.ID);

            user = entity;

            await _usersDbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
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
                    await _robotsManager.DeleteAsync(robotId);
                }
            }

            var user = await _usersDbContext.Users.FirstOrDefaultAsync(u => u.ID == id);

            if (user != null)
            {
                _usersDbContext.Users.Remove(user);

                await _usersDbContext.SaveChangesAsync();

                _usersCache.Remove(id.ToString());
            }
        }
    }
}
