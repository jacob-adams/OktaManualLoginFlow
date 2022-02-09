using System.Runtime.Serialization;

namespace OktaManualLoginFlow.Utility
{
    [Serializable]
    internal class StateMismatchException : Exception
    {
        public StateMismatchException()
        {
        }

        public StateMismatchException(string? message) : base(message)
        {
        }

        public StateMismatchException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected StateMismatchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}