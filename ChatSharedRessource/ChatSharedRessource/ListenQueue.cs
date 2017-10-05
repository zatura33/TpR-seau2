using System.Collections.Generic;

namespace ChatSharedRessource
{
    public class ListenQueue
    {
        private static ListenQueue instance;
        private Queue<string> MyList { get; set; }
        public ListenQueue()
        {
           MyList = new Queue<string>();
        }

        public static ListenQueue MyInstance()
        {
         
                if (instance == null)
                {
                    instance = new ListenQueue();
                }
                return instance;
            
        }
        public void AddMessage(string message)
        {
            if(message.Length < 10000)
            MyList.Enqueue(message);
        }
        public string PullMessage()
        {
            if(MyList.Count != 0)
             return MyList.Dequeue();
            return string.Empty;
        }

        public int Count()
        {
            return MyList.Count;
        }

    }
}