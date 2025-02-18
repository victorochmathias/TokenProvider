﻿

using System.ComponentModel.DataAnnotations;

namespace TokenProvider.Infrastructure.Data.Entities;

public class RefreshTokenEntity
{
    [Key]
    public string RefreshToken { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public DateTime ExpiryDate { get; set; } = DateTime.Now.AddDays(7);
}
