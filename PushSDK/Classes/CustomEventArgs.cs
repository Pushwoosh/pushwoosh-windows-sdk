using System;

namespace PushSDK.Classes
{
    public delegate void CustomEventHandler<T>(object sender, CustomEventArgs<T> e);  

    public class CustomEventArgs<T> : EventArgs
    {
        public T Result { get; set; }
    }
}