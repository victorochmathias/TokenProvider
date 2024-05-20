using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TokenProvider.Infrastructure.Models;
using TokenProvider.Infrastructure.Services;

namespace TokenProvider.Functions
{
    public class GenerateTokens(ILogger<GenerateTokens> logger, ITokenService tokenService)
    {
        private readonly ILogger<GenerateTokens> _logger = logger;
        private readonly ITokenService _tokenService = tokenService;

        [Function("GenerateTokens")] // OBS INTE FÄRDIG
        public async  Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "token/generate")] HttpRequest req)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var tokenRequest = JsonConvert.DeserializeObject<TokenRequest>(body);


            if (tokenRequest == null || tokenRequest.UserId == null || tokenRequest.Email == null)
                return new BadRequestObjectResult(new { Error = "Please provide a valid user id and email address." });


            try
            {
                RefreshTokenResult refreshTokenResult = null!;
                AccessTokenResult accessTokenResult = null!;

                using var ctsTimeOut = new CancellationTokenSource(TimeSpan.FromSeconds(120 * 1000));
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ctsTimeOut.Token, req.HttpContext.RequestAborted);

                req.HttpContext.Request.Cookies.TryGetValue("refreshToken", out var resfreshToken);
                if (!string.IsNullOrEmpty(resfreshToken))
                   
                refreshTokenResult = await _tokenService.GetRefreshTokenAsync(resfreshToken, cts.Token);

                if (refreshTokenResult.ExpiryDate < DateTime.Now)
                    return new UnauthorizedObjectResult(new { Error = "Refresh token has expired." });

                if (refreshTokenResult.ExpiryDate < DateTime.Now.AddDays(1))
                    refreshTokenResult = await _tokenService.GenerateRefreshTokenAsync(tokenRequest.UserId, cts.Token);

                accessTokenResult = _tokenService.GenerateAccessToken(tokenRequest, refreshTokenResult.Token);

                if (refreshTokenResult.Token != null && refreshTokenResult.CookieOptions != null)
                    req.HttpContext.Response.Cookies.Append("refreshtoken", refreshTokenResult.Token, refreshTokenResult.CookieOptions);



                if (accessTokenResult != null && accessTokenResult.Token != null && refreshTokenResult.Token != null)
                    return new OkObjectResult(new { AccessToken = accessTokenResult.Token, RefreshToken = refreshTokenResult.Token });


            }
            catch { }

            return new UnauthorizedObjectResult(new { Error = "Refresh token has expired." });
        }
    }
}
