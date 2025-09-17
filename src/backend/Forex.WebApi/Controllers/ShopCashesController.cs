namespace Forex.WebApi.Controllers;

using Forex.Application.Features.ShopCashes.Commands;
using Forex.Application.Features.ShopCashes.DTOs;
using Forex.Application.Features.ShopCashes.Queries;
using Forex.WebApi.Controllers.Common;

public class ShopCashesController
    : CrudController<ShopCashDto,
                     GetAllShopCashesQuery,
                     GetShopCashByIdQuery,
                     CreateShopCashCommand,
                     UpdateShopCashCommand,
                     DeleteShopCashCommand>;