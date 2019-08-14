using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsersDataBase
{
    public class Session
    {
        public int ID { get; set; }

        public string SessionName { get; set; }

        [Required]
        public Robot Robot { get; set; }
    }
}
