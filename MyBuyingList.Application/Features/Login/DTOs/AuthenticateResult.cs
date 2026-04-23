namespace MyBuyingList.Application.Features.Login.DTOs;

public record AuthenticateResult(int UserId, string UserName, IReadOnlyList<string> Permissions, bool IsAdmin);
