﻿using Asteh.Core.Models;
using Asteh.Core.Services.Users;
using Asteh.Domain.Entities;
using Asteh.Domain.Repositories.Base;
using AutoFixture;
using FluentAssertions;
using NSubstitute;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Asteh.Core.UnitTests
{
	public class UserServiceTests
	{
		private readonly IUserService _sut;
		private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

		public UserServiceTests()
		{
			_sut = new UserService(_unitOfWork);
		}

		[Theory]
		[InlineData(10)]
		[InlineData(5)]
		[InlineData(0)]
		[InlineData(100)]
		public async Task GivenUsersCount_WhenGetUsersAsyncIsCalled_ThenReturnCountUsers(int usersCount)
		{
			// Arrange
			var usersResult = new Fixture()
				.CreateMany<UserEntity>(usersCount)
				.ToList();

			_unitOfWork.UserRepository.GetAllAsync().Returns(usersResult);
			// Act
			var users = await _sut.GetUsersAsync();

			// Assert
			users.Should().HaveCount(usersCount);
		}

		[Fact]
		public async Task GivenDate_WhenGetUsersAsyncIsCalled_ThenReturnDatesStringsEquivalents()
		{
			// Arrange
			const int COUNT = 10;
			var usersResult = new Fixture()
				.CreateMany<UserEntity>(COUNT)
				.ToList();
			var usersDatesResult = usersResult.Select(u => u.LastVisitDate.ToString("dd.MM.yyyy"));

			_unitOfWork.UserRepository.GetAllAsync().Returns(usersResult);
			// Act
			var users = await _sut.GetUsersAsync();
			var usersDates = users.Select(u => u.LastVisitDate);

			// Assert
			usersDates.Should().BeEquivalentTo(usersDatesResult);
		}

		[Fact]
		public async Task GivenTypes_WhenGetUsersAsyncIsCalled_ThenReturnTypesNames()
		{
			// Arrange
			const int COUNT = 10;
			var usersResult = new Fixture()
				.CreateMany<UserEntity>(COUNT)
				.ToList();
			var usersTypesResult = usersResult.Select(
				u => u.Type?.Name ?? throw new ArgumentNullException("Type couldn't be null"));

			_unitOfWork.UserRepository.GetAllAsync().Returns(usersResult);
			// Act
			var users = await _sut.GetUsersAsync();
			var usersTypes = users.Select(u => u.TypeName);

			// Assert
			usersTypes.Should().BeEquivalentTo(usersTypesResult);
		}

		[Fact]
		public async Task GivenFilter_WhenFindUsersAsyncIsCalled_ThenReturnFilteredUsers()
		{
			// Arrange
			const int COUNT = 10;
			var filter = new Fixture()
				.Build<FilterUserModel>()
				.With(u => u.TypeName, "")
				.With(u => u.BeginDate, "")
				.With(u => u.EndDate, "")
				.Create();
			var filteredUsersResult = new Fixture()
				.CreateMany<UserEntity>(COUNT)
				.ToList();

			_unitOfWork.UserRepository
				.FindByAsync(u =>
					u.Type != null &&
					u.Type.Equals(filter.TypeName) &&
					u.Name.Equals(filter.Name) &&
					u.Name.Equals(filter.Name) &&
					u.Name.Equals(filter.Name))
				.Returns(filteredUsersResult);
			// Act
			var filteredUsers = await _sut.FindUsersAsync(filter);

			// Assert
			filteredUsers.Should().BeEquivalentTo(filteredUsersResult);
		}

		[Fact]
		public async Task GivenNullFilter_WhenFindUsersAsyncIsCalled_ThenReturnFullUsers()
		{
			// Arrange
			const int COUNT = 10;
			var filteredUsersResult = new Fixture()
				.CreateMany<UserEntity>(COUNT)
				.ToList();

			_unitOfWork.UserRepository
				.FindByAsync(null!)
				.Returns(filteredUsersResult);
			// Act
			var filteredUsers = await _sut.FindUsersAsync(null!);

			// Assert
			filteredUsers.Should().BeEquivalentTo(filteredUsersResult);
		}
	}
}