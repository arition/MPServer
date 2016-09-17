using System;
using System.Text;
using Android.Content;
using Android.Preferences;
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
                        var msgBody = new StringBuilder();
                        foreach (var smsMessage in messages)
                        {
                            if (smsMessage?.MessageBody != null)
                            {
                                msgBody.Append(smsMessage.MessageBody);
                            }
                        }
                        //var msgAddress = messages[0].OriginatingAddress;
                        var msgDate = messages[0].TimestampMillis;
                        var saltBytes = AesEncryptamajig.GenerateSalt();
                        var messageAesPassword = PreferenceManager.GetDefaultSharedPreferences(context)
                            .All["PrefMessageAesPassword"] as string;
                        var content = AesEncryptamajig.Encrypt(msgBody.ToString(), messageAesPassword, saltBytes);
                        ReceiveMessage?.Invoke(this, new MessageEventArgs(new Message
                        {
                            Salt = Convert.ToBase64String(saltBytes),
                            Content = content,
                            MessageTime = Utils.FromUnixTime(msgDate)
                        }));
                    }
                }
            }
        }
    }
}