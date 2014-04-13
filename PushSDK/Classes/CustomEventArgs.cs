using System;

namespace PushSDK.Classes
{
    internal delegate void CustomEventHandler<T>(object sender, CustomEventArgs<T> e);

    internal class CustomEventArgs<T> : EventArgs
    {
        public T Result { get; set; }
    }
}