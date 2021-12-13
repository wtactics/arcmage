using System;
using Matrix.Sdk.Api.Responses.Pushers;

namespace Matrix.Sdk.Api.Events
{
    public partial class Events
    {
        public class NotificationEventArgs : EventArgs
        {
            public Notification Notification { get; set; }
        }

        public delegate void NotificationEventDelegate(object sender, NotificationEventArgs e);

        public event NotificationEventDelegate NotificationEvent;

        internal void FireNotificationEvent(Notification notif) => NotificationEvent?.Invoke(this, new NotificationEventArgs() { Notification = notif });
    }
}
