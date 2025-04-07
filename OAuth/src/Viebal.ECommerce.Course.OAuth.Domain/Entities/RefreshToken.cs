namespace Viebal.ECommerce.Course.OAuth.Domain.Entities;

public class RefreshToken
{
    #region Mapping
    public long UserId { get; set; }

    public User? User { get; set; }
    #endregion

    public long Id { get; set; }

    public string? Token { get; set; }

    public DateTime ExpriesOnUtc { get; set; }
}
