using System;
using System.Collections.Generic;
using System.Text;

namespace ReceiptScanner
{
    public class Receipt
    {
        public string DisplayText { get; set; }
        public string DisplayDetail { get; set; }
        public string ImagePath { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public decimal TotalCost { get; set; }
        public decimal GST { get; set; }
    }
}
