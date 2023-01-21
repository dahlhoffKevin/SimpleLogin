using Microsoft.Data.Sqlite;
using System;
using System.IO.Pipes;
using System.Net;
using System.Net.NetworkInformation;

namespace SimpleLogin
{
    public class SimpleLogin
    {
        public static readonly string _encryptKey = "b14ca5898a4e4133bbce2ea2315a1916";

        public static void Main()
        {
            bool main = true;
            while (main)
            {
                Console.WriteLine("Input Exit : 0 | Login : 1 | Register : 2");
                int start = short.Parse(Console.ReadLine() ?? "1");

                string username = GetUsername();
                string password = GetPassword();

                switch (start)
                {
                    case 1:
                        if (CheckUserCredential(username, password)) Console.WriteLine("Login successfull!");
                        else Console.WriteLine("Login failed! Username or Password incorrect");
                        break;

                    case 2:
                        if (!CheckIfUserExists(username))
                        {
                            AddNewUser(username, password);
                            Console.WriteLine("Successfully registered!");
                        }
                        else Console.WriteLine("An User with this Username already exists");
                        break;

                    default:
                        break;
                }
            }
        }

        private static string GetUsername()
        {
            Console.WriteLine("Input Username:");
            return Console.ReadLine() ?? "";
        }

        private static string GetPassword()
        {
            Console.WriteLine("Input Password:");
            return SimpleEncrypter.EncryptString(_encryptKey, Console.ReadLine() ?? "");
        }

        private static void AddNewUser(string username, string password)
        {
            using var database = new UserContext();
            database.Add(new User() { Id = new Guid(), Username = username, Usersecret = password });
            database.SaveChanges();
        }

        private static bool CheckIfUserExists(string username)
        {
            using var database = new UserContext();
            if (database.Users.Any(u => u.Username == username)) return true;
            return false;
        }

        private static bool CheckUserCredential(string username, string password)
        {
            using var database = new UserContext();
            if (database.Users.Any(u => u.Username == username && u.Usersecret == password)) return true;
            return false;
        }
    }
}