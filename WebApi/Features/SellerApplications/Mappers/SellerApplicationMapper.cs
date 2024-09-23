﻿using WebApi.Data.Entities;
using WebApi.Features.SpecificationUnits.Models;

namespace WebApi.Features.SellerApplications.Mappers;

public static class SellerApplicationMapper
{
    public static ICollection<BillingMailApplication>? ToBillingMailApplication(this ICollection<string> billingMailsRequest)
    {
        if (billingMailsRequest != null)
        {
            return billingMailsRequest
            .Select(email => new BillingMailApplication
            {
                Mail = email,
            })
            .ToList();
        }
        return null;
    }
}
