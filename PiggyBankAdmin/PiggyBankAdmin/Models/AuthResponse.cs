using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiggyBankAdmin.Models
{
    public class AuthResponse
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; }
    }
}
