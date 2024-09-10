using System;

namespace rbac.Infra.Exceptions;

public class DomainException : Exception
{
    /// <summary>
    /// 不带信息的领域异常
    /// </summary>
    public DomainException()
    {

    }

    public DomainException(string message) : base(message)
    {

    }

    public DomainException(string message,Exception innerException) : base(message,innerException)
    {
        
    }  


}
