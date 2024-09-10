using System;

namespace rbac.Infra;

/// <summary>
/// 响应结果
/// </summary>
public interface IResponse
{
    //状态码
    public int Code { get; set; }

    //文本描述
    public string Msg { get; set; }
}

/// <summary>
/// 泛型响应结果
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IResponse<T> : IResponse
{
    public T Data { get; set; }
}

public class ExceptionResponse : IResponse
{
    //异常状态码
    public int Code { get ; set ; }
    //异常信息
    public string Msg { get ; set ; } = string.Empty;
    //异常路径
    public string Instance { get; set; } = string.Empty;
    //错误字典
    public Dictionary<string,object> Errors { get; set; } = new Dictionary<string, object>();    
}

