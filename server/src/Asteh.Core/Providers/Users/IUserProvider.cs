﻿using Asteh.Core.Models;
using Asteh.Core.Models.RequestModels;

namespace Asteh.Domain.Providers.Users
{
	public interface IUserProvider
	{
		Task<IEnumerable<UserModel>> GetUsersAsync(
			CancellationToken cancellationToken = default);
		Task<IEnumerable<UserModel>> FindUsersAsync(
			FilterUserModel filter,
			CancellationToken cancellationToken = default);
		Task CreateUserAsync(
			UserCreateModel model,
			CancellationToken cancellationToken = default);
		Task UpdateUserAsync(
			int id,
			UserUpdateModel model,
			CancellationToken cancellationToken = default);
		Task DeleteUserAsync(
			int id,
			CancellationToken cancellationToken = default);
	}
}