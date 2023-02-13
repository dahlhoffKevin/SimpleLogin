﻿using Microsoft.Data.Sqlite;
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
    public class SimpleLogin
    {
        //Hashkey wird genutzt, um Zeichenfolgen zu verschlüsseln
        public static readonly string _encryptKey = "b14ca5898a4e4133bbce2ea2315a1916";

        public static void Main()
        {
            using var database = new UserContext();

            while (true)
            {
                Console.WriteLine("Input Exit : 0 | Login : 1 | Register : 2 | Show All User: 3 | Search User: 4");
                bool result = int.TryParse(Console.ReadLine() ?? "1", out int start);
                //Überprüft, ob der Input vom Nutzer verwendet werden kann
                if (result == false || !new List<int>() { 0,1,2,3,4 }.Contains(start)) continue;

                switch (start)
                {
                    case 0:
                        Environment.Exit(1);
                        break;

                    case 1:
                        if (CheckUserCredential(database)) Console.WriteLine("Login successfull!\n");
                        else Console.WriteLine("Login failed! Username or Password incorrect\n");
                        break;

                    case 2:
                        if (AddNewUser(database)) Console.WriteLine("Successfully registered!\n");
                        else Console.WriteLine("Registration failed: User does already exists!\n");
                        break;

                    case 3:
                        Console.WriteLine("All User:");
                        ShowAllUsers(database);
                        Console.WriteLine("");
                        break;

                    case 4:
                        Console.WriteLine("Search User:");
                        SearchUser(database);
                        Console.WriteLine("");
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
        private static string GetPassword()
        {
            Console.WriteLine("Input Password:");
            return SimpleEncrypter.EncryptString(_encryptKey, Console.ReadLine() ?? "");
        }

        //Erstellt einen neuen Nutzer und lädt ihn in die Datenbank
        private static bool AddNewUser(UserContext database)
        {
            string username = GetUsername();
            string password = GetPassword();
            if (username == "" || password == "") return false;

            if (CheckIfUserExists(database, username)) return false;

            database.Add(new User() { Id = new Guid(), Username = username, Usersecret = password });
            database.SaveChanges();

            return true;
        }

        //Überprüft, ob das eingegebenen Passwort korrekt ist
        private static bool CheckUserCredential(UserContext database)
        {
            string username = GetUsername();
            string password = GetPassword();

            if (database.Users.Any(u => u.Username == username && u.Usersecret == password)) return true;
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
                Console.WriteLine($"({u.Id}) {u.Username}"); //{SimpleEncrypter.DecryptString(_encryptKey, u.Usersecret ?? "<No password submitted>")}
            }
        }

        //Methode zum suche von Nutzern im System
        private static void SearchUser(UserContext database)
        {
            string username = GetUsername();
            Guid newGuid = new();
            User user = database.Users.Where(u => u.Username == username).FirstOrDefault() ?? new User() { Id = newGuid, Username = $"{newGuid}-null", Usersecret = $"{newGuid}-null" };

            if (user.Username == $"{newGuid}-null")
            {
                Console.WriteLine("User not found");
                return;
            }

            Console.WriteLine($"1 User found:\nUsername = ({user.Id}) {user.Username}");
        }
    }
}