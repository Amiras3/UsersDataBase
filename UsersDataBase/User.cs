using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsersDataBase
{
    public class User
    {
        public User()
        {
            Robots = new HashSet<Robot>();
        }

        public int ID { get; set; }

        public int UserName { get; set; }

        public int Email { get; set; }

        public ICollection<Robot> Robots { get; set; }
    }
}
