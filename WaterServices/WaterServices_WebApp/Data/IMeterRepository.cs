﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaterServices_WebApp.Data.API;
using WaterServices_WebApp.Data.Entities;
using WaterServices_WebApp.Models;

namespace WaterServices_WebApp.Data
{
    public interface IMeterRepository : IGenericRepository<Meter>
    {
        Task<Meter> GetMeterWithUserByIdAsync(int id);

        Task<IQueryable<Meter>> GetMetersAsync(string email);

        List<MeterDto> ConvertToMeterDto(IEnumerable<Meter> meters);

        Task<IQueryable<Consumption>> GetConsumptionsAsync(string email);

        List<ConsumptionDto> ConvertToConsumptionDto(IEnumerable<Consumption> consumptions);

        Task<Meter> GetMeterWithAllRelatedDataAsync(int id);

        Task<Meter> GetMeterWithConsumptionsAsync(int id);

        Task<Consumption> GetConsumptionByIdAsync(int id);

        Task<Consumption> AddConsumptionAsync(ConsumptionViewModel model);

        Task<int> UpdateConsumptionAsync(Consumption consumption);

        Task<int> DeleteConsumptionAsync(Consumption consumption);

        Task<Invoice> ApproveConsumption(Consumption consumption);

        Task<Invoice> GetInvoiceByConsumptionIdAsync(int id);
    }
}
