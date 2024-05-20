﻿

using Microsoft.AspNetCore.Http;

namespace TokenProvider.Infrastructure.Models;

public class RefreshTokenResult
{
    public int? StatusCode { get; set; }
    public string? Token { get; set; }
    public CookieOptions? CookieOptions { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Error { get; set; }

}
