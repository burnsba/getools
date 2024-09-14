using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;

namespace Gebug64.Unfloader.Protocol.Gebug.Dto
{
    /// <summary>
    /// Container for category+command tuple.
    /// </summary>
    public record MessageCategoryCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageCategoryCommand"/> class.
        /// </summary>
        /// <param name="category">Category.</param>
        /// <param name="command">Command.</param>
        public MessageCategoryCommand(GebugMessageCategory category, int command)
        {
            Category = category;
            Command = command;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageCategoryCommand"/> class.
        /// </summary>
        /// <param name="category">Category.</param>
        /// <param name="command">Command.</param>
        public MessageCategoryCommand(GebugMessageCategory category, GebugCmdCheat command)
        {
            Category = category;
            Command = (int)command;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageCategoryCommand"/> class.
        /// </summary>
        /// <param name="category">Category.</param>
        /// <param name="command">Command.</param>
        public MessageCategoryCommand(GebugMessageCategory category, GebugCmdMemory command)
        {
            Category = category;
            Command = (int)command;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageCategoryCommand"/> class.
        /// </summary>
        /// <param name="category">Category.</param>
        /// <param name="command">Command.</param>
        public MessageCategoryCommand(GebugMessageCategory category, GebugCmdDebug command)
        {
            Category = category;
            Command = (int)command;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageCategoryCommand"/> class.
        /// </summary>
        /// <param name="category">Category.</param>
        /// <param name="command">Command.</param>
        public MessageCategoryCommand(GebugMessageCategory category, GebugCmdMeta command)
        {
            Category = category;
            Command = (int)command;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageCategoryCommand"/> class.
        /// </summary>
        /// <param name="category">Category.</param>
        /// <param name="command">Command.</param>
        public MessageCategoryCommand(GebugMessageCategory category, GebugCmdMisc command)
        {
            Category = category;
            Command = (int)command;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageCategoryCommand"/> class.
        /// </summary>
        /// <param name="category">Category.</param>
        /// <param name="command">Command.</param>
        public MessageCategoryCommand(GebugMessageCategory category, GebugCmdRamrom command)
        {
            Category = category;
            Command = (int)command;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageCategoryCommand"/> class.
        /// </summary>
        /// <param name="category">Category.</param>
        /// <param name="command">Command.</param>
        public MessageCategoryCommand(GebugMessageCategory category, GebugCmdStage command)
        {
            Category = category;
            Command = (int)command;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageCategoryCommand"/> class.
        /// </summary>
        /// <param name="category">Category.</param>
        /// <param name="command">Command.</param>
        public MessageCategoryCommand(GebugMessageCategory category, GebugCmdBond command)
        {
            Category = category;
            Command = (int)command;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageCategoryCommand"/> class.
        /// </summary>
        /// <param name="category">Category.</param>
        /// <param name="command">Command.</param>
        public MessageCategoryCommand(GebugMessageCategory category, GebugCmdChr command)
        {
            Category = category;
            Command = (int)command;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageCategoryCommand"/> class.
        /// </summary>
        /// <param name="category">Category.</param>
        /// <param name="command">Command.</param>
        public MessageCategoryCommand(GebugMessageCategory category, GebugCmdVi command)
        {
            Category = category;
            Command = (int)command;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageCategoryCommand"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public MessageCategoryCommand(IGebugMessage message)
        {
            Category = message.Category;
            Command = message.Command;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageCategoryCommand"/> class.
        /// </summary>
        /// <param name="packet">Packet.</param>
        public MessageCategoryCommand(GebugPacket packet)
        {
            Category = packet.Category;
            Command = packet.Command;
        }

        /// <summary>
        /// Message category.
        /// Size: 1 byte.
        /// </summary>
        public GebugMessageCategory Category { get; init; }

        /// <summary>
        /// Message command.
        /// Command interpretation depends on <see cref="Category"/>.
        /// Size: 1 byte.
        /// </summary>
        public int Command { get; init; }
    }
}
