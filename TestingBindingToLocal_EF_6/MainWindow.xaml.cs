using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        /// к этому свойству в XAML делается привязка 
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

            //связываемся с коллекцией Local
            ShopsCollection = Db.Shops.Local.ToObservableCollection();            

            //закачиваем первую запись для наглядности, остальные кнопками
            Db.Shops.FirstOrDefault(s => s.Name == "Shop1");

            DataContext = this;
            InitializeComponent();
        }

        private void Button_Sync(object sender, RoutedEventArgs e)
        {
            try
            {
                //закачиваем следующую запись, это изменяет Local-коллекцию
                //но в главном потоке, поэтому всё ок.
                Db.Shops.FirstOrDefault(s => s.Name == "Shop2");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex}");
            }
        }

        private async void Button_Async(object sender, RoutedEventArgs e)
        {
            try
            {               
                //закачиваем следующую запись асинхронно, это изменяет Local-коллекцию через другой поток
                //поэтому получаем исключение, что SourceCollection для ICollectionView (встроенная, из DataGrid)
                //не может быть изменен из потока отличного от главного(UI)
                await Db.Shops.LoadAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}");
            }
        }

        private async void Button_Async_Dispatcher(object sender, RoutedEventArgs e)
        {
            //имитируем какую-то дополнительную работу асинхронного метода
            await Task.Delay(1000).ConfigureAwait(false);
            try
            {
                //здесь по сути происходит синхронное обращение к БД из UI
                //если запрос большой, то UI виснет;
                //если отключить БД("нет соединения"), то UI виснет ровно на 10 секунд и только потом
                //выводит исключение, что БД не доступна
                await Application.Current.Dispatcher.InvokeAsync(
                    () => Db.Shops.FirstOrDefault(s => s.Name == "Shop4"));
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
