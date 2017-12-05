
namespace ChatSharedRessource.Models
{
    using System.Collections.ObjectModel;
    // Used to observe client list modification in the UI
    public class MyObserveBoxContainer
    {
        public ObservableCollection<Client> MainClients { get; set; }

        public MyObserveBoxContainer()
        {
            MainClients = new ObservableCollection<Client>();
        }
    }
}