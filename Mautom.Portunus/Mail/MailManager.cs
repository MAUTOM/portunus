using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using MailKit.Net.Smtp;
using MailKit.Security;
using Mautom.Portunus.Config;
using Microsoft.Extensions.Configuration;
using MimeKit;
using NLog;
using NLog.Fluent;

namespace Mautom.Portunus.Mail
{
    public sealed class MailManager : IDisposable
    {
        private static MailManager _instance = null!;
        public static MailManager Instance => _instance ??= new MailManager();
        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly SmtpClient _smtp;

        private readonly Thread _senderThread;
        private readonly Queue<MimeMessage> _messageQueue = new Queue<MimeMessage>();
        private readonly object _mailLock = new object();
        private int _counter = 0;
        
        private string Host { get; }
        private int Port { get; }
        private SecureSocketOptions Ssl { get; }

        private volatile bool _stop;
        
        private MailManager()
        {
            try
            {
                var smtpConfig = ConfigManager.Configuration.GetSection("SMTP");
                Host = smtpConfig.GetValue<string>("Host");
                Port = smtpConfig.GetValue<int>("Port");
                Ssl = smtpConfig.GetValue<bool>("SSL") ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
                
                _smtp = new SmtpClient();
                _smtp.Connect(Host, Port, Ssl);

                _senderThread = new Thread(SendMails)
                {
                    Name = "Portunus Mail Dispatcher Thread", 
                    Priority = ThreadPriority.BelowNormal
                };

                
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Could not initialize mail sender.");
                throw; // cannot operate without mail sending
            }
        }

        public void RunDispatcher() => _senderThread.Start();

        public void Enqueue(MimeMessage message)
        {
            lock (_mailLock)
            {
                _messageQueue.Enqueue(message);
                _log.Debug("Enqueued new mail message");
            }
        }

        private void SendMails()
        {
            _log.Info($"Mail dispatcher thread is running (#{Thread.CurrentThread.ManagedThreadId})");
            while (!_stop)
            {
                lock (_mailLock)
                {
                    if (_messageQueue.Count > 0)
                    {
                        var message = _messageQueue.Dequeue();
                        _log.Debug($"Sending e-mail message. Counter {_counter}");
                        SendMessage(message);
                        Interlocked.Increment(ref _counter);
                    }
                }
                
                Thread.Sleep(2000);
            }

            _log.Info("Mail dispatcher thread stopped.");
        }

        private void SendMessage(MimeMessage message) => _smtp.Send(message);

        public void Dispose()
        {
            _log.Debug("MailManager.Dispose() called.");
            lock (_mailLock)
            {
                _stop = true;
                _senderThread.Join();
                
                if(_messageQueue.Count > 0)
                {
                    _log.Info($"Sending out remaining queued e-mails (no: {_messageQueue.Count})");
                    
                    foreach (var msg in _messageQueue)
                    {
                        SendMessage(msg);
                    }

                    _messageQueue.Clear();
                }
                
                _smtp.Dispose();
            }
        }
    }
}