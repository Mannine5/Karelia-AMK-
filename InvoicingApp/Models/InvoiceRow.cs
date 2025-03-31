using System.ComponentModel;

namespace InvoicingApp.Models
{
    // Laskurivi-luokka
    public class InvoiceRow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int? InvoiceRowId { get; set; }  // Tunniste
        public string ProductName { get; set; } = "";
        public string RowType { get; set; } = "";

        private int? productId;
        public int? ProductId
        {
            get { return productId; }
            set
            {
                if (productId != value)
                {
                    productId = value;
                    OnPropertyChanged(nameof(ProductId));
                }
            }
        }



        private int quantity;
        public int Quantity
        {
            get { return quantity; }
            set
            {
                if (quantity != value)
                {
                    quantity = value;
                    OnPropertyChanged(nameof(Quantity));

                }
            }
        }

        private decimal unitPrice;
        public decimal UnitPrice
        {
            get { return unitPrice; }
            set
            {
                if (unitPrice != value)
                {
                    unitPrice = value;
                    OnPropertyChanged(nameof(UnitPrice));
                }
            }
        }

        private decimal workHours;
        public decimal WorkHours
        {
            get { return workHours; }
            set
            {
                if (workHours != value)
                {
                    workHours = value;
                    OnPropertyChanged(nameof(WorkHours));

                }
            }
        }

        private decimal hourCost;
        public decimal HourCost
        {
            get { return hourCost; }
            set
            {
                if (hourCost != value)
                {
                    hourCost = value;
                    OnPropertyChanged(nameof(HourCost));

                }
            }
        }

        private string description = "";
        public string Description
        {
            get { return description; }
            set
            {
                if (description != value)
                {
                    description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

    }
}