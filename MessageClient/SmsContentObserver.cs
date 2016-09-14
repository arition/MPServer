using System;
using Android.Content;
using Android.Telephony;
using Message = MessageClient.Models.Message;

namespace MessageClient
{
    public class SmsBroadcastReceiver : BroadcastReceiver
    {
        public static string SmsReceived = "android.provider.Telephony.SMS_RECEIVED";

        public class MessageEventArgs : EventArgs
        {
            public Message Message { get; }

            public MessageEventArgs(Message message)
            {
                Message = message;
            }
        }

        public event EventHandler<MessageEventArgs> ReceiveMessage;

        public override void OnReceive(Context context, Intent intent)
        {
            if (SmsReceived.Equals(intent.Action))
            {
                var bundle = intent.Extras;
                if (bundle != null)
                {
                    var pdus = (Java.Lang.Object[]) bundle.Get("pdus");
                    var messages = new SmsMessage[pdus.Length];
                    for (var i = 0; i < pdus.Length; i++)
                    {
                        #pragma warning disable 618
                        messages[i] = SmsMessage.CreateFromPdu((byte[]) pdus[i]);
                        #pragma warning restore 618
                    }
                    if (messages.Length > 0)
                    {
                        var msgBody = messages[0].MessageBody;
                        //var msgAddress = messages[0].OriginatingAddress;
                        var msgDate = messages[0].TimestampMillis;
                        ReceiveMessage?.Invoke(this, new MessageEventArgs(new Message
                        {
                            Content = msgBody,
                            MessageTime = Utils.FromUnixTime(msgDate)
                        }));
                    }
                }
            }
        }
    }
}