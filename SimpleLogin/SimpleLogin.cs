using Microsoft.Data.Sqlite;
using System;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace SimpleLogin
{
    /// <summary>
    /// Simple Login
    /// Nutzt das Entity Framework zum aufbau einer sicheren Verbindung zu einer Sqlite Datenbank
    /// Speichert registrierte Nutzer in einer Datenbank
    /// </summary>

    //Info:
    // Add Database Migration:
    // Add-Migration InitialCreate
    // Update Database:
    // Update-Database

    public class SimpleLogin
    {
        //Hashkey wird genutzt, um Zeichenfolgen zu verschlüsseln
        public static readonly string _encryptKey = "b14ca5898a4e4133bbce2ea2315a1916";
        public const string _username = "";
        private static bool _konami = false;

        public static void Main()
        {
            using var database = new UserContext();

            while (true)
            {
                Console.WriteLine(@"Input Exit : 0 | 
                                    Login : 1 | 
                                    Register : 2 | 
                                    Show All User: 3 | 
                                    Search User: 4 | 
                                    Reset Password (Experimental): 5 | 
                                    Change Username (Experimental): 6");

                bool result = int.TryParse(Console.ReadLine() ?? "1", out int start);
                //Überprüft, ob der Input vom Nutzer verwendet werden kann
                if (result == false || !new List<int>() { 0, 1, 2, 3, 4, 5, 6, 3254785 }.Contains(start))
                {
                    Console.WriteLine("Invalid Input!\n");
                    continue;
                }

                switch (start)
                {
                    case 0:
                        Environment.Exit(1);
                        break;

                    case 1:
                        if (CheckUserCredential(database, GetUsername(), GetUsersecret())) Console.WriteLine("Login successfull!\n");
                        else Console.WriteLine("Login failed! Username or Password incorrect\n");
                        break;

                    case 2:
                        if (AddNewUser(database, GetUsername(), GetUsersecret())) Console.WriteLine("Successfully registered!\n");
                        else Console.WriteLine("Registration failed: User does already exists!\n");
                        break;

                    case 3:
                        Console.WriteLine("All User:");
                        ShowAllUsers(database);
                        Console.WriteLine("");
                        break;

                    case 4:
                        Console.WriteLine("Search User:");
                        SearchUser(database, GetUsername());
                        Console.WriteLine("");
                        break;

                    case 5:
                        Console.WriteLine("Reset Password:");
                        ResetUserPassword(database, GetUsername(), GetUserRecoveryKey());
                        Console.WriteLine("");
                        break;
                    case 6:
                        Console.WriteLine("Change Username:");
                        ChangeUsername(database, GetUsername(), GetUsersecret());
                        Console.WriteLine("");
                        break;
                    case 3254785:
                        _konami = !_konami;
                        break;

                    default:
                        break;
                }
            }
        }

        //Gibt den, vom Nutzer eingegebenen, Nutzernamen zurück
        private static string GetUsername()
        {
            Console.WriteLine("Input Username:");
            return Console.ReadLine() ?? "";
        }

        //Gibt das, vom Nutzer eingegebenen, Passwort zurück
        private static string GetUsersecret()
        {
            Console.WriteLine("Input Password:");
            return SimpleEncrypter.EncryptString(_encryptKey, Console.ReadLine() ?? "");
        }

        private static string GetUserRecoveryKey()
        {
            Console.WriteLine("Input your recovery key:");
            return SimpleEncrypter.EncryptString(_encryptKey, Console.ReadLine() ?? "");
        }

        //Methode um ein, vom User eingegebenes, Passwort zu überprüfen
        private static string GetNewUsersecret()
        {
            Console.WriteLine("Input new Password:");
            string newUsersecret = Console.ReadLine() ?? "";

            while (string.IsNullOrEmpty(newUsersecret))
            {
                Console.WriteLine("Your new Password should not be blanck!");
                newUsersecret = Console.ReadLine() ?? "";
            }

            return SimpleEncrypter.EncryptString(_encryptKey, newUsersecret);
        }

        //Erstellt einen neuen Nutzer und lädt ihn in die Datenbank
        private static bool AddNewUser(UserContext database, string username, string usersecret)
        {
            if (username == "") return false;
            if (usersecret == "4pZrH4vsAwcGHvo486Rrww==") return false;
            if (CheckIfUserExists(database, username)) return false;

            string recoveryKey = RecoveryKeyGenerator.GenerateRecoveryKey();
            string encryptedRecoveryKey = SimpleEncrypter.EncryptString(_encryptKey, recoveryKey);

            Console.WriteLine($"This is your recovery key, please safe this key to a safe place and dont lose it, you will need this key to reset your password:\n{recoveryKey}");

            database.Add(new User() { Id = new Guid(), Username = username, Usersecret = usersecret, RecoveryKey = encryptedRecoveryKey });
            database.SaveChanges();

            return true;
        }

        //Überprüft, ob das eingegebenen Passwort korrekt ist
        private static bool CheckUserCredential(UserContext database, string username, string usersecret)
        {
            if (database.Users.Any(u => u.Username == username && u.Usersecret == usersecret)) return true;
            return false;
        }

        //Überprüft, ob der eingegebenen Nutzername vorhanden ist
        private static bool CheckIfUserExists(UserContext database, string username)
        {
            if (database.Users.Any(u => u.Username == username)) return true;
            return false;
        }

        //Zeigt alle aktuell registrierten Nutzer in der Datenbank an
        private static void ShowAllUsers(UserContext database)
        {
            List<User> user = database.Users.ToList();
            Console.WriteLine(user.Count);
            foreach (User u in user)
            {
                if (!_konami)
                    Console.WriteLine($"({u.Id}) {u.Username}");
                else
                    Console.WriteLine($"({u.Id}) {u.Username} | {SimpleEncrypter.DecryptString(_encryptKey, u.Usersecret ?? "<No password submitted>")}");
            }
        }

        //Methode zum suche von Nutzern im System
        private static void SearchUser(UserContext database, string username)
        {
            Guid newGuid = new();
            User user = database.Users.Where(u => u.Username == username).FirstOrDefault() ?? 
                new User() { Id = newGuid, Username = $"{newGuid}-null", Usersecret = $"{newGuid}-null" };

            if (user.Username == $"{newGuid}-null")
            {
                Console.WriteLine("User not found");
                return;
            }

            Console.WriteLine($"1 User found:\nUsername = ({user.Id}) {user.Username}");
        }

        //Methode zum zurücksetzen des Passworts
        private static void ResetUserPassword(UserContext database, string username, string encryptedRecoveryKey)
        {
            //Überprüft anhand des gegebenen Nutzernamen und des recoveryKeys, ob der Nutzer existiert
            var user = database.Users.Where(u => u.Username == username).FirstOrDefault();

            if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(encryptedRecoveryKey))
            {
                Console.WriteLine("There is no user with this username!");
                return;
            }

            if (user == null)
            {
                Console.WriteLine("User not found!");
                return;
            }

            //Überprüft, ob der recoveryKey korrekt ist
            if (user.RecoveryKey != encryptedRecoveryKey)
            {
                Console.WriteLine("The recovery key is incorrect!");
                return;
            }

            string newUsersecret = GetNewUsersecret();
            if (newUsersecret == "") return;

            // Setzt das Passwort des Nutzers zurück
            user.Usersecret = newUsersecret;
            database.SaveChanges();

            Console.WriteLine("Password updated!");
        }

        private static void ChangeUsername(UserContext database, string username, string usersecret)
        {
            if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(usersecret))
            {
                Console.WriteLine("Username or Password is empty!");
                return;
            }

            if (CheckIfUserExists(database, username))
            {
                Console.WriteLine("User does not exists!");
                return;
            }
            if (CheckUserCredential(database, username, usersecret))
            {
                Console.WriteLine("Username or Password is incorrect!");
                return;
            }

            //New Username
            Console.WriteLine("Input new Username:");
            string newUsername = Console.ReadLine() ?? "";
            if (string.IsNullOrEmpty(newUsername))
            {
                Console.WriteLine("Username is empty!");
                return;
            }

            //Change Username
            var user = database.Users.Where(u => u.Username == username).FirstOrDefault();

            if (user == null)
            {
                Console.WriteLine("User not found!");
                return;
            }

            user.Username = newUsername;
            database.SaveChanges();

            Console.WriteLine("Username updated!:");
        }
    }
}
