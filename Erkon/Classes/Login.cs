using System.Linq;
using System.Security.Claims;
using Erkon.Models;

namespace Erkon.Classes
{
	public class Login
	{		
		public static UserModel GetUserInfo(ClaimsPrincipal user)
		{
			if (user.Identity != null && user.Identity.IsAuthenticated)
			{
				var userModel = new UserModel()
				{
					Username = user.Claims.FirstOrDefault(x => x.Type == "preferred_username")?.Value,
					Fullname = user.Claims.FirstOrDefault(x => x.Type == "name")?.Value
				};
				return userModel;
			}

			return null;
		}

		public static bool IsAuthenticated(ClaimsPrincipal user)
		{
			return user.Identity.IsAuthenticated;
		}
	}
}
