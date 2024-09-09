using System;
using SqlSugar;

namespace rbac.Repository.Base;

public class Repository<T> : SimpleClient<T> where T :class, new()
{
    public Repository(ISqlSugarClient db)
    {
        base.Context=db;        
    }

    // TODO: 等待添加更多拓展方法 

}
