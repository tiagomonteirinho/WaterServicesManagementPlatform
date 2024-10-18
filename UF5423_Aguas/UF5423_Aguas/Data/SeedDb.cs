﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UF5423_Aguas.Data.Entities;
using UF5423_Aguas.Helpers;

namespace UF5423_Aguas.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public SeedDb(DataContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();

            await _userHelper.EnsureCreatedRoleAsync("Admin");
            await _userHelper.EnsureCreatedRoleAsync("Employee");
            await _userHelper.EnsureCreatedRoleAsync("Customer");

            var users = await _userHelper.GetAllUsersAsync();
            if (users == null || users.Count <= 1) // If only admin or no users exist
            {
                var usersToAdd = new List<(string fullName, string email, string role)>
                {
                    ("Tiago", "admin@mail", "Admin"),
                    ("Joaquim", "employee@mail", "Employee"),
                    ("Joana", "customer@mail", "Customer"),
                    ("Bruno", "customer2@mail", "Customer"),
                };

                foreach (var (fullName, email, role) in usersToAdd)
                {
                    var user = await CreateUser(fullName, email, role);
                    users.Add(user);
                }
            }

            if (!_context.Meters.Any())
            {
                CreateMeter($"DAE AS320U-150P Water Meter with Pulse Output", "Rua das Flores", users.FirstOrDefault(u => u.Email == "customer@mail"));
                CreateMeter($"DAE O45S-PL Garden Water Meter", "Rua das Cores", users.FirstOrDefault(u => u.Email == "customer2@mail"));
                await _context.SaveChangesAsync();
            }
        }

        private async Task<User> CreateUser(string fullName, string email, string role)
        {
            var user = await _userHelper.GetUserByEmailAsync(email);
            if (user == null)
            {
                user = new User
                {
                    FullName = fullName,
                    Email = email,
                    UserName = email,
                    EmailConfirmed = true,
                };

                var result = await _userHelper.RegisterUserAsync(user, "123456");
                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException($"Could not create seed user.");
                }

                await _userHelper.AddUserToRoleAsync(user, role);
                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                await _userHelper.ConfirmAccountAsync(user, token);
            }

            if (!await _userHelper.IsUserInRoleAsync(user, role))
            {
                await _userHelper.AddUserToRoleAsync(user, role);
            }

            return user;
        }

        private void CreateMeter(string name, string address, User user)
        {
            var random = new Random();
            var meter = new Meter()
            {
                Address = address,
                UserEmail = user.Email,
                User = user,
                SerialNumber = random.Next(1000000, 10000000),
            };

            _context.Meters.Add(meter);
            user.Meters.Add(meter);
        }
    }
}
