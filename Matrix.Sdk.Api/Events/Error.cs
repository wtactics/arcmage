using System;

namespace Matrix.Sdk.Api.Events
{
    public partial class Events
    {
        public class ErrorEventArgs : EventArgs
        {
            public string Message { get; set; }
        }

        public delegate void ErrorOccurred(object sender, ErrorEventArgs e);

        public event ErrorOccurred ErrorEvent;

        internal void FireErrorEvent(string err) => ErrorEvent?.Invoke(this, new ErrorEventArgs() { Message = err });
    }
}
