using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace TestingBindingToLocal_EF_6
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {  
        DbCtx Db { get; }

        private ObservableCollection<Shop>? shopsCollection = new();
        /// <summary>
        /// the property to which the binding is made in xaml
        /// </summary>
        public ObservableCollection<Shop>? ShopsCollection
        {
            get => shopsCollection; 
            set 
            { 
                shopsCollection = value; 
                OnPropertyChanged(nameof(ShopsCollection));
            }
        }
        
        public MainWindow() 
        {
            Db = new DbCtx();            
            ShopsCollection = Db.Shops.Local.ToObservableCollection();
            DataContext = this;
            InitializeComponent();
        }        

        private async void Button_AsyncLoad(object sender, RoutedEventArgs e)
        {
            try
            {
                await Db.Shops.LoadAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}");
            }
        }
        private void Button_Sync(object sender, RoutedEventArgs e)
        {
            try
            {
                _ = Db.Shops.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}");
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        #endregion INotifyPropertyChanged
    }
}
