﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApi.Common.Exceptions;
using WebApi.Common.Settings;
using WebApi.Data;
using WebApi.Data.Entities;
using WebApi.Services.Auth.Mappers;
using WebApi.Services.Auth.Models;


namespace WebApi.Services.Auth;

public class TokenService(IOptions<JwtSettings> jwtSettings)
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(jwtSettings.Value.SigningKey));

    public string CreateToken(TokenRequest tokenRequest)
    {
        var userInfoJson = JsonConvert.SerializeObject(tokenRequest, new StringEnumConverter());

        var claims = new List<Claim>
        {
            new("UserInfo", userInfoJson),
            new("TokenClaim", "ForVerifyOnly")
        };

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddMinutes(30),
            SigningCredentials = creds,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string CreateRefreshToken(TokenRequest tokenRequest)
    {
        var userInfoJson = JsonConvert.SerializeObject(tokenRequest, new StringEnumConverter());

        var claims = new List<Claim>
        {
            new("UserInfo", userInfoJson),
            new("RFTokenClaim", "ForVerifyOnly")
        };

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(7),
            SigningCredentials = creds,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<User?> ValidateRefreshToken(string token, AppDbContext context)
    {
        if (token.IsNullOrEmpty())
        {
            throw TechGadgetException.NewBuilder()
                .WithCode(TechGadgetErrorCode.WEA_00)
                .AddReason("token", "Thiếu mã Token")
                .Build();
        }
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _key,
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            // Extract the UserInfo claim
            var userInfoJson = principal.Claims.FirstOrDefault(c => c.Type == "UserInfo")?.Value;

            if (string.IsNullOrEmpty(userInfoJson))
                throw TechGadgetException.NewBuilder()
                .WithCode(TechGadgetErrorCode.WEA_00)
                .AddReason("token", "Không có thông tin người dùng trong mã Token.")
                .Build();

            var checkClaim = principal.Claims.FirstOrDefault(c => c.Type == "RFTokenClaim" && c.Value == "ForVerifyOnly")?.Value;

            if (string.IsNullOrEmpty(checkClaim))
                throw TechGadgetException.NewBuilder()
                .WithCode(TechGadgetErrorCode.WEA_00)
                .AddReason("token", "Thiếu thông tin xác thực trong mã Token.")
                .Build();

            // Deserialize the custom user info object
            var tokenInfo = JsonConvert.DeserializeObject<TokenRequest>(userInfoJson);
            var userInfo = tokenInfo.ToUser();

            return await context.Users.FirstOrDefaultAsync(u => u.Id == userInfo!.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw TechGadgetException.NewBuilder()
                .WithCode(TechGadgetErrorCode.WEB_02)
                .AddReason("token", "Mã Token không hợp lệ.")
                .Build();
        }
    }
}
