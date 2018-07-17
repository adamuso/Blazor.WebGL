using System;
using System.Runtime.Serialization;

namespace Blazor.WebGL
{
    [Serializable]
    internal class WebGLException : Exception
    {
        public WebGLException()
        {
        }

        public WebGLException(string message) : base(message)
        {
        }

        public WebGLException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WebGLException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}