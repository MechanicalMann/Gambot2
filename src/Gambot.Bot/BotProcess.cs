using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.IO;
using Gambot.Module.Say;
using SimpleInjector;

namespace Gambot.Bot
{
    public class BotProcess
    {
        private readonly ICollection<GambotModule> _modules;
        private readonly ICollection<ICommand> _commands;
        private readonly ICollection<IListener> _listeners;
        private readonly ICollection<IResponder> _responders;
        private readonly ICollection<ITransformer> _transformers;

        private readonly IMessenger _messenger;

        private readonly ILogger _log;

        public BotProcess(ICollection<GambotModule> modules, ICollection<ICommand> commands, ICollection<IListener> listeners, ICollection<IResponder> responders, ICollection<ITransformer> transformers, IMessenger messenger, ILogger log)
        {
            _modules = modules;
            _commands = commands;
            _listeners = listeners;
            _responders = responders;
            _transformers = transformers;
            _messenger = messenger;
            _log = log;
        }

        public async Task Initialize()
        {
            _log.Info("Initializing bot process.");
            await Task.WhenAll(_modules.Select(async m => await m.Initialize()));
            _log.Debug("Modules initialized.");

            _log.Trace("Connecting to messenger...");
            var connected = await _messenger.Connect();
            if (!connected)
            {
                _log.Warn("Unable to connect to messenger.");
                return;
            }
            _log.Trace("Connected.");

            _messenger.OnMessageReceived += HandleMessage;
        }

        public async Task HandleMessage(object sender, OnMessageReceivedEventArgs e)
        {
            var message = e.Message;
            Response response = null;

            _log.Trace("Handling commands");
            foreach (var command in _commands)
            {
                try
                {
                    response = await command.Handle(message);
                    if (response != null)
                        break;
                }
                catch (Exception ex)
                {
                    _log.Error(ex, $"An error occurred while {command} was handling: {ex.Message}");
                    try
                    {
                        await _messenger.SendMessage(message.Channel, "I tried my best, but something went wrong!", false);
                    }
                    catch (Exception sendEx)
                    {
                        _log.Error(sendEx, "Additionally, encountered an error while trying to notify the channel.");
                    }
                }
            }
            if (response != null)
            {
                _log.Trace("Handled command, responding immediately.");
                await response.Send();
                return;
            }
            _log.Trace("No commands to handle.");

            _log.Trace("Processing listeners");
            await Task.WhenAll(_listeners.Select(async l =>
            {
                try { await l.Listen(message); }
                catch (Exception ex)
                {
                    _log.Error(ex, $"An error occurred while {l} was listening: {ex.Message}");
                }
            }));
            _log.Trace("Listeners have listened.");

            _log.Trace("Processing responders");
            foreach (var responder in _responders)
            {
                try
                {
                    response = await responder.Respond(message);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, $"An error occurred while trying to get a response from {responder}: {ex.Message}");
                }
                if (response != null)
                {
                    _log.Trace("Got a response");
                    break;
                }
            }
            _log.Trace("Responders have responded.");

            if (response == null)
            {
                _log.Trace("No response generated.");
                return;
            }

            _log.Trace("Processing transformers");
            foreach (var transformer in _transformers)
            {
                try
                {
                    response = await transformer.Transform(response);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, $"An error ocurred while attempting to transform the response with {transformer}: {ex.Message}");
                }
            }
            _log.Trace("Transformers have transformed");

            _log.Trace("Sending response");
            try
            {
                await response.Send();
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"An error occurred while attempting to send the response: {ex.Message}");
            }
        }

        public async Task Stop()
        {
            if (_messenger != null)
            {
                await _messenger.Disconnect();
                _messenger.Dispose();
            }
        }
    }
}