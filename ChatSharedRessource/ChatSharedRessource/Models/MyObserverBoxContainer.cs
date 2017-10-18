using System.Collections.ObjectModel;

namespace ChatSharedRessource.Models
{
    public class MyObserveBoxContainer
    {
        public ObservableCollection<Client> MainClients { get; set; }

        public MyObserveBoxContainer()
        {
            MainClients = new ObservableCollection<Client>();
        }
    }
}