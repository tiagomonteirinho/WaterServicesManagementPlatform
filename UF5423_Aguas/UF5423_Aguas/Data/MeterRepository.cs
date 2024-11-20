﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UF5423_Aguas.Data.API;
using UF5423_Aguas.Data.Entities;
using UF5423_Aguas.Helpers;
using UF5423_Aguas.Models;

namespace UF5423_Aguas.Data
{
    //TODO: Add Authorize.
    public class MeterRepository : GenericRepository<Meter>, IMeterRepository
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;
        private readonly ITierRepository _tierRepository;

        public MeterRepository(DataContext context, IUserHelper userHelper, ITierRepository tierRepository) : base(context)
        {
            _context = context;
            _userHelper = userHelper;
            _tierRepository = tierRepository;
        }

        public async Task<Meter> GetMeterWithUserByIdAsync(int id)
        {
            return await _context.Meters
                    .Include(m => m.User)
                    .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IQueryable<Meter>> GetMetersAsync(string email)
        {
            var user = await _userHelper.GetUserByEmailAsync(email);
            if (await _userHelper.IsUserInRoleAsync(user, "Customer"))
            {
                return _context.Meters
                        .Include(m => m.User)
                        .Where(m => m.User.Email == email)
                        .OrderBy(m => m.Id);
            }

            return _context.Meters
                .Include(m => m.User)
                .OrderBy(m => m.User.FullName);
        }

        public IQueryable<MeterDto> ConvertToMeterDtoAsync(IQueryable<Meter> meters)
        {
            var meterDtos = meters.Select(m => new MeterDto
            {
                Id = m.Id,
                Address = m.Address,
                SerialNumber = m.SerialNumber,
                Consumptions = m.Consumptions,
            });

            return meterDtos;
        }

        public async Task<IQueryable<Consumption>> GetConsumptionsAsync(string email)
        {
            var user = await _userHelper.GetUserByEmailAsync(email);
            if (await _userHelper.IsUserInRoleAsync(user, "Employee"))
            {
                return _context.Consumptions
                    .Include(c => c.Meter)
                    .ThenInclude(m => m.User)
                    .OrderBy(c => c.Meter.User.FullName)
                    .ThenByDescending(c => c.Meter.Id);
            }

            return _context.Consumptions
                    .Include(c => c.Meter)
                    .ThenInclude(m => m.User)
                    .Where(c => c.Meter.User.Email == email)
                    .OrderBy(c => c.Meter.Id);
        }

        public async Task<Meter> GetMeterWithConsumptionsAsync(int id)
        {
            return await _context.Meters
                .Include(m => m.Consumptions)
                .Include(m => m.User)
                .Where(m => m.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Consumption> GetConsumptionByIdAsync(int id)
        {
            return await _context.Consumptions.FindAsync(id);
        }

        public async Task<Consumption> AddConsumptionAsync(ConsumptionViewModel model)
        {
            var meter = await GetMeterWithConsumptionsAsync(model.MeterId);
            if (meter == null)
            {
                return null;
            }

            var consumption = new Consumption
            {
                MeterId = model.MeterId,
                Meter = model.Meter,
                Date = model.Date,
                Volume = model.Volume,
                Status = "Awaiting approval",
            };

            meter.Consumptions.Add(consumption);
            await _context.SaveChangesAsync();
            return consumption;
        }

        public async Task<int> UpdateConsumptionAsync(Consumption consumption)
        {
            var meter = await _context.Meters
                .Where(m => m.Consumptions.Any(c => c.Id == c.Id))
                .FirstOrDefaultAsync();

            _context.Consumptions.Update(consumption);
            await _context.SaveChangesAsync();
            return consumption.Id;
        }

        public async Task<int> DeleteConsumptionAsync(Consumption consumption)
        {
            var meter = await _context.Meters
                .Where(m => m.Consumptions.Any(c => c.Id == c.Id))
                .FirstOrDefaultAsync();

            if (meter == null)
            {
                return 0;
            }

            _context.Consumptions.Remove(consumption);
            await _context.SaveChangesAsync();
            return meter.Id;
        }

        public async Task<Invoice> ApproveConsumption(Consumption consumption)
        {
            var tiers = _tierRepository.GetAll();
            decimal price = 0;
            var remainingVolume = consumption.Volume;
            var tierUsages = new List<TierUsage>();

            foreach (var tier in tiers)
            {
                var remainingTierVolume = Math.Min(remainingVolume, tier.VolumeLimit);
                price += remainingTierVolume * tier.UnitPrice;
                decimal tierPrice = 0;

                if (remainingTierVolume > 0)
                {
                    tierPrice += tier.UnitPrice;
                    tierUsages.Add(new TierUsage
                    {
                        TierId = tier.Id,
                        VolumeUsed = remainingTierVolume,
                        UnitPrice = tier.UnitPrice,
                        Price = remainingTierVolume * tier.UnitPrice,
                    });
                }

                remainingVolume -= remainingTierVolume;
                if (remainingVolume <= 0)
                {
                    break;
                }
            }

            var invoice = new Invoice
            {
                Consumption = consumption,
                ConsumptionId = consumption.Id,
                Price = price,
                TierUsages = tierUsages,
            };

            _context.Invoices.Add(invoice);
            consumption.Status = "Awaiting payment";
            _context.Consumptions.Update(consumption);
            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task<Invoice> GetInvoiceByConsumptionIdAsync(int id)
        {
            return await _context.Invoices
                .Include(i => i.TierUsages)
                .Include(i => i.Consumption)
                .ThenInclude(c => c.Meter)
                .FirstOrDefaultAsync(i => i.ConsumptionId == id);
        }
    }
}
