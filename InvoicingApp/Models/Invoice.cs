using System.Collections.ObjectModel;
using System.ComponentModel;

namespace InvoicingApp.Models
{
    // Lasku-luokka
    public class Invoice : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int? invoiceId;
        public int? InvoiceId
        {
            get { return invoiceId; }
            set
            {
                if (invoiceId != value)
                {
                    invoiceId = value;
                    OnPropertyChanged(nameof(InvoiceId));
                }
            }
        }

        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public Customer Customer { get; set; } = new Customer();
        public ObservableCollection<InvoiceRow> Rows { get; set; } = new();
        public string Notes { get; set; } = "";

        public decimal TotalPrice
        {
            get
            {
                return Rows.Sum(row => row.Quantity * row.UnitPrice + row.WorkHours * row.HourCost);
            }
        }
    }
}