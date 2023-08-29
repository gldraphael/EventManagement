﻿#nullable enable

using System.ComponentModel.DataAnnotations;
using Eventuras.Domain;

namespace Eventuras.WebApi.Controllers.Events;

public record EventInfoOptionsDto(EventInfoOptionsDto.EventInfoRegistrationPolicyDto RegistrationPolicy)
{
    public EventInfoOptions MapToEntity()
    {
        var registrationPolicy = new EventInfoOptions.EventInfoRegistrationPolicy
        {
            AllowedRegistrationEditHours = RegistrationPolicy.AllowedRegistrationEditHours,
            AllowModificationsAfterCancellationDue = RegistrationPolicy.AllowModificationsAfterCancellationDue
        };

        return new EventInfoOptions { RegistrationPolicy = registrationPolicy };
    }

    public static EventInfoOptionsDto MapFromEntity(EventInfoOptions entity)
    {
        return new EventInfoOptionsDto(new EventInfoRegistrationPolicyDto(entity.RegistrationPolicy.AllowedRegistrationEditHours,
            entity.RegistrationPolicy.AllowModificationsAfterCancellationDue));
    }

    public record EventInfoRegistrationPolicyDto(
        [Range(0, 365 * 24)] int? AllowedRegistrationEditHours = 24,
        bool AllowModificationsAfterCancellationDue = false);
};