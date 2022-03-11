﻿using Asteh.Core.Models;
using Asteh.Core.Models.RequestModels;
using Asteh.Domain.Entities;
using Asteh.Domain.Repositories.Base;
using AutoMapper;

namespace Asteh.Core.Services.Users
{
	public class UserService : IUserService
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public UserService(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<IEnumerable<UserModel>> GetUsersAsync(
			CancellationToken cancellationToken = default)
		{
			var	users = await _unitOfWork.UserRepository.GetAllAsync(cancellationToken);
			var	resultUsers = _mapper.Map<IEnumerable<UserModel>>(users);
			return resultUsers;
		}

		public async Task<IEnumerable<UserModel>> FindUsersAsync(
			FilterUserModel filter,
			CancellationToken cancellationToken = default)
		{
			// TODO: Just for testing!
			await Task.Delay(5000);

			var isBeginDateNullOrWmpty = filter == null || string.IsNullOrEmpty(filter.BeginDate);
			var isEndDateNullOrWmpty = filter == null || string.IsNullOrEmpty(filter.EndDate);
			if (filter == null || (
				string.IsNullOrEmpty(filter.Name) &&
				string.IsNullOrEmpty(filter.TypeName) &&
				isBeginDateNullOrWmpty &&
				isEndDateNullOrWmpty))
			{
				return await GetUsersAsync(cancellationToken);
			}

			var isCorrectBeginDate = DateTime
				.TryParse(filter.BeginDate, out var beginDate) || isBeginDateNullOrWmpty;
			var isCorrectEndDate = DateTime
				.TryParse(filter.EndDate, out var endDate) || isEndDateNullOrWmpty;
			if (!(isCorrectBeginDate && isCorrectEndDate &&
				(beginDate < endDate || isBeginDateNullOrWmpty || isEndDateNullOrWmpty)))
			{
				throw new ArgumentException(
					$"Uncorrect range date: {filter.BeginDate} - {filter.EndDate}");
			}

			var filterUsers = await _unitOfWork.UserRepository.FindByAsync(
				d => CheckUserByFilter(
					d,
					filter.Name,
					filter.TypeName,
					isCorrectBeginDate ? null : beginDate,
					isEndDateNullOrWmpty ? null : endDate), cancellationToken);
			var resultFilteredUsers = _mapper.Map<IEnumerable<UserModel>>(filterUsers);
			return resultFilteredUsers;
		}

		private static bool CheckUserByFilter(
			UserEntity userEntity,
			string name,
			string typeName,
			DateTime? beginDate,
			DateTime? endDate)
		{
			if (userEntity == null)
			{
				throw new ArgumentNullException($"{nameof(userEntity)} couldn't be null");
			}
			else if (userEntity.Type is null)
			{
				throw new ArgumentException($"Type of the {nameof(userEntity)} couldn't be null");
			}
			return userEntity.Name.Equals(name) &&
				userEntity.Type.Name.Equals(typeName) &&
				(beginDate == null || userEntity.LastVisitDate >= beginDate) &&
				(endDate == null || userEntity.LastVisitDate <= endDate);
		}

		public async Task CreateUserAsync(
			UserCreateModel model,
			CancellationToken cancellationToken = default)
		{
			if (await _unitOfWork.UserRepository.AnyAsync(
				d => d.Login.Equals(model.Login), cancellationToken))
			{
				throw new ArgumentException($"User with email: {model.Login} is exists");
			}

			var userType = await FindSingleUserTypeAsync(model.TypeName, cancellationToken);
			var userEntity = new UserEntity
			{
				Login = model.Login,
				Name = model.Name,
				Password = model.Password,
				TypeId = userType.Id,
				LastVisitDate = DateTime.UtcNow
			};
			_unitOfWork.UserRepository.Create(userEntity);
			await _unitOfWork.SaveChangesAsync(cancellationToken);
		}

		public async Task UpdateUserAsync(
			int id,
			UserUpdateModel model,
			CancellationToken cancellationToken = default)
		{
			var user = await FindSingleUserAsync(id, cancellationToken);
			user.Name = model.Name;
			user.Password = model.Password;

			var userType = await FindSingleUserTypeAsync(model.TypeName, cancellationToken);
			user.TypeId = userType.Id;

			var isValidNewDate = DateTime.TryParse(model.LastVisitDate, out var newDate);
			if (!isValidNewDate)
			{
				throw new ArgumentException($"Invalid new lastVisitDate {model.LastVisitDate}");
			}
			user.LastVisitDate = newDate;

			_unitOfWork.UserRepository.Update(user);
			await _unitOfWork.SaveChangesAsync(cancellationToken);
		}

		private async Task<UserTypeEntity> FindSingleUserTypeAsync(
			string typeName,
			CancellationToken cancellationToken)
		{
			var userType = await _unitOfWork.UserTypeRepository
				.SingleOrDefaultAsync(d => d.Name.Equals(typeName), cancellationToken);
			if (userType is null)
			{
				throw new ArgumentException($"UserType with name: {typeName} doesn't exists");
			}
			return userType;
		}

		public async Task DeleteUserAsync(
			int id,
			CancellationToken cancellationToken = default)
		{
			var user = await FindSingleUserAsync(id, cancellationToken);
			_unitOfWork.UserRepository.Delete(user);
			await _unitOfWork.SaveChangesAsync(cancellationToken);
		}

		private async Task<UserEntity> FindSingleUserAsync(
			int id,
			CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.UserRepository
				.SingleOrDefaultAsync(d => d.Id == id, cancellationToken);
			if (user is null)
			{
				throw new ArgumentException($"User with id: {id} doesn't exists");
			}
			return user;
		}
	}
}
