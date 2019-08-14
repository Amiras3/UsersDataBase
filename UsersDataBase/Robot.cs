using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsersDataBase
{
    public class Robot
    {
        public Robot()
        {
            Sessions = new HashSet<Session>();
        }

        public int ID { get; set; }

        public string RobotName { get; set; }

        public ICollection<Session> Sessions { get; set; }

        public User User { get; set; }
    }
}
