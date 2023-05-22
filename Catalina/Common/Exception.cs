using System;

namespace Catalina.Common;
public class Exceptions
{
    public class DuplicateEntryException : Exception
    {
        public DuplicateEntryException() : base("Entry already exists") {}

        public DuplicateEntryException(string message) : base(message) { }
    }
    public class InvalidArgumentException : Exception
    {
        public InvalidArgumentException() : base("The input provided is invalid") {}
        public InvalidArgumentException(string message) : base(message) { }
    }
    public class InvalidMessageLinkException : Exception
    {
        public InvalidMessageLinkException() : base("The message link provided is invalid") { }
        public InvalidMessageLinkException(string message) : base(message) { }
    }
    public class InsufficientPermissionsException : Exception 
    {
        public InsufficientPermissionsException() : base("You do not have sufficient permissions to perform this action") { }
        public InsufficientPermissionsException(string message) : base(message) { }
    }
    public class InvalidChannelException : Exception
    {
        public InvalidChannelException() : base("The channel provided is invalid") { }
        public InvalidChannelException(string message) : base(message) { }
    }
    public class InvalidRoleException : Exception
    {
        public InvalidRoleException() : base("The role provided is invalid") { }
        public InvalidRoleException(string message) : base(message) { }
    }
    public class InvalidGuildException : Exception
    {
        public InvalidGuildException() : base("The guild provided is invalid") { }
        public InvalidGuildException(string message) : base(message) { }
    }
    public class InvalidEmoteException : Exception
    {
        public InvalidEmoteException() : base("The emote provided is invalid") { }
        public InvalidEmoteException(string message) : base(message) { }
    }
    public class InvalidMessageException : Exception
    {
        public InvalidMessageException() : base("The message provided is invalid") { }
        public InvalidMessageException(string message) : base(message) { }
    }
    public class InvalidGuildChannelException : Exception
    {
        public InvalidGuildChannelException() : base("The channel provided is invalid for this guild") { }
        public InvalidGuildChannelException(string message) : base(message) { }
    }
    public class InvalidGuildMessagException : Exception
    {
        public InvalidGuildMessagException() : base("The message provided is invalid for this guild") { }
        public InvalidGuildMessagException(string message) : base(message) { }
    }
    public class InvalidGuildEmoteException : Exception
    {
        public InvalidGuildEmoteException() : base("The emote provided is invalid for this guild") { }
        public InvalidGuildEmoteException(string message) : base(message) { }
    }
    public class UnknownException : Exception
    {
        public UnknownException() : base("The error source cannot be determined") { }

        public UnknownException(string message) : base(message) { }
    }
    public class UnknownEmoteException : Exception
    {
        public UnknownEmoteException() : base("The emote provided is invalid for this guild") { }
        public UnknownEmoteException(string message) : base(message) { }
    }

}
