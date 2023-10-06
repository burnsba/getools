using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Manage;
using Gebug64.Unfloader.Protocol.Gebug;
using Gebug64.Unfloader.Protocol.Gebug.Message;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Getools.Lib.Game.Asset.Ramrom;
using Getools.Lib.Kaitai;
using Microsoft.Extensions.Logging;

namespace Gebug64.Win.QueryTask
{
    public class DemoReplayFromPcTask : QueryTaskContext
    {
        private readonly RamromFile _ramromFile;
        private readonly string _demoFilePath;
        private readonly string _filename;
        private readonly ILogger _logger;
        private readonly IConnectionServiceProviderResolver _connectionServiceProviderResolver;

        private GebugRamromStartDemoReplayFromPcMessage? _headerMessage = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DemoReplayFromPcTask"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="connectionServiceProviderResolver">Service provider resolver.</param>
        /// <param name="demoFilePath">Path on disk of demo file.</param>
        public DemoReplayFromPcTask(ILogger logger, IConnectionServiceProviderResolver connectionServiceProviderResolver, string demoFilePath)
        {
            _logger = logger;
            _connectionServiceProviderResolver = connectionServiceProviderResolver;

            _ramromFile = RamromParser.ParseBin(demoFilePath);
            _demoFilePath = demoFilePath;

            _filename = System.IO.Path.GetFileName(_demoFilePath);

            _logger.Log(LogLevel.Information, $"Load ramrom file {_demoFilePath}");
        }

        /// <inheritdoc />
        public override bool TaskIsUnique => true;

        /// <inheritdoc />
        public override string DisplayName => $"Ramrom replay {_filename}";

        /// <inheritdoc />
        public override void Begin()
        {
            IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();
            if (object.ReferenceEquals(null, connectionServiceProvider))
            {
                throw new NullReferenceException();
            }

            _headerMessage = new GebugRamromStartDemoReplayFromPcMessage();
            _headerMessage.Header = _ramromFile.ToRamromFileStructByteArray();

            connectionServiceProvider.Subscribe(IterationMessageCallback, 0, IterationMessageFilter);

            connectionServiceProvider.SendMessage(_headerMessage);

            State = TaskState.Running;
        }

        public override void Cancel()
        {
            // There's no cancel message to send to the console, so just allow cancel.
            State = TaskState.Stopped;

            _logger.Log(LogLevel.Information, $"{nameof(DemoReplayFromPcTask)}: cancel request");
        }

        private bool IterationMessageFilter(IGebugMessage msg)
        {
            if (msg.Category == GebugMessageCategory.Ramrom
                && msg.Command == (int)GebugCmdRamrom.ReplayRequestNextIteration)
            {
                if (msg is GebugRamromIterationMessage iterMessage)
                {
                    if (iterMessage.ReplayId == _headerMessage!.MessageId)
                    {
                        // If this was previously stopped, we just got a new message so make sure state is updated.
                        State = TaskState.Running;

                        return true;
                    }
                }
            }

            return false;
        }

        private void IterationMessageCallback(IGebugMessage msg)
        {
            // Should only get here after filter callback, so it will be safe to cast.
            var iterMessage = (GebugRamromIterationMessage)msg;

            IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();
            if (object.ReferenceEquals(null, connectionServiceProvider))
            {
                throw new NullReferenceException();
            }

            var reply = new GebugRamromIterationMessage();
            reply.ReplyTo(msg);

            if (iterMessage.IterationIndex < _ramromFile.Iterations.Count)
            {
                reply.IterationData = _ramromFile.Iterations[iterMessage.IterationIndex].ToByteArray();
            }
            else
            {
                _logger.Log(LogLevel.Information, $"{nameof(DemoReplayFromPcTask)}: iteration request failed, index={iterMessage.IterationIndex} outside array length={_ramromFile.Iterations.Count}");

                State = TaskState.Stopped;

                return;
            }

            connectionServiceProvider.SendMessage(reply);

            // If this was the last message, console shouldn't be asking for any more data.
            if (iterMessage.IterationIndex == _ramromFile.Iterations.Count - 1)
            {
                _logger.Log(LogLevel.Information, $"{nameof(DemoReplayFromPcTask)}: sent last iteration, stopping now");

                State = TaskState.Stopped;
            }
        }
    }
}
