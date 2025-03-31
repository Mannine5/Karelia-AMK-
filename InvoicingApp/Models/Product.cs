using System.ComponentModel;

namespace InvoicingApp.Models
{
    // Tuote-luokka
    public class Product : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public decimal? Price { get; set; }

        public string Description { get; set; } = "";
    }
}