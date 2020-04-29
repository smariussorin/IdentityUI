﻿
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using SSRD.IdentityUI.Core.Data.Entities.Group;
using SSRD.IdentityUI.Core.Data.Specifications;
using SSRD.IdentityUI.Core.Interfaces.Data.Repository;
using SSRD.IdentityUI.Core.Interfaces.Services.Group;
using SSRD.IdentityUI.Core.Models.Result;
using SSRD.IdentityUI.Core.Services.Group.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SSRD.IdentityUI.Core.Services.Group
{
    internal class GroupService : IGroupService
    {
        private readonly IBaseRepository<GroupEntity> _groupRepository;

        private readonly IValidator<AddGroupRequest> _addGroupValidator;

        private readonly ILogger<GroupService> _logger;

        public GroupService(IBaseRepository<GroupEntity> groupRepository, IValidator<AddGroupRequest> addGroupValidator, ILogger<GroupService> logger)
        {
            _groupRepository = groupRepository;

            _addGroupValidator = addGroupValidator;

            _logger = logger;
        }

        public Result Add(AddGroupRequest addGroup)
        {
            ValidationResult validationResult = _addGroupValidator.Validate(addGroup);
            if(!validationResult.IsValid)
            {
                _logger.LogWarning($"Invalid AddGroupRequest model");
                return Result.Fail(validationResult.Errors);
            }

            BaseSpecification<GroupEntity> groupExistSpecification = new BaseSpecification<GroupEntity>();
            groupExistSpecification.AddFilter(x => x.Name.ToUpper() == addGroup.Name.ToUpper());

            bool groupExist = _groupRepository.Exist(groupExistSpecification);
            if(groupExist)
            {
                _logger.LogError($"Group with the same name already exist");
                return Result.Fail("group_with_name_already_exist", "Group with name already exist");
            }

            GroupEntity group = new GroupEntity(
                name: addGroup.Name);

            bool addResult = _groupRepository.Add(group);
            if(!addResult)
            {
                _logger.LogError($"Failed to add group");
                return Result.Fail("failed_to_add_group", "Failed to add group");
            }

            return Result.Ok();
        }
    }
}
