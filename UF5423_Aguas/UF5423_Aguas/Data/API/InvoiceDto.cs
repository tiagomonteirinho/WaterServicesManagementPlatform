﻿namespace UF5423_Aguas.Data.API
{
    public class InvoiceDto
    {
        public int Id { get; set; }

        public decimal Price { get; set; }

        public ConsumptionDto Consumption { get; set; }

        public MeterDto Meter { get; set; }
    }
}
