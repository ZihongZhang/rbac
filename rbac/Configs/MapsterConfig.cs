using System;
using Mapster;
using rbac.CoreBusiness.Dtos;
using rbac.CoreBusiness.Dtos.AiDtos;
using rbac.CoreBusiness.Vms;
using rbac.Modals.Models;
using rbac.Modals.Models.AiRelatedModel;

namespace rbac.Configs;

public static class MapsterConfig
{
    public static void Configure()
    { 
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
        //将role转化成roleVm
        TypeAdapterConfig<Role,RoleVm>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.RoleName, src => src.RoleName)
            .Map(dest => dest.ParentRoleId, src => src.ParentRoleId)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.MenuIdList, src => (from menu in src.MenuList select menu.Id).ToList());

        //将roleVm转化成role
        TypeAdapterConfig<RoleVm,Role>
            .NewConfig()
            .Map(dest => dest.MenuList, src => src.MenuIdList.Select(id => new Menu { Id = id }).ToList());

        //将user转化为userDto
        TypeAdapterConfig<User,UserDto>
            .NewConfig()
            .Map(dest => dest.RoleIdList, src => (from role in src.RoleList select role.Id).ToList());
        
        //将userDto转化为user
        TypeAdapterConfig<UserDto,User>
            .NewConfig()
            .Map(dest => dest.RoleList, src => src.RoleIdList.Select(id => new Role{Id =id}).ToList());
        
        //将user转化为userVm
        TypeAdapterConfig<User,UserVm>.NewConfig()
            .Map(dest => dest.RoleIdList, src =>src.RoleList!=null? src.RoleList.Select(x=>x.Id).ToList():new());
        
        //userVm转化为user
        TypeAdapterConfig<UserVm,User>
            .NewConfig()
            .Map(dest => dest.RoleList, src => src.RoleIdList.Select(id => new Role { Id = id }).ToList());
        
        TypeAdapterConfig<MessageEntry,MessageEntryDto>
            .NewConfig()
            .Map(dest => dest.role, src => src.role.ToString());
    }
}


