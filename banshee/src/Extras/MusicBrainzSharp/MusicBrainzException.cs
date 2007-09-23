using System;

namespace MusicBrainzSharp
{
    public class MusicBrainzInvalidParameterException : Exception
    {
        public MusicBrainzInvalidParameterException()
            : base("One of the parameters is invalid. The MBID may be invalid, or you may be using an illegal parameter for this resource type.")
        {
        }
    }

    public class MusicBrainzNotFoundException : Exception
    {
        public MusicBrainzNotFoundException()
            : base("Specified resource was not found. Perhaps it was merged or deleted.")
        {
        }
    }

    public class MusicBrainzUnauthorizedException : Exception
    {
        public MusicBrainzUnauthorizedException()
            : base("The client is not authorized to perform this action. You may not have authenticated, or the username or password may be incorrect.")
        {
        }
    }
}
