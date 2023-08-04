using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* Bei änderung der Datenbankstruktur folgende Befehle ausführen:
 * Add-Migration InitialCreate_<int>
 * Update-Database
 */

namespace SimpleLogin
{
    public class User
    {
        public Guid Id { get; set; }

        public string? Username { get; set; }

        public string? Usersecret { get; set; }

        public string? RecoveryKey { get; set; }
    }
}
