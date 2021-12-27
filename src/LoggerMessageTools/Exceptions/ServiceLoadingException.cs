namespace LoggerMessageTools.Exceptions
{
    public class ServiceLoadingException : Exception

    {
        public ServiceLoadingException(Type serviceType) : base(serviceType.FullName)
        {}
    }
}
