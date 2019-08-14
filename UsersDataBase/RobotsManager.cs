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
    public class RobotsManager : IRepository<Robot>
    {
        private UsersDbContext _usersDbContext;
        private RedisClient _robotsCache;
        private SessionsManager _sessionsManager;

        public int ID { get; private set; }

        public RobotsManager(UsersDbContext usersDbContext, RedisClient robotsCache,
            SessionsManager sessionsManager)
        {
            _usersDbContext = usersDbContext;
            _robotsCache = robotsCache;
            _sessionsManager = sessionsManager;
        }

        public async Task<Robot> Get(int id)
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

        public async Task Post(Robot entity)
        {
            _robotsCache.SetValue(entity.ID.ToString(), entity.ToJson());

            _usersDbContext.Robots.Add(entity);

            await _usersDbContext.SaveChangesAsync();
        }

        public async Task Delete(int id)
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
                    await _sessionsManager.Delete(sessionId);
                }
            }

            var robot = await _usersDbContext.Robots.FirstOrDefaultAsync(r => r.ID == id);

            if(robot != null)
            {
                _robotsCache.Remove(id.ToString());

                _usersDbContext.Robots.Remove(robot);

                await _usersDbContext.SaveChangesAsync();
            }
        }
    }
}
