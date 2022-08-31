using System;

namespace Catalina.Discord.Common;
public class Exceptions
{
    public class InvalidParameter : Exception
    {
        public InvalidParameter() : base("The input provided is invalid") {}
        public InvalidParameter(string message) : base(message) { }
    }
    public class InvalidMessageLink : Exception
    {
        public InvalidMessageLink() : base("The message link provided is invalid") { }
        public InvalidMessageLink(string message) : base(message) { }
    }
    public class InsufficientPermissions : Exception 
    {
        public InsufficientPermissions() : base("You do not have sufficient permissions to perform this action") { }
        public InsufficientPermissions(string message) : base(message) { }
    }
    public class InvalidChannel : Exception
    {
        public InvalidChannel() : base("The channel provided is invalid") { }
        public InvalidChannel(string message) : base(message) { }
    }
    public class InvalidRole : Exception
    {
        public InvalidRole() : base("The role provided is invalid") { }
        public InvalidRole(string message) : base(message) { }
    }
    public class InvalidGuild : Exception
    {
        public InvalidGuild() : base("The guild provided is invalid") { }
        public InvalidGuild(string message) : base(message) { }
    }
    public class InvalidEmote : Exception
    {
        public InvalidEmote() : base("The emote provided is invalid") { }
        public InvalidEmote(string message) : base(message) { }
    }
    public class InvalidMessage : Exception
    {
        public InvalidMessage() : base("The message provided is invalid") { }
        public InvalidMessage(string message) : base(message) { }
    }
    public class InvalidChannelForGuild : Exception
    {
        public InvalidChannelForGuild() : base("The channel provided is invalid for this guild") { }
        public InvalidChannelForGuild(string message) : base(message) { }
    }
    public class InvalidMessageForGuild : Exception
    {
        public InvalidMessageForGuild() : base("The message provided is invalid for this guild") { }
        public InvalidMessageForGuild(string message) : base(message) { }
    }
    public class InvalidEmoteForGuild : Exception
    {
        public InvalidEmoteForGuild() : base("The emote provided is invalid for this guild") { }
        public InvalidEmoteForGuild(string message) : base(message) { }
    }
    public class UnknownError : Exception
    {
        public UnknownError() : base("The emote provided is invalid for this guild") { }
        public UnknownError(string message) : base(message) { }
    }

}
