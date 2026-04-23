using MyBuyingList.Domain.Entities;

namespace MyBuyingList.Domain.Constants;

public static class Roles
{
    public const string Administrator = "Administrator";
    public const string RegularUser = "RegularUser";
    
    public static List<Role> GetValues() =>
    [
        new() { Id = 1, Name = Administrator },
        new() { Id = 2, Name = RegularUser }
    ];
}
