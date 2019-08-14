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
    public class RobotsRepository : IRepository<Robot>
    {
        private UsersDbContext _usersDbContext;
        private RedisClient _robotsCache;
        private SessionsRepository _sessionsManager;

        public int ID { get; private set; }

        public RobotsRepository(UsersDbContext usersDbContext, RedisClient robotsCache,
            SessionsRepository sessionsManager)
        {
            _usersDbContext = usersDbContext;
            _robotsCache = robotsCache;
            _sessionsManager = sessionsManager;
        }

        public async Task<Robot> GetAsync(int id)
        {
            var cachedRobot = _robotsCache.GetValue(id.ToString())
                .FromJson<Robot>();

            if (cachedRobot != null)
            {
                return cachedRobot;
            }

            var robot = await _usersDbContext.Robots
                .FirstOrDefaultAsync(s => s.ID == id);

            _robotsCache.SetValue(id.ToString(), robot.ToJson());

            return robot;
        }

        public async Task PostAsync(Robot entity)
        {
            _usersDbContext.Robots.Add(entity);

            await _usersDbContext.SaveChangesAsync();

            _robotsCache.SetValue(entity.ID.ToString(), entity.ToJson());
        }

        public async Task UpdateAsync(Robot entity)
        {
            var robot = await _usersDbContext.Robots.FirstOrDefaultAsync(r => r.ID == entity.ID);

            robot = entity;

            await _usersDbContext.SaveChangesAsync();

            _robotsCache.SetValue(entity.ID.ToString(), entity.ToJson());
        }

        public async Task DeleteAsync(int id)
        {
            var sessionsIds = await _usersDbContext.Sessions.Join(
                _usersDbContext.Robots.Where(r => r.ID == id),
                s => s.Robot.ID,
                r => r.ID,
                (s, r) => new { Session = s, Robot = r }
            )
             .Where(p => p.Session.Robot.ID == p.Robot.ID)
             .Select(p => p.Session.ID)
             .ToListAsync();

            if(sessionsIds != null && sessionsIds.Any())
            {
                foreach(var sessionId in sessionsIds)
                {
                    await _sessionsManager.DeleteAsync(sessionId);
                }
            }

            var robot = await _usersDbContext.Robots.FirstOrDefaultAsync(r => r.ID == id);

            if(robot != null)
            {
                _usersDbContext.Robots.Remove(robot);

                await _usersDbContext.SaveChangesAsync();

                _robotsCache.Remove(id.ToString());
            }
        }
    }
}
