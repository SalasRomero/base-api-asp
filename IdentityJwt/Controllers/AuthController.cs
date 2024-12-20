﻿using IdentityJwt.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityJwt.Controllers
{
	[Route("api/auth")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly UserManager<IdentityUser> _userManager;
		private readonly SignInManager<IdentityUser> _signInManager;
		private readonly IConfiguration _configuration;
		public AuthController(UserManager<IdentityUser> userManager, IConfiguration configuration, SignInManager<IdentityUser> signInManager)
		{
			_userManager = userManager;
			_configuration = configuration;
			_signInManager = signInManager;
		}

		[HttpPost("crear")]
		public async Task<ActionResult<RespuestaAunteticacion>> Create([FromBody] CredencialesUsuario credenciales)
		{
			var usuario = new IdentityUser { UserName = credenciales.Email, Email = credenciales.Email };
			var resultado = await this._userManager.CreateAsync(usuario, credenciales.Password);

			if (resultado.Succeeded)
			{
				return await this.ConstruirToken(credenciales);
			}
			else
			{
				return BadRequest(resultado.Errors);
			}
		}

		[HttpPost("login")]
		public async Task<ActionResult<RespuestaAunteticacion>> Login([FromBody] CredencialesUsuario credenciales)
		{

			var resultado = await this._signInManager.PasswordSignInAsync(credenciales.Email, credenciales.Password,
				isPersistent: false, lockoutOnFailure: false);

			if (resultado.Succeeded)
			{
				return await this.ConstruirToken(credenciales);
			}
			else
			{
				return BadRequest("Login Incorrecto.");
			}
		}
		private async Task<RespuestaAunteticacion> ConstruirToken(CredencialesUsuario credenciales)
		{
			var claims = new List<Claim>()
			{
				new Claim("email",credenciales.Email)
			};

			var usuario = await this._userManager.FindByEmailAsync(credenciales.Email);
			var claimsDB = await this._userManager.GetClaimsAsync(usuario);

			claims.AddRange(claimsDB);

			var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._configuration["jwt_key"]));
			var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

			var expiracion = DateTime.Now.AddMinutes(20);

			var token = new JwtSecurityToken(issuer: null, audience: null, claims: claims, expires: expiracion, signingCredentials: creds);

			return new RespuestaAunteticacion()
			{
				Token = new JwtSecurityTokenHandler().WriteToken(token),
				Expiracion = expiracion
			};
		}
	}
}
